//--------------------------------------------------------------------------
// <copyright file="AzureRolesReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common module for validating/reading Azure roles xml
// </summary>
//--------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using Microsoft.ComputeCluster.Management;
    using Microsoft.ComputeCluster.Management.ClusterModel;
    using Microsoft.Hpc.Management.Azure;
    using Microsoft.SystemDefinitionModel;

    [Serializable]
    public class AzureRolesReadingException : Exception
    {
        public enum AzureRolesReadingError
        {
            FileNotFound,
            BadFormatXSD,
            XMLNotMatchXSD,
            RemoteFileNotFound,
            UnknownProxy,
            UnknownRole,
            RoleInMulitpleGroups,
            RoleNotInAnyGroup,
        }

        public AzureRolesReadingError Error { get; set; }

        public AzureRolesReadingException(AzureRolesReadingError error)
        {
            this.Error = error;
        }

        public AzureRolesReadingException(AzureRolesReadingError error, Exception inner)
            : base(null, inner)
        {
            this.Error = error;
        }
    }

    internal partial class RoleNames
    {
        public static Dictionary<string, RoleDefinition> RoleNameToRoleDefMapping = null;
        public static List<HashSet<RoleDefinition>> RoleGroups = null;
        public static object RoleMappingLock = new object();

        public static Dictionary<string, RoleDefinition> DefaultRowDefinitionMapping = null;

        static RoleNames()
        {
            // Set default value, in case failed to read role XML            
            DefaultRowDefinitionMapping = new Dictionary<string, RoleDefinition>();

            DefaultRowDefinitionMapping["Small"] = new RoleDefinition("Small", "Small", "S", 1, 1750, 225, 100);
            DefaultRowDefinitionMapping["Medium"] = new RoleDefinition("Medium", "Medium", "M", 2, 3500, 490, 200);
            DefaultRowDefinitionMapping["Large"] = new RoleDefinition("Large", "Large", "L", 4, 7000, 1000, 400);
            DefaultRowDefinitionMapping["ExtraLarge"] = new RoleDefinition("ExtraLarge", "Extra Large", "XL", 8, 14000, 1000, 800);

            DefaultRowDefinitionMapping["Small"].ProxyRole = DefaultRowDefinitionMapping["Medium"];
            DefaultRowDefinitionMapping["Medium"].ProxyRole = DefaultRowDefinitionMapping["Medium"];
            DefaultRowDefinitionMapping["Large"].ProxyRole = DefaultRowDefinitionMapping["Medium"];
            DefaultRowDefinitionMapping["ExtraLarge"].ProxyRole = DefaultRowDefinitionMapping["Medium"];

            RoleNameToRoleDefMapping = new Dictionary<string, RoleDefinition>(DefaultRowDefinitionMapping);

            HashSet<RoleDefinition> set = new HashSet<RoleDefinition>();
            set.Add(RoleNameToRoleDefMapping["Small"]);
            set.Add(RoleNameToRoleDefMapping["Medium"]);
            set.Add(RoleNameToRoleDefMapping["Large"]);
            set.Add(RoleNameToRoleDefMapping["ExtraLarge"]);

            RoleGroups = new List<HashSet<RoleDefinition>>();
            RoleGroups.Add(set);
        }
    }

    public static class AzureRolesReader
    {
        public const string ExcludeRole = "ExtraSmall";

        public const string AzureRoleFileName = "AzureRoles.xml";

        private const string AzureRoleTmpFileName = "AzureRolesTmp.xml";

        private const string VMSizePatternWithSSD = "^Standard_((D|G)S[0-9]+|[A-Z]+[0-9]+s)(_v[0-9]+)?$";

        private const string VMSizePatternWithRdma = "^((Standard_)?A(8|9)|Standard_[A-Z]+[0-9]+m?r(_v[0-9]+)?)$";


        internal const string AZURE_ROLE_NAMESPACE = "http://schemas.microsoft.com/HpcAzureRoleConfigurations/2013/01";

        private enum LoadRoleType
        {
            LoadLocalXML,
            LoadRemoteXML,
            LoadDefault
        }

        public static bool Initialized { get; private set; }

        /// <summary>
        /// The maximum interval between downloading AzureRoles.xml, default value is 1 day
        /// </summary>
        public readonly static TimeSpan RoleXMLDownloadMaxInterval = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// The minimum interval between downloading AzureRoles.xml, the purpose is to avoid several downloading triggered by different client almost at same time
        /// </summary>
        public readonly static TimeSpan RoleXMLDownloadMinInterval = new TimeSpan(0, 1, 0);

        /// <summary>
        /// Supply one way to overwrite the link of AzureRoles.xml, just for internal testing
        /// </summary>
        private const string RegistryKeyName = "AzureRoleXmlUri";

        public static DateTime LastDownLoadTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Current version of AzureRoles.xml
        /// </summary>
        private static Version AzureRoleXmlVersion;

        private static object UpdateAzureRolesSyncObject = new object();

        private static object RoleSizeCacheLock = new object();

        /// <summary>
        /// cache of role sizes for azure subscription
        /// </summary>
        private static IDictionary<Guid, IList<RoleDefinition>> RoleSizeCacheBasedOnSubs = new ConcurrentDictionary<Guid, IList<RoleDefinition>>();

        /// <summary>
        /// The last time to retrieve role sizes for azure subscription from RDFE
        /// </summary>
        private static IDictionary<Guid, DateTime> RoleSizeCacheLastUpdate = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// The maximum interval to retrieve role sizes for azure subscription from RDFE
        /// it means the timeout interval for local role sizes cache
        /// </summary>
        private readonly static TimeSpan RoleSizeRefreshInterval = new TimeSpan(1, 0, 0, 0);

        /// <summary>
        /// The official link for AzureRoles.xml
        /// https://go.microsoft.com/fwlink/?linkid=862603 for V5.1 or later
        /// https://go.microsoft.com/fwlink/?linkid=842286 for V5.0
        /// http://go.microsoft.com/fwlink/?LinkId=398083&clcid=0x409 for the version before 5.0
        /// </summary>
        private const string DefaultURL = "https://go.microsoft.com/fwlink/?linkid=862603";

        private static async Task<string> GetAzureRolesXmlURL()
        {
            return await HpcContext.Get().Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, RegistryKeyName, HpcContext.Get().CancellationToken, DefaultURL);
        }

        private static string azureRoleFileLocation;

        public static string AzureRoleFileLocation
        {
            get
            {
                if (string.IsNullOrEmpty(azureRoleFileLocation))
                {
                    string home = Environment.GetEnvironmentVariable(HpcConstants.CcpHome);
                    if (string.IsNullOrEmpty(home))
                    {
                        azureRoleFileLocation = Path.GetDirectoryName(Assembly.GetAssembly(typeof(AzureRolesReader)).Location);
                    }
                    else
                    {
                        azureRoleFileLocation = Path.Combine(home, "conf");
                    }
                }

                return azureRoleFileLocation;
            }
        }

        public static string DefaultAzureRoleFileLocation
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetAssembly(typeof(AzureRolesReader)).Location);
            }
        }

        public static void Reset()
        {
            AzureRolesReader.Initialized = false;
        }

        public static RoleDefinition GetAzureRoleDefinition(string headNode, string roleSize)
        {
            AzureRolesReader.LoadRoleIfNotLoaded(headNode, null, false);
            if (RoleNames.RoleNameToRoleDefMapping.ContainsKey(roleSize))
            {
                return RoleNames.RoleNameToRoleDefMapping[roleSize];
            }
            else
            {
                throw new ArgumentException("Role size {0} can not be found", roleSize);
            }
        }

        public static int GetCoresForAzureRoleDefinition(string headNode, string roleSize)
        {
            RoleDefinition def = GetAzureRoleDefinition(headNode, roleSize);
            return def.CoreNumber;
        }

        public static void ValidateXml(string definintionXml, byte[] definitionXsd)
        {
            if (string.IsNullOrEmpty(definintionXml) || !File.Exists(definintionXml))
            {
                throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.FileNotFound, new FileNotFoundException(definintionXml));
            }

            if (definitionXsd == null || definitionXsd.Length == 0)
            {
                throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.FileNotFound, new XmlSchemaException(definintionXml));
            }

            bool xmlvalid = true;
            string xmlError = null;

            // Set the validation settings.
            XmlReaderSettings settings = NewXmlReadingSetting();
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(definitionXsd);
                using (XmlReader xsdReader = XmlReader.Create(stream))
                {
                    stream = null;
                    settings.Schemas.Add(AZURE_ROLE_NAMESPACE, xsdReader);
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationEventHandler += new ValidationEventHandler(delegate (object sender, ValidationEventArgs args) { if (xmlvalid) { xmlvalid = false; xmlError = args.Message; } });

                    // Create the XmlReader object.
                    using (XmlReader reader = XmlReader.Create(definintionXml, settings))
                    {
                        // Parse the file. 
                        while (reader.Read()) ;
                    }
                }
            }
            catch (XmlSchemaException ex)
            {
                throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.BadFormatXSD, ex);
            }
            finally
            {
                stream?.Dispose();
            }

            if (!xmlvalid)
            {
                throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.XMLNotMatchXSD, new XmlSchemaException(xmlError));
            }
        }

        public static void LoadRoleDefinition(XmlDocument doc, out Dictionary<string, RoleDefinition> roleNameToRoleDefMapping, out List<HashSet<RoleDefinition>> roleGroups, out Version roleVersion)
        {
            Dictionary<string, RoleDefinition> roleMapping = new Dictionary<string, RoleDefinition>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", AZURE_ROLE_NAMESPACE);

            XmlNode roleConfigNode = doc.SelectSingleNode("//ns:RoleConfig", nsmgr);
            string version = roleConfigNode.Attributes["Version"].Value;
            roleVersion = new Version(version);

            foreach (XmlNode node in doc.SelectNodes("//ns:Roles/ns:Role", nsmgr))
            {
                string name = node.Attributes["VMSize"].Value;
                string description = string.Empty;
                if (node.Attributes["Description"] != null)
                {
                    description = node.Attributes["Description"].Value;
                }
                string abbr = name;
                if (node.Attributes["Abbreviation"] != null)
                {
                    abbr = node.Attributes["Abbreviation"].Value;
                }

                int core = Int32.Parse(node.Attributes["NumberOfCores"].Value);
                ulong memory = ulong.Parse(node.Attributes["MemorySize"].Value);
                ulong disk = ulong.Parse(node.Attributes["DiskSize"].Value);
                ulong bandwidth = ulong.Parse(node.Attributes["BandWidth"].Value);
                var supportWorkRoles = true;
                if (node.Attributes["SupportedByWorkerRoles"] != null)
                {
                    supportWorkRoles = bool.Parse(node.Attributes["SupportedByWorkerRoles"].Value);
                }

                var supportVirtualMachines = false;
                if (node.Attributes["SupportedByVirtualMachines"] != null)
                {
                    supportVirtualMachines = bool.Parse(node.Attributes["SupportedByVirtualMachines"].Value);
                }

                var supportsRdma = node.Attributes["SupportsRdma"] != null ? bool.Parse(node.Attributes["SupportsRdma"].Value) : Regex.IsMatch(name, VMSizePatternWithRdma, RegexOptions.IgnoreCase);
                var supportsSsd = node.Attributes["SupportsSSD"] != null ? bool.Parse(node.Attributes["SupportsSSD"].Value) : Regex.IsMatch(name, VMSizePatternWithSSD, RegexOptions.IgnoreCase);

                roleMapping.Add(name, new RoleDefinition(name, description, abbr, core, memory, disk, bandwidth, supportWorkRoles, supportVirtualMachines, supportsRdma, supportsSsd));
            }

            List<HashSet<RoleDefinition>> tmpRoleGroups = new List<HashSet<RoleDefinition>>();
            foreach (XmlNode node in doc.SelectNodes("//ns:CoLocationGroups/ns:Group", nsmgr))
            {
                RoleDefinition proxy;
                if (!roleMapping.TryGetValue(node.Attributes["ProxyRole"].Value, out proxy))
                {
                    throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.UnknownProxy);
                }

                HashSet<RoleDefinition> set = new HashSet<RoleDefinition>();
                foreach (XmlNode roleNode in node.SelectNodes("ns:AssociatedRole", nsmgr))
                {
                    RoleDefinition role;
                    if (!roleMapping.TryGetValue(roleNode.InnerText, out role))
                    {
                        throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.UnknownRole);
                    }
                    if (role.ProxyRole != null && role.ProxyRole != role)
                    {
                        throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.RoleInMulitpleGroups);
                    }
                    role.ProxyRole = proxy;
                    set.Add(role);
                }

                tmpRoleGroups.Add(set);
            }

            foreach (RoleDefinition role in roleMapping.Values)
            {
                if (role.ProxyRole == null && role.SupportedByWorkerRoles)
                {
                    throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.RoleNotInAnyGroup);
                }
            }

            roleNameToRoleDefMapping = roleMapping;
            roleGroups = tmpRoleGroups;
            Initialized = true;
        }

        private static void LoadRoleDefinitionXml(string definintionXml, byte[] definitionXsd, out Dictionary<string, RoleDefinition> roleNameToRoleDefMapping, out List<HashSet<RoleDefinition>> roleGroups, out Version roleVersion)
        {
            ValidateXml(definintionXml, definitionXsd);

            XmlReaderSettings settings = NewXmlReadingSetting();
            using (XmlReader reader = XmlReader.Create(definintionXml, settings))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                LoadRoleDefinition(doc, out roleNameToRoleDefMapping, out roleGroups, out roleVersion);
            }
        }

        // If the parameter valve of "headnode" is null or empty, it means that it doesn't specify the head node and it will get the cluster name from the registry.
        private static void LoadRemoteRoleDefinitionXml(string clusterConnectionString, out Dictionary<string, RoleDefinition> roleNameToRoleDefMapping, out List<HashSet<RoleDefinition>> roleGroups, out Version roleVersion)
        {
            byte[] content = null;

            try
            {
                IHpcContext context = HpcContext.Get(clusterConnectionString);
                string headnode = context.ResolveManagementNodeAsync().GetAwaiter().GetResult();
                Trace.TraceInformation("Try connect to HpcManagement Service on the head node: {0}", headnode);
                IClusterManager2 managerConnection = ManagementServicesConnectionHelper.CreateClusterManagerProxy(context, headnode);
                content = managerConnection.GetAzureRoleConfigurations();
                WcfChannelModule.DisposeWcfProxy(managerConnection);
            }
            catch (Exception ex)
            {
                throw new AzureRolesReadingException(AzureRolesReadingException.AzureRolesReadingError.RemoteFileNotFound, ex);
            }

            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream(content);
                XmlReaderSettings settings = NewXmlReadingSetting();
                using (XmlReader reader = XmlReader.Create(ms, settings))
                {
                    ms = null;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);
                    LoadRoleDefinition(doc, out roleNameToRoleDefMapping, out roleGroups, out roleVersion);
                }
            }
            finally
            {
                ms?.Dispose();
            }
        }

        /// <summary>
        /// Load role definitions 
        /// </summary>
        /// <param name="clusterConnectionString">The head node name.</param>
        /// <param name="loadLocalConfig">Whether it's going to load local defiition file. The head node loads the file form local and the other nodes load the file from the head node.</param>
        /// <param name="headNodeBeforeV4SP1">Whether the connected head node is earlier than v4sp1.</param>
        private static void LoadRoles(string clusterConnectionString, byte[] roleSchema, bool loadLocalConfig, bool headNodeBeforeV4SP1 = false)
        {
            LoadRoleType loadType = LoadRoleType.LoadDefault;
            if (loadLocalConfig && !headNodeBeforeV4SP1)
            {
                loadType = LoadRoleType.LoadLocalXML;
            }
            else if (!loadLocalConfig && !headNodeBeforeV4SP1)
            {
                loadType = LoadRoleType.LoadRemoteXML;
            }

            Version roleVersion;
            lock (RoleNames.RoleMappingLock)
            {
                switch (loadType)
                {
                    case LoadRoleType.LoadLocalXML:
                        {
                            string file = Path.Combine(AzureRoleFileLocation, AzureRolesReader.AzureRoleFileName);
                            if (!File.Exists(file))
                            {
                                file = Path.Combine(DefaultAzureRoleFileLocation, AzureRolesReader.AzureRoleFileName);
                            }

                            SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Load AzureRoles from {0}", file);
                            AzureRolesReader.LoadRoleDefinitionXml(file,
                                                            roleSchema,
                                                            out RoleNames.RoleNameToRoleDefMapping,
                                                            out RoleNames.RoleGroups,
                                                            out roleVersion);
                            AzureRoleXmlVersion = roleVersion;
                            break;
                        }
                    case LoadRoleType.LoadRemoteXML:
                        {
                            AzureRolesReader.LoadRemoteRoleDefinitionXml(clusterConnectionString, out RoleNames.RoleNameToRoleDefMapping, out RoleNames.RoleGroups, out roleVersion);
                            AzureRoleXmlVersion = roleVersion;
                            break;
                        }
                    case LoadRoleType.LoadDefault:
                        {
                            // Use the default "RoleNames.RoleNameToRoleDefMapping" and "RoleNames.RoleGroups".
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Load role definitions if not loaded 
        /// </summary>
        /// <param name="clusterConnectionString">The head node name.</param>
        /// <param name="loadLocalConfig">Whether it's going to load local defiition file. The head node loads the file form local and the other nodes load the file from the head node.</param>
        /// <param name="headNodeBeforeV4SP1">Whether the connected head node is earlier than v4sp1.</param>
        public static void LoadRoleIfNotLoaded(string clusterConnectionString, byte[] roleSchema, bool loadLocalConfig, bool headNodeBeforeV4SP1 = false)
        {
            lock (AzureRolesReader.UpdateAzureRolesSyncObject)
            {
                if (!AzureRolesReader.Initialized)
                {
                    LoadRoles(clusterConnectionString, roleSchema, loadLocalConfig, headNodeBeforeV4SP1);
                }
            }
        }

        /// <summary>
        /// Load role definitions if not loaded 
        /// </summary>
        /// <param name="roleSchema">The Azure role schema in XSD format</param>
        /// <param name="loadLocalConfig">Whether it's going to load local defiition file. The head node loads the file form local and the other nodes load the file from the head node.</param>
        /// <param name="headNodeBeforeV4SP1">Whether the connected head node is earlier than v4sp1.</param>
        public static void LoadRoleIfNotLoaded(byte[] roleSchema, bool loadLocalConfig, bool headNodeBeforeV4SP1 = false)
        {
            LoadRoleIfNotLoaded(string.Empty, roleSchema, loadLocalConfig, headNodeBeforeV4SP1);
        }

        private static XmlReaderSettings NewXmlReadingSetting()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            return settings;
        }

        /// <summary>
        /// Return supported azure nodes size by this client
        /// </summary>
        /// <returns>role group relationship with role name/description pair</returns>
        public static IDictionary<string, HashSet<KeyValuePair<string, string>>> SupportedAzureNodeSizes(string clusterConnectionString, bool beforeV4SP1, bool forceReloadRoles = false)
        {
            return GetAzureNodeSizes(clusterConnectionString, beforeV4SP1, forceReloadRoles);
        }

        /// <summary>
        /// Get roles from roleSizes which is defined in AzureRoles.xml
        /// </summary>
        /// <param name="roleSizes">role size list</param>
        /// <returns>the sub role size list which defined in AzureRoles.xml</returns>
        public static IList<RoleDefinition> GetContainsRoles(IList<RoleDefinition> roleSizes)
        {
            IList<RoleDefinition> validRoles = new List<RoleDefinition>();
            foreach (RoleDefinition role in roleSizes)
            {
                if (RoleNames.RoleNameToRoleDefMapping.ContainsKey(role.Name))
                {
                    validRoles.Add(role);
                }
            }

            return validRoles;
        }

        /// <summary>
        /// Return supported azure nodes size by this client
        /// </summary>
        /// <returns>role group relationship with role name/description pair</returns>
        public static IDictionary<string, HashSet<KeyValuePair<string, string>>> SupportedAzureNodeSizes(string clusterConnectionString, Guid subscriptionId, string thumbPrint, string serviceName, string storageName, bool forceReloadRoles = false)
        {
            IList<RoleDefinition> roles = null;
            roles = GetAzureRoleSizes(subscriptionId);
            if (roles == null)
            {
                lock (AzureRolesReader.RoleSizeCacheLock)
                {
                    roles = GetAzureRoleSizes(subscriptionId);
                    if (roles == null)
                    {
                        RetryManager retry = new RetryManager(new PeriodicRetryTimer(1000), 5);

                        while (true)
                        {
                            try
                            {
                                IHpcContext context = HpcContext.Get(clusterConnectionString);
                                string headnode = context.ResolveManagementNodeAsync().GetAwaiter().GetResult();
                                Trace.TraceInformation("Try connect to HpcManagement Service on the head node: {0}", headnode);
                                IClusterManager2 managerConnection = ManagementServicesConnectionHelper.CreateClusterManagerProxy(context, headnode);
                                roles = managerConnection.GetAzureRoleSizes(subscriptionId, thumbPrint, serviceName, storageName);
                                AddAzureRoleSizes(subscriptionId, roles);
                                WcfChannelModule.DisposeWcfProxy(managerConnection);
                                break;
                            }
                            catch (Exception ex)
                            {
                                if (retry.HasAttemptsLeft)
                                {
                                    SdmTracing.TraceWarning(Cluster.ClusterModelFacility, "Retry to connect to HN to get Azure Role sizes, retry count = {0}", retry.RetryCount);
                                    retry.WaitForNextAttempt();
                                    continue;
                                }
                                else
                                {
                                    SdmTracing.TraceError(Cluster.ClusterModelFacility, "Cannot connect to HN to get Azure Role sizes for subscription {0}, retry count = {1}, error = {2}", subscriptionId, retry.RetryCount, ex.Message);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            IDictionary<string, HashSet<KeyValuePair<string, string>>> roleGroup = new Dictionary<string, HashSet<KeyValuePair<string, string>>>();

            if (roles == null)
            {
                roleGroup = GetAzureNodeSizes(clusterConnectionString, false, forceReloadRoles);
            }
            else
            {
                // Check whether all roles are in RoleNameToRoleDefMapping
                bool needReload = roles.Any(validRole => !RoleNames.RoleNameToRoleDefMapping.ContainsKey(validRole.Name));

                if (needReload)
                {
                    SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Need resync Azure roles info with headnode");
                    AzureRolesReader.Reset();
                    AzureRolesReader.LoadRoleIfNotLoaded(clusterConnectionString, null, false);
                }

                foreach (HashSet<RoleDefinition> roleSet in RoleNames.RoleGroups)
                {
                    HashSet<KeyValuePair<string, string>> group = new HashSet<KeyValuePair<string, string>>();
                    foreach (RoleDefinition role in roleSet)
                    {
                        bool isValid = roles.Any(validRole => validRole.SupportedByWorkerRoles && validRole.Name.Equals(role.Name, StringComparison.InvariantCultureIgnoreCase));

                        if (!isValid)
                        {
                            continue;
                        }

                        group.Add(new KeyValuePair<string, string>(role.Name, RoleNames.RoleNameToRoleDefMapping[role.Name].Description));
                        roleGroup.Add(role.Name, group);
                    }
                }
            }

            return roleGroup;
        }

        public static void AddAzureRoleSizes(Guid subscriptionId, IList<RoleDefinition> roleSizes)
        {
            AzureRolesReader.RoleSizeCacheBasedOnSubs[subscriptionId] = roleSizes;
            AzureRolesReader.RoleSizeCacheLastUpdate[subscriptionId] = DateTime.Now;
        }

        public static IList<RoleDefinition> GetAzureRoleSizes(Guid subscriptionId)
        {
            IList<RoleDefinition> roleList;
            if (AzureRolesReader.RoleSizeCacheBasedOnSubs.TryGetValue(subscriptionId, out roleList))
            {
                DateTime lastUpdate;
                if (AzureRolesReader.RoleSizeCacheLastUpdate.TryGetValue(subscriptionId, out lastUpdate))
                {
                    if ((DateTime.Now - lastUpdate) < AzureRolesReader.RoleSizeRefreshInterval)
                    {
                        return roleList;
                    }
                }
            }

            return null;
        }

        public static async Task<bool> DownloadAndLoadAzureRolesXml(byte[] definitionXsd)
        {
            if (Initialized && (DateTime.Now - LastDownLoadTime) < RoleXMLDownloadMinInterval)
            {
                SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Don't need download AzureRoles.xml, as it was downloaded at {0}, less than {1}", LastDownLoadTime.ToString("s"), RoleXMLDownloadMinInterval);
                await Task.Yield();
                return false;
            }

            lock (AzureRolesReader.UpdateAzureRolesSyncObject)
            {
                if (Initialized && (DateTime.Now - LastDownLoadTime) < RoleXMLDownloadMinInterval)
                {
                    // the file should be downloaded in other thread (triggered by some other client)
                    return true;
                }

                // download file with retry
                RetryManager retry = new RetryManager(new PeriodicRetryTimer(1000), 5);
                if (!Directory.Exists(AzureRoleFileLocation))
                {
                    Directory.CreateDirectory(AzureRoleFileLocation);
                }

                string downloadedTmpFile = Path.Combine(AzureRoleFileLocation, AzureRolesReader.AzureRoleTmpFileName);
                string downloadedFile = Path.Combine(AzureRoleFileLocation, AzureRolesReader.AzureRoleFileName);

                while (true)
                {
                    try
                    {
                        WebClient client = new WebClient();
                        SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Begin to download AzureRoles.xml");
                        string xmlUrl = GetAzureRolesXmlURL().Result;
                        client.DownloadFile(new Uri(xmlUrl), downloadedTmpFile);

                        LastDownLoadTime = DateTime.Now;
                        break;
                    }
                    catch (WebException e)
                    {
                        if (retry.HasAttemptsLeft)
                        {
                            SdmTracing.TraceWarning(Cluster.ClusterModelFacility, "Retry to download AzureRoles.xml, retry count = {0}, error = {1}", retry.RetryCount, e);
                            retry.WaitForNextAttempt();
                            continue;
                        }
                        else
                        {
                            SdmTracing.TraceError(Cluster.ClusterModelFacility, "Failed to download AzureRoles.xml, retry count = {0}, error = {1}", retry.RetryCount, e);
                            // if download failed after retry, we'd better to avoid download again in short interval
                            LastDownLoadTime = DateTime.Now;
                            return false;
                        }
                    }
                }

                // validate the downloaded xml, if validation failed, need delete it, so will use default AzureRoles.xml
                try
                {
                    Dictionary<string, RoleDefinition> roleNameToRoleDefMapping;
                    List<HashSet<RoleDefinition>> roleGroups;
                    Version roleVersion;
                    LoadRoleDefinitionXml(downloadedTmpFile, definitionXsd, out roleNameToRoleDefMapping, out roleGroups, out roleVersion);

                    if (AzureRoleXmlVersion != null && AzureRoleXmlVersion >= roleVersion)
                    {
                        SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Ignore the downloaded AzureRoles.xml, as current AzureRoles.xml version is {0}, and the downloaded version is {1}", AzureRoleXmlVersion, roleVersion);
                    }
                    else
                    {
                        File.Copy(downloadedTmpFile, downloadedFile, true);
                        RoleNames.RoleGroups = roleGroups;
                        RoleNames.RoleNameToRoleDefMapping = roleNameToRoleDefMapping;
                        AzureRoleXmlVersion = roleVersion;
                        SdmTracing.TraceInfo(Cluster.ClusterModelFacility, "Download AzureRoles.xml successful, save to {0}", downloadedFile);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    SdmTracing.TraceWarning(Cluster.ClusterModelFacility, "AzureRoles.xml downloaded is invalid, error =  {0}", e);
                    return false;
                }
                finally
                {
                    try
                    {
                        File.Delete(downloadedTmpFile);
                    }
                    catch
                    {
                        // ignore this exception
                    }
                }
            }
        }

        public static IEnumerable<RoleDefinition> GetAzureRoleDefinitions(string clusterConnectionString, bool beforeV4SP1, bool forceReloadRoles = false)
        {
            try
            {
                if (forceReloadRoles)
                {
                    AzureRolesReader.LoadRoles(clusterConnectionString, null, false, beforeV4SP1);
                }
                else
                {
                    AzureRolesReader.LoadRoleIfNotLoaded(clusterConnectionString, null, false, beforeV4SP1);
                }
            }
            catch (AzureRolesReadingException ex)
            {
                // Ignore error, we will fall back to default value
                string errorMsg = ex.InnerException == null ? string.Empty : ex.InnerException.Message;
                SdmTracing.TraceWarning(Cluster.ClusterModelFacility, "Load Azure Role configuration error: {0}, detail {1}", ex.Error, errorMsg);

                // Don't ignore remoting connection error. This may cause future deployment errors. 
                // Because the roles defined on the head node may be very different with the default ones.
                if (ex.Error == AzureRolesReadingException.AzureRolesReadingError.RemoteFileNotFound)
                {
                    throw ex.InnerException;
                }
            }

            return RoleNames.RoleNameToRoleDefMapping.Values;
        }

        private static IDictionary<string, HashSet<KeyValuePair<string, string>>> GetAzureNodeSizes(string clusterConnectionString, bool beforeV4SP1, bool forceReloadRoles = false)
        {
            GetAzureRoleDefinitions(clusterConnectionString, beforeV4SP1, forceReloadRoles);

            IDictionary<string, HashSet<KeyValuePair<string, string>>> roleGroup = new Dictionary<string, HashSet<KeyValuePair<string, string>>>();
            foreach (HashSet<RoleDefinition> roleSet in RoleNames.RoleGroups)
            {
                HashSet<KeyValuePair<string, string>> group = new HashSet<KeyValuePair<string, string>>();
                foreach (RoleDefinition role in roleSet)
                {
                    if (beforeV4SP1 && !RoleNames.DefaultRowDefinitionMapping.ContainsKey(role.Name))
                    {
                        continue;
                    }

                    group.Add(new KeyValuePair<string, string>(role.Name, RoleNames.RoleNameToRoleDefMapping[role.Name].Description));
                    roleGroup.Add(role.Name, group);
                }
            }

            return roleGroup;
        }
        
        /// <summary>
        /// Get RoleDefinition from local cache
        /// </summary>
        /// <param name="subscriptionId">Azure subscription id</param>
        /// <param name="size">role size name</param>
        /// <returns>Role detail info</returns>
        public static RoleDefinition GetRoleDefinition(Guid subscriptionId, string size)
        {
            if (RoleNames.RoleNameToRoleDefMapping.ContainsKey(size))
            {
                return RoleNames.RoleNameToRoleDefMapping[size];
            }

            IList<RoleDefinition> roleList;
            if (RoleSizeCacheBasedOnSubs.TryGetValue(subscriptionId, out roleList))
            {
                foreach (RoleDefinition def in roleList)
                {
                    if (def.Name.Equals(size, StringComparison.OrdinalIgnoreCase))
                    {
                        return def;
                    }
                }
            }

            return null;
        }
    }
}
