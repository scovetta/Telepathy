using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Indicates to which group the user belongs.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To schedule jobs, the user must belong to the Administrators group 
    /// or Users group. To run commands, the user must belong to the Administrators group.</para>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const AccessDenied = 0
    /// const User = 1
    /// const Admin = 2</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetUserPrivilege" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidUserPrivilege)]
    [Obsolete("Deprecated in HPC Pack 2012 with Service Pack 1 (SP1).  Use the UserRoles enumeration instead.")]
    public enum UserPrivilege
    {
        /// <summary>
        ///   <para>The user has no access privileges (the user does not belong 
        /// to the Administrators group or Users group). This enumeration member represents a value of 0.</para>
        /// </summary>
        AccessDenied = 0,
        /// <summary>
        ///   <para>The user belongs to the Users group on the head node. This enumeration member represents a value of 1.</para>
        /// </summary>
        User = 1,
        /// <summary>
        ///   <para>The user belongs to the Administrators group on the head node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Admin = 2,
        /// <summary>
        ///   <para />
        /// </summary>
        JobOperator = 4,
        /// <summary>
        ///   <para />
        /// </summary>
        JobAdministrator = 8
    }
}
