using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the permissions that you can assign to a user for accessing and modifying a job template.</para>
    /// </summary>
    [Flags]
    public enum JobTemplateRights
    {
        /// <summary>
        ///   <para>Reserved.</para>
        /// </summary>
        None = 0x0,

        //profile specific rights
        //ReadTemplate = 0x1,
        /// <summary>
        ///   <para>Permission to edit the job template.</para>
        /// </summary>
        EditTemplate = 0x2,
        /// <summary>
        ///   <para>Permission to submit a job using the template.</para>
        /// </summary>
        SubmitJob = 0x4,

        //standard rights
        /// <summary>
        ///   <para>Permission to delete the job template.</para>
        /// </summary>
        Delete = 0x10000,
        //ReadPermissions = 0x20000,
        /// <summary>
        ///   <para>Permission to change the security descriptor.</para>
        /// </summary>
        ChangePermissions = 0x40000,
        //TakeOwnership = 0x80000,
        //Synchronize = 0x100000,

        //generic rights
        //Generic_Read = ReadProfile | ReadPermissions,
        //Generic_Write = EditProfile | Delete,
        //Generic_Execute = UseProfile | Synchronize,
        //Generic_All = Generic_Read | Generic_Write | Generic_Execute | ChangePermissions | TakeOwnership,
        /// <summary>
        ///   <para>Permission to read the job template.</para>
        /// </summary>
        Generic_Read = 0,
        /// <summary>
        ///   <para>Permission to update the job template.</para>
        /// </summary>
        Generic_Write = EditTemplate,
        /// <summary>
        ///   <para>Permission to submit a job using the template.</para>
        /// </summary>
        Generic_Execute = SubmitJob,
        /// <summary>
        ///   <para>Permission to read, update, and apply the job template.</para>
        /// </summary>
        Generic_All = Generic_Read | Generic_Write | Generic_Execute | Delete | ChangePermissions,
    }

    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a job template.</para>
    /// </summary>
    public class JobTemplatePropertyIds
    {
        /// <summary>
        ///   <para>Initializes a new instance of this class.</para>
        /// </summary>
        protected JobTemplatePropertyIds()
        {
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the job template in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The name of the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>The date and time that the job template was created.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId CreateTime
        {
            get { return StorePropertyIds.CreateTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job template was last touched.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ChangeTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para>A security descriptor that defines who can use the job template and how they can use it.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId SecurityDescriptor
        {
            get { return _SecurityDescriptor; }
        }

        /// <summary>
        ///   <para>The description of the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Description
        {
            get { return _Description; }
        }

        /// <summary>
        ///   <para>The internal job template object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TemplateObject
        {
            get { return StorePropertyIds.TemplateObject; }
        }

        /// <summary>
        ///   <para>The reason why the property returned null.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Error
        {
            get { return StorePropertyIds.Error; }
        }

        // 
        // Private members
        //

        static PropertyId _Enabled = new PropertyId(StorePropertyType.Boolean, "Enabled", PropertyIdConstants.ProfilePropertyIdStart + 1);
        static PropertyId _SecurityDescriptor = new PropertyId(StorePropertyType.String, "SecurityDescriptor", PropertyIdConstants.ProfilePropertyIdStart + 2);
        static PropertyId _Description = new PropertyId(StorePropertyType.String, "Description", PropertyIdConstants.ProfilePropertyIdStart + 3);
    }
}

