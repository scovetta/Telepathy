using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Contains the file version information for the HPC server assembly.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetServerVersion" /> method.</para>
    ///   <para>This interface contains the same information as the <see cref="System.Version" /> class. </para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIServerVersion)]
    public interface IServerVersion
    {
        /// <summary>
        ///   <para>Retrieves the build portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The build number. Is -1 if undefined.</para>
        /// </value>
        int Build { get; }

        /// <summary>
        ///   <para>Retrieves the major portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The major number.</para>
        /// </value>
        int Major { get; }

        /// <summary>
        ///   <para>Retrieves the high 16 bits of the <see cref="Microsoft.Hpc.Scheduler.IServerVersion.Revision" /> number.</para>
        /// </summary>
        /// <value>
        ///   <para>The major revision number.</para>
        /// </value>
        short MajorRevision { get; }

        /// <summary>
        ///   <para>Retrieves the minor portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The minor number.</para>
        /// </value>
        int Minor { get; }

        /// <summary>
        ///   <para>Retrieves the low 16 bits of the <see cref="Microsoft.Hpc.Scheduler.IServerVersion.Revision" /> number.</para>
        /// </summary>
        /// <value>
        ///   <para>The minor revision number.</para>
        /// </value>
        short MinorRevision { get; }

        /// <summary>
        ///   <para>Retrieves the revision portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The revision number. Is -1 if undefined.</para>
        /// </value>
        int Revision { get; }
    }

    /// <summary>
    ///   <para>Contains the file version information for the HPC server assembly.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IServerVersion" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidServerVersion)]
    [ClassInterface(ClassInterfaceType.None)]
    public class ServerVersion : IServerVersion
    {
        int _build = 0;
        int _major = 0;
        int _minor = 0;
        int _revision = 0;

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified <see cref="System.Version" /> object.</para>
        /// </summary>
        /// <param name="ver">
        ///   <para>A <see cref="System.Version" /> object used to initialize the contents of this class. </para>
        /// </param>
        public ServerVersion(Version ver)
        {
            _major = ver.Major;
            _minor = ver.Minor;
            _build = ver.Build;
            _revision = ver.Revision;
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified parts of the version string.</para>
        /// </summary>
        /// <param name="major">
        ///   <para>The major number.</para>
        /// </param>
        /// <param name="minor">
        ///   <para>The minor number.</para>
        /// </param>
        /// <param name="build">
        ///   <para>The build number or -1 if not defined.</para>
        /// </param>
        /// <param name="revision">
        ///   <para>The revision number or -1 if not defined.</para>
        /// </param>
        public ServerVersion(int major, int minor, int build, int revision)
        {
            _major = major;
            _minor = minor;
            _build = build;
            _revision = revision;
        }

        /// <summary>
        ///   <para>Retrieves the build portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The build number. Is -1 if undefined.</para>
        /// </value>
        public int Build
        {
            get { return _build; }
        }

        /// <summary>
        ///   <para>Retrieves the major portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The major number.</para>
        /// </value>
        public int Major
        {
            get { return _major; }
        }

        /// <summary>
        ///   <para>Retrieves the high 16 bits of the <see cref="Microsoft.Hpc.Scheduler.ServerVersion.Revision" /> number.</para>
        /// </summary>
        /// <value>
        ///   <para>The major revision number.</para>
        /// </value>
        public short MajorRevision
        {
            get { return 0; }
        }

        /// <summary>
        ///   <para>The minor portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The minor number.</para>
        /// </value>
        public int Minor
        {
            get { return _minor; }
        }

        /// <summary>
        ///   <para>Retrieves the low 16 bits of the <see cref="Microsoft.Hpc.Scheduler.ServerVersion.Revision" /> number.</para>
        /// </summary>
        /// <value>
        ///   <para>The minor revision number.</para>
        /// </value>
        public short MinorRevision
        {
            get { return 0; }
        }

        /// <summary>
        ///   <para>Retrieves the revision portion of the version number.</para>
        /// </summary>
        /// <value>
        ///   <para>The revision number. Is -1 if undefined.</para>
        /// </value>
        public int Revision
        {
            get { return _revision; }
        }
    }


}
