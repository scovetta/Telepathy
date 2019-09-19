// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.EchoClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The configurations for the EchoClient
    /// </summary>
    public class Config
    {
        private const string HeadNodeArg = "headnode";
        private const string RequestArg = "numberOfRequests";
        private const string TimeMSArg = "timeMS";
        private const string SizeByteArg = "sizeByte";
        private const string ResourceTypeArg = "resourceType";
        private const string MinArg = "min";
        private const string MaxArg = "max";
        private const string WarmupSecArg = "warmupSec";
        private const string DurableArg = "durable";
        private const string AsyncArg = "async";
        private const string TransportSchemeArg = "scheme";
        private const string InprocessBrokerArg = "inprocessBroker";
        private const string IsNoSessionArg = "isNoSession";
        private const string RegPathArg = "regPath";
        private const string JobTemplateArg = "jobtemplate";
        private const string PriorityArg = "priority";
        private const string NodeGroupsArg = "groups";
        private const string NodesArg = "requestedNodes";
        private const string UserNameArg = "username";
        private const string PasswordArg = "password";
        private const string InsecureArg = "insecure";
        private const string AzureQueueArg = "azureQueue";
        private const string AzureStorageConnectionStringArg = "azureStor";
        private const string ServiceNameArg = "serviceName";
        private const string ShareSessionArg = "shareSession";
        private const string SessionPoolArg = "sessionPool";
        private const string RuntimeArg = "runtime";
        private const string JobNameArg = "jobName";
        private const string EnvironmentArg = "environment";
        private const string BrokerClientArg = "brokerClient";
        private const string VerboseArg = "verbose";
        private const string FlushArg = "flush";
        private const string SizeKBRandomArg = "sizeKBRandom";
        private const string TimeMSRandomArg = "timeMSRandom";
        private const string MsgTimeoutSecArg = "msgTimeoutSec";
        private const string ParentJobIdsArg = "parentIds";
        private const string ServiceIdleSecArg = "serviceIdleSec";
        private const string ServiceHangSecArg = "serviceHangSec";
        private const string UseWindowsClientCredentialArg = "useWCC";
        private const string UseAadArg = "useAad";
        private const string TargetListArg = "targetList";

        private bool helpInfo = false;
        public bool HelpInfo
        {
            get { return helpInfo; }
        }
        private string headNode = "%CCP_SCHEDULER%";
        public string HeadNode
        {
            get { return headNode; }
        }
        private int numberOfRequest = 10;
        public int NumberOfRequest
        {
            get { return numberOfRequest; }
        }
        private int callDurationMS = 0;
        public int CallDurationMS
        {
            get { return callDurationMS; }
        }
        private long messageSizeByte = 0;
        public long MessageSizeByte
        {
            get { return messageSizeByte; }
        }
        private int minResource = 0;
        public int MinResource
        {
            get { return minResource; }
        }
        private int maxResource = 0;
        public int MaxResource
        {
            get { return maxResource; }
        }
        private string resourceType = "core";
        public string ResourceType
        {
            get { return resourceType; }
        }
        private string transportScheme = "nettcp";
        public string TransportScheme
        {
            get { return transportScheme; }
        }
        private int warmupTimeSec = 0;
        public int WarmupTimeSec
        {
            get { return warmupTimeSec; }
        }
        private string jobTemplate = null;
        public string JobTemplate
        {
            get { return jobTemplate; }
        }
        private int priority = 2000;
        public int Priority
        {
            get { return priority; }
        }
        private string nodeGroups = string.Empty;
        public string NodeGroups
        {
            get { return nodeGroups; }
        }
        private string nodes = string.Empty;
        public string Nodes
        {
            get { return nodes; }
        }
        private string username;
        public string Username
        {
            get { return username; }
        }
        private string password;
        public string Password
        {
            get { return password; }
        }
        private bool durable = false;
        public bool Durable
        {
            get { return durable; }
        }
        private bool asyncResponseHandler = false;
        public bool AsyncResponseHandler
        {
            get { return asyncResponseHandler; }
        }
        private bool inprocessBroker = false;
        public bool InprocessBroker
        {
            get { return inprocessBroker; }
        }

        private bool isNoSession = false;
        public bool IsNoSession
        {
            get { return isNoSession; }
        }

        private string regPath = string.Empty;
        public string RegPath
        {
            get { return regPath; }
        }

        private bool insecure = false;
        public bool Insecure
        {
            get { return insecure; }
        }
        private bool? azureQueue = null;
        public bool? AzureQueue
        {
            get { return azureQueue; }
        }

        private string azureStorageConnectionString = null;

        public string AzureStorageConnectionString
        {
            get
            {
                return this.azureStorageConnectionString;
            }
        } 

        private bool shareSession = false;

        public bool ShareSession
        {
            get { return shareSession; }
        }
        private bool sessionPool = false;
        public bool SessionPool
        {
            get { return sessionPool; }
        }
        private int runtime = -1;
        public int Runtime
        {
            get { return runtime; }
        }
        private string jobName = string.Empty;
        public string JobName
        {
            get { return jobName; }
        }
        private string environment = string.Empty;
        public string Environment
        {
            get { return environment; }
        }
        private int brokerClient = 1;
        public int BrokerClient
        {
            get { return brokerClient; }
        }
        private string serviceName = "CcpEchoSvc";
        public string ServiceName
        {
            get { return serviceName; }
        }
        private bool verbose = false;
        public bool Verbose
        {
            get { return verbose; }
        }
        private int flush = 0;
        public int Flush
        {
            get { return flush; }
        }
        private string sizeKBRandom = string.Empty;
        public string SizeKBRandom
        {
            get { return sizeKBRandom; }
        }
        private string timeMSRandom = string.Empty;
        public string TimeMSRandom
        {
            get { return timeMSRandom; }
        }
        private int msgTimeoutSec = 60 * 60; //by default, the message operation timeout is 60 minutes
        public int MsgTimeoutSec
        {
            get { return msgTimeoutSec; }
        }
        private string parentIds = string.Empty;
        public string ParentIds
        {
            get { return parentIds; }
        }
        private int? serviceIdleSec = null; //by default, the service idle timeout depends on that in service registration
        public int? ServiceIdleSec
        {
            get { return serviceIdleSec; }
        }
        private int? serviceHangSec = null; //by default, the service hang timeout depends on that in service registration
        public int? ServiceHangSec
        {
            get { return serviceHangSec; }
        }
        private bool useWCC = false;
        public bool UseWCC
        {
            get { return useWCC; }
        }
        public bool UseAad { get; private set; } = false;

        private List<string> targetList = null;
        public List<string> TargetList
        {
            get { return targetList; }
        }

        public Config(CmdParser parser)
        {
            helpInfo = parser.GetSwitch("?") || parser.GetSwitch("help");
            parser.TryGetArg<string>(HeadNodeArg, ref headNode);
            parser.TryGetArg<int>(RequestArg, ref numberOfRequest);
            parser.TryGetArg<int>(TimeMSArg, ref callDurationMS);
            parser.TryGetArg<long>(SizeByteArg, ref messageSizeByte);
            parser.TryGetArg<string>(ResourceTypeArg, ref resourceType);
            parser.TryGetArg<int>(MinArg, ref minResource);
            parser.TryGetArg<int>(MaxArg, ref maxResource);
            parser.TryGetArg<int>(WarmupSecArg, ref warmupTimeSec);
            parser.TryGetArg<string>(TransportSchemeArg, ref transportScheme);
            parser.TryGetArg<string>(JobTemplateArg, ref jobTemplate);
            parser.TryGetArg<int>(PriorityArg, ref priority);
            parser.TryGetArg<string>(NodeGroupsArg, ref nodeGroups);
            parser.TryGetArg<string>(NodesArg, ref nodes);
            parser.TryGetArg<string>(UserNameArg, ref username);
            parser.TryGetArg<string>(PasswordArg, ref password);
            parser.TryGetArg<bool?>(AzureQueueArg, ref azureQueue);
            parser.TryGetArg<int>(RuntimeArg, ref runtime);
            parser.TryGetArg<int>(BrokerClientArg, ref brokerClient);
            parser.TryGetArg<string>(JobNameArg, ref jobName);
            parser.TryGetArg<string>(EnvironmentArg, ref environment);
            parser.TryGetArg<string>(ServiceNameArg, ref serviceName);
            parser.TryGetArg<int>(FlushArg, ref flush);
            parser.TryGetArg<string>(TimeMSRandomArg, ref timeMSRandom);
            parser.TryGetArg<string>(SizeKBRandomArg, ref sizeKBRandom);
            parser.TryGetArg<int>(MsgTimeoutSecArg, ref msgTimeoutSec);
            parser.TryGetArg<string>(ParentJobIdsArg, ref parentIds);
            parser.TryGetArg<int?>(ServiceIdleSecArg, ref serviceIdleSec);
            parser.TryGetArg<int?>(ServiceHangSecArg, ref serviceHangSec);
            parser.TryGetArg<string>(RegPathArg, ref regPath);
            parser.TryGetArgList<string>(TargetListArg, ref targetList);
            parser.TryGetArg(AzureStorageConnectionStringArg, ref azureStorageConnectionString);

            inprocessBroker = parser.GetSwitch(InprocessBrokerArg);
            isNoSession = parser.GetSwitch(IsNoSessionArg);
            insecure = parser.GetSwitch(InsecureArg);
            durable = parser.GetSwitch(DurableArg);
            asyncResponseHandler = parser.GetSwitch(AsyncArg);
            shareSession = parser.GetSwitch(ShareSessionArg);
            sessionPool = parser.GetSwitch(SessionPoolArg);
            verbose = parser.GetSwitch(VerboseArg);
            useWCC = parser.GetSwitch(UseWindowsClientCredentialArg);
            this.UseAad = parser.GetSwitch(UseAadArg);
        }

        public void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: EchoClient.exe -headnode <HeadNode> -jobName <JobName> -serviceName <ServiceName> -numberOfRequests <10> -timeMS <0> -sizeByte <0> -resourceType:<core|node|socket|gpu> -min <N> -max <N> -scheme <http|nettcp|custom> -groups <nodeGroupA,nodeGroupB> -requestedNodes <NodeA,NodeB> -priority <N> -jobTemplate <templateA> -environment <Environment> -username <Username> -password <Password> -azureQueue <True|False> -runtime <N Sec> -brokerClient <N> -flush <N> -timeMSRandom <N>_<N> -sizeKBRandom <N>_<N> -msgTimeoutSec <N> -parentIds <id,id,...> -serviceIdleSec <N> -serviceHangSec <N> -regPath <RegistrationFolderPath> -targetList <machine,machine,...> -azureStor <StorageConnectionString> -durable -insecure -async -inprocessBroker -isNoSession -useWCC -useAad -shareSession -sessionPool -verbose");
            Console.WriteLine();
            Console.WriteLine("Usage: EchoClient.exe /headnode:<HeadNode> /jobName:<JobName> /serviceName:<ServiceName> /numberOfRequests:<10> /timeMS:<0> /sizeByte:<0> /resourceType:<core|node|socket|gpu> /min:<N> /max:<N> /scheme:<http|nettcp|custom> /groups:<nodeGroupA,nodeGroupB> /requestedNodes:<NodeA,NodeB> /priority:<N> /jobTemplate:<templateA> /environment:<Environment> /username:<Username> /password:<Password> /azureQueue:<True|False> /runtime:<N Sec> /brokerClient:<N> /flush:<N> /timeMSRandom:<N>_<N> /sizeKBRandom:<N>_<N> /msgTimeoutSec:<N> /parentIds:<id,id,...> /serviceIdleSec:<N> /serviceHangSec:<N> /regPath:<RegistrationFolderPath> /targetList:<machine,machine,...> /azureStor:<StorageConnectionString> /durable /insecure /async /inprocessBroker /isNoSession /useWCC /useAad /shareSession /sessionPool /verbose");
            Console.WriteLine();
            Console.WriteLine("Sample: EchoClient.exe");
            Console.WriteLine("Sample: EchoClient.exe -h HeadNode -n 20");
            Console.WriteLine("Sample: EchoClient.exe -h HeadNode -n 50 -min 10 -max 10 -async -v");
            Console.WriteLine("Sample: EchoClient.exe -h HeadNode -n 100 -time 5 -size 10 -res node -min 10 -max 20 -durable");
            Console.WriteLine("Sample: EchoClient.exe -h HeadNode.cloudapp.net -scheme http -n 1000 -async");
        }

        public bool PrintUsedParams(CmdParser parser)
        {
            bool used = false;
            Dictionary<string, string> usedArgs;
            List<string> usedSwitches;
            parser.Used(out usedArgs, out usedSwitches);
            if (usedArgs != null && usedArgs.Count > 0)
            {
                foreach (var kv in usedArgs)
                {
                    Console.WriteLine("Parameter : {0}=\"{1}\"", kv.Key, kv.Value);
                }
                used = true;
            }
            if (usedSwitches != null && usedSwitches.Count > 0)
            {
                foreach (string s in usedSwitches)
                {
                    Console.WriteLine("Switch : -{0}", s);
                }
                used = true;
            }
            return used;
        }

        public bool PrintUnusedParams(CmdParser parser)
        {
            bool anyUnused = false;
            Dictionary<string, string> unusedArgs;
            List<string> unusedSwitches;
            parser.Unused(out unusedArgs, out unusedSwitches);
            ConsoleColor prevFGColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            if (unusedArgs != null && unusedArgs.Count > 0)
            {
                foreach (var kv in unusedArgs)
                {
                    Console.WriteLine("Error : Unidentified parameter {0}=\"{1}\"", kv.Key, kv.Value);
                }
                anyUnused = true;
            }
            if (unusedSwitches != null && unusedSwitches.Count > 0)
            {
                foreach (string s in unusedSwitches)
                {
                    Console.WriteLine("Error : Unidentified switch -{0}", s);
                }
                anyUnused = true;
            }
            Console.ForegroundColor = prevFGColor;
            return anyUnused;
        }
    }
}
