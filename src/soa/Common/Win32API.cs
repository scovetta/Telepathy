using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    internal enum COMPUTER_NAME_FORMAT : int
    {
        ComputerNameNetBIOS,
        ComputerNameDnsHostname,
        ComputerNameDnsDomain,
        ComputerNameDnsFullyQualified,
        ComputerNamePhysicalNetBIOS,
        ComputerNamePhysicalDnsHostname,
        ComputerNamePhysicalDnsDomain,
        ComputerNamePhysicalDnsFullyQualified,
        ComputerNameMax
    };

    internal enum SC_MANAGER_ACCESS : int
    {
        SC_MANAGER_ALL_ACCESS = 0xF003F,
        SC_MANAGER_CREATE_SERVICE = 0x0002,
        SC_MANAGER_CONNECT = 0x0001,
        SC_MANAGER_ENUMERATE_SERVICE = 0x0004,
        SC_MANAGER_LOCK = 0x0008,
        SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020,
        SC_MANAGER_QUERY_LOCK_STATUS = 0x0010,
        STANDARD_RIGHTS_READ = 0x00020000,
        GENERIC_READ = STANDARD_RIGHTS_READ | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_QUERY_LOCK_STATUS,
        GENERIC_ALL = SC_MANAGER_ALL_ACCESS
    };
    
    internal enum WIN32_ERRORS : int
    {
        ERROR_DEPENDENCY_NOT_FOUND = 5002
    };

    internal enum CLUSTER_ENUM_RESULT : uint
    {
        ERROR_SUCCESS=0,
        ERROR_NO_MORE_ITEMS=259,
        ERROR_MORE_DATA=234,
        WAIT_TIMEOUT=258  
    };

    internal enum CLUSTER_NODE_ENUM_TYPE : uint
    {
        CLUSTER_NODE_ENUM_NETINTERFACES = 0x00000001,
        CLUSTER_NODE_ENUM_GROUPS = 0x00000002,
        CLUSTER_NODE_ENUM_ALL = (CLUSTER_NODE_ENUM_NETINTERFACES | CLUSTER_NODE_ENUM_GROUPS)
    };


    internal enum CLUSTER_ENUM_TYPE : uint
    {
        CLUSTER_ENUM_NODE = 0x00000001,
        CLUSTER_ENUM_RESTYPE = 0x00000002,
        CLUSTER_ENUM_RESOURCE = 0x00000004,
        CLUSTER_ENUM_GROUP = 0x00000008,
        CLUSTER_ENUM_NETWORK = 0x00000010,
        CLUSTER_ENUM_NETINTERFACE = 0x00000020,
        CLUSTER_ENUM_INTERNAL_NETWORK = 0x80000000,
        CLUSTER_ENUM_ALL = (CLUSTER_ENUM_NODE |
                           CLUSTER_ENUM_RESTYPE |
                           CLUSTER_ENUM_RESOURCE |
                           CLUSTER_ENUM_GROUP |
                           CLUSTER_ENUM_NETWORK |
                           CLUSTER_ENUM_NETINTERFACE)
    };

    internal enum CLUSTER_CHANGE : uint
    {
        CLUSTER_CHANGE_NODE_STATE = 0x00000001,
        CLUSTER_CHANGE_NODE_DELETED = 0x00000002,
        CLUSTER_CHANGE_NODE_ADDED = 0x00000004,
        CLUSTER_CHANGE_NODE_PROPERTY = 0x00000008,
        CLUSTER_CHANGE_REGISTRY_NAME = 0x00000010,
        CLUSTER_CHANGE_REGISTRY_ATTRIBUTES = 0x00000020,
        CLUSTER_CHANGE_REGISTRY_VALUE = 0x00000040,
        CLUSTER_CHANGE_REGISTRY_SUBTREE = 0x00000080,
        CLUSTER_CHANGE_RESOURCE_STATE = 0x00000100,
        CLUSTER_CHANGE_RESOURCE_DELETED = 0x00000200,
        CLUSTER_CHANGE_RESOURCE_ADDED = 0x00000400,
        CLUSTER_CHANGE_RESOURCE_PROPERTY = 0x00000800,
        CLUSTER_CHANGE_GROUP_STATE = 0x00001000,
        CLUSTER_CHANGE_GROUP_DELETED = 0x00002000,
        CLUSTER_CHANGE_GROUP_ADDED = 0x00004000,
        CLUSTER_CHANGE_GROUP_PROPERTY = 0x00008000,
        CLUSTER_CHANGE_RESOURCE_TYPE_DELETED = 0x00010000,
        CLUSTER_CHANGE_RESOURCE_TYPE_ADDED = 0x00020000,
        CLUSTER_CHANGE_RESOURCE_TYPE_PROPERTY = 0x00040000,
        CLUSTER_CHANGE_CLUSTER_RECONNECT = 0x00080000,
        CLUSTER_CHANGE_NETWORK_STATE = 0x00100000,
        CLUSTER_CHANGE_NETWORK_DELETED = 0x00200000,
        CLUSTER_CHANGE_NETWORK_ADDED = 0x00400000,
        CLUSTER_CHANGE_NETWORK_PROPERTY = 0x00800000,
        CLUSTER_CHANGE_NETINTERFACE_STATE = 0x01000000,
        CLUSTER_CHANGE_NETINTERFACE_DELETED = 0x02000000,
        CLUSTER_CHANGE_NETINTERFACE_ADDED = 0x04000000,
        CLUSTER_CHANGE_NETINTERFACE_PROPERTY = 0x08000000,
        CLUSTER_CHANGE_QUORUM_STATE = 0x10000000,
        CLUSTER_CHANGE_CLUSTER_STATE = 0x20000000,
        CLUSTER_CHANGE_CLUSTER_PROPERTY = 0x40000000,
        CLUSTER_CHANGE_HANDLE_CLOSE = 0x80000000,
        CLUSTER_CHANGE_ALL = 0xffffffff
    };

    internal enum CLUSTER_GROUP_STATE : int
    {
        CLUSTER_GROUP_STATE_UNKNOWN=-1,
        CLUSTER_GROUP_ONLINE=0,
        CLUSTER_GROUP_OFFLINE=1,
        CLUSTER_GROUP_FAILED=2,
        CLUSTER_GROUP_PARTIAL_ONLINE=3,
        CLUSTER_GROUP_PENDING=4
    };

    internal enum CLUSTER_GROUP_ENUM : uint
    {
        CLUSTER_GROUP_ENUM_CONTAINS   = 1,
        CLUSTER_GROUP_ENUM_NODES      = 2,
        CLUSTER_GROUP_ENUM_ALL        = 3 
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct CLUS_NETNAME_VS_TOKEN_INFO 
    {
        public uint ProcessID;
        public uint DesiredAccess;
        public bool InheritHandle;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        internal static int GetSize()
        {
            return 4 + 4 + 4;
        }
    };

    internal enum CLUSCTL_RESOURCE_CODES : uint
    {
        CLUSCTL_RESOURCE_NETNAME_GET_VIRTUAL_SERVER_TOKEN = 0x0100016d
    };

    internal enum ClusterState : uint
    {
        ClusterStateNotInstalled = 0,
        ClusterStateNotConfigured = 1,
        ClusterStateNotRunning = 3,
        ClusterStateRunning = 19
    };

    // GetVersionEx is used in validation of credentials when a new .Net remoting connection is received
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct OSVERSIONINFOEX
    {
        internal uint dwOSVersionInfoSize;
        internal uint dwMajorVersion;
        internal uint dwMinorVersion;
        internal uint dwBuildNumber;
        internal uint dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal string szCSDVersion;
        internal ushort wServicePackMajor;
        internal ushort wServicePackMinor;
        internal ushort wSuiteMask;
        internal byte wProductType;
        internal byte wReserved;
    }

    internal static class Win32API
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        static public int MAX_HOST_NAME_LEN = 1024;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        static public int MAX_COMPUTERNAME_LENGTH = 15;

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenCluster(string clusterName);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseCluster(IntPtr hCluster);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr ClusterOpenEnum(IntPtr hCluster, uint type);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern uint ClusterCloseEnum(IntPtr hClusterEnum);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr CreateClusterNotifyPort(IntPtr hChange, IntPtr hCluster, uint filter, IntPtr key);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseClusterNotifyPort(IntPtr hClusterPort);

        [DllImport("clusapi.dll", SetLastError = true, CharSet=CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int GetClusterNotify(IntPtr hChange, out IntPtr key, out uint filterType, 
                    [Out] StringBuilder name, ref int nameLen, uint milliseconds);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterEnum(IntPtr hClusterEnum, int index, out uint type, [Out] StringBuilder name, ref int nameLen);
    
        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenClusterGroup(IntPtr hCluster, string groupName);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern CLUSTER_GROUP_STATE GetClusterGroupState(IntPtr hGroup, StringBuilder nodeName, ref int nodeNameLen);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr ClusterGroupOpenEnum(IntPtr hGroup, int type);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterGroupEnum(IntPtr hGroup, int index, out int type, [Out] StringBuilder resourceName, ref int resourceNameLen);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseClusterGroup(IntPtr hGroup);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr ClusterResourceOpenEnum(IntPtr hGroup, uint type);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterResourceEnum(IntPtr hResEnum, int index, out int type, [Out] StringBuilder resourceName, ref int resourceNameLen);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")][System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool GetClusterResourceNetworkName(IntPtr hResource, StringBuilder networkName, ref int networkNameLen);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern uint ClusterResourceCloseEnum(IntPtr hResEnum);

        [DllImport("clusapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenClusterResource(IntPtr hCluster, string resourceName);

        [DllImport("clusapi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseClusterResource(IntPtr hResource);

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, int access);

        [DllImport("Advapi32.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseServiceHandle(IntPtr hService);

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenService(IntPtr hSCManager, string serviceName, int access);

        [DllImport("ResUtils.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool ResUtilResourceTypesEqual(string resourceTypeName, IntPtr hResource);

        [DllImport("ClusApi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "ClusterResourceControl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterResourceControl_NetNameToken(IntPtr hResource, IntPtr hClusterNode, int controlCode,
            [In] ref CLUS_NETNAME_VS_TOKEN_INFO inBuffer, int inBufferSize, out IntPtr outBuffer, int outBufferSize, out int bytesReturned);

        [DllImport("ClusApi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr OpenClusterNode(IntPtr hCluster, String nodeName);

        [DllImport("ClusApi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterNodeEnum(IntPtr hNodeEnum, int index, out int type, [Out]StringBuilder name, ref int nameLen);

        [DllImport("ClusApi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool CloseClusterNode(IntPtr hClusterNode);

        [DllImport("ClusApi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern uint GetNodeClusterState(string nodeName, out int clusterState);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern bool GetComputerNameEx(int nameType, [Out] StringBuilder computerName, ref int length);

        [DllImport("ClusApi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern IntPtr ClusterNodeOpenEnum(IntPtr hNode, int type);

        [DllImport("ClusApi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterNodeGetEnumCount(IntPtr hNodeEnum);

        [DllImport("ClusApi.dll", SetLastError = true)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static extern int ClusterNodeCloseEnum(IntPtr hNodeEnum);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public extern static bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Returns whether the BN has Windows Failover Cluster enabled. This is done by checking if the 
        /// failover cluster service is installed. We cant just open a connection to the cluster because
        /// it may be down when the broker service is started.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared source file")]
        public static bool IsFailoverBrokerNode()
        {
            IntPtr hSCManager = IntPtr.Zero;
            IntPtr hService = IntPtr.Zero;
            bool result = false;

            try
            {
                hSCManager = Win32API.OpenSCManager(null, null, (int)SC_MANAGER_ACCESS.SC_MANAGER_ENUMERATE_SERVICE);
                if (hSCManager != IntPtr.Zero)
                {
                    hService = Win32API.OpenService(hSCManager, "ClusSvc", (int)SC_MANAGER_ACCESS.GENERIC_READ);
                    if (hService != IntPtr.Zero)
                    {
                        result = true;
                    }
                }
            }

            finally
            {
                if (hService != IntPtr.Zero)
                    Win32API.CloseServiceHandle(hService);

                if (hSCManager != IntPtr.Zero)
                    Win32API.CloseServiceHandle(hSCManager);
            }

            return result;
        }
    }
}
