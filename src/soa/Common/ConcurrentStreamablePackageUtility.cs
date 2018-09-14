//-----------------------------------------------------------------------
// <copyright file="ConcurrentStreamablePackageUtility.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Provides utiliy methods to read data from the
//   ConcurrentStreamablePackage stream and writes into a package
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Packaging;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides utiliy methods to read data from the
    /// ConcurrentStreamablePackage stream and writes into a package
    /// </summary>
    internal static class ConcurrentStreamablePackageUtility
    {
        /// <summary>
        /// Stores the exception symbol
        /// </summary>
        /// <remarks>
        /// We don't reference this value from Constant class
        /// because this file is shared by multiple projects
        /// including UI and SOA components
        /// </remarks>
        private const string ExceptionSymbol = "$EXCEPTION$";

        /// <summary>
        /// Reads the incoming stream of ConcurrentStreamablePackage and
        /// writes the data into a package
        /// </summary>
        /// <param name="reader">indicating the reader</param>
        /// <param name="output">indicating the output package</param>
        public static void ReadToPackage(BinaryReader reader, Package output)
        {
            MemoryStream exceptionBuffer = null;
            try
            {
                exceptionBuffer = new MemoryStream();
                Dictionary<string, Stream> partDic = new Dictionary<string, Stream>();
                try
                {
                    while (true)
                    {
                        // Read a frame (uri, length, payload)
                        string uri = reader.ReadString();
                        int length = reader.ReadInt32();
                        byte[] buffer = reader.ReadBytes(length);

                        if (uri != ExceptionSymbol)
                        {
                            // Write it into the package
                            Stream stream;
                            if (!partDic.TryGetValue(uri, out stream))
                            {
                                Uri packUri = PackUriHelper.CreatePartUri(new Uri(uri, UriKind.Relative));
                                PackagePart part = output.CreatePart(packUri, MediaTypeNames.Text.Plain, CompressionOption.Normal);
                                stream = part.GetStream();
                                partDic.Add(uri, stream);
                            }

                            stream.Write(buffer, 0, length);
                        }
                        else
                        {
                            exceptionBuffer.Write(buffer, 0, length);
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                }
                finally
                {
                    // Close all streams
                    foreach (Stream stream in partDic.Values)
                    {
                        try
                        {
                            stream.Close();
                        }
                        catch
                        {
                        }
                    }

                    output.Flush();
                }

                if (exceptionBuffer.Length != 0)
                {
                    exceptionBuffer.Seek(0, SeekOrigin.Begin);
                    using (BinaryReader exceptionReader = new BinaryReader(exceptionBuffer))
                    {
                        exceptionBuffer = null;
                        var ex = new ConcurrentStreamablePackageException(exceptionReader);
                        if (ex.ExceptionDetails.Count > 0)
                        {
                            throw ex;
                        }
                    }
                }
            }
            finally
            {
                if (exceptionBuffer != null)
                {
                    exceptionBuffer.Dispose();
                }
            }
        }

        /// <summary>
        /// Writes exceptions into the package stream
        /// </summary>
        /// <param name="sw">indicating the binary writer</param>
        /// <param name="task">indicating the task instance</param>
        /// <param name="node">indicating the node name</param>
        public static void WriteExceptions(BinaryWriter sw, Task task, string node)
        {
            if (task.IsFaulted)
            {
                sw.Write(node);
                sw.Write(task.Exception.InnerExceptions.Count);
                foreach (Exception ex in task.Exception.InnerExceptions)
                {
                    sw.Write(BuildExceptionDetail(ex));
                }
            }
        }

        /// <summary>
        /// Utility method to build exception detail from an instance
        /// of Exception class.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <remarks>
        /// For debug build, this method would build the exception detail
        /// using Exception.ToString() method (which would include the
        /// callstack and inner exceptions).
        /// For retail build, this method would manually build a string
        /// include Exception.Message property of all the inner exceptions.
        /// </remarks>
        private static string BuildExceptionDetail(Exception exception)
        {
#if DEBUG
            Debug.Assert(exception != null, "Internal param check.");
            return exception.ToString();
#else
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(exception.Message);
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                sb.AppendFormat("\tInnerException: {0}\n", exception.Message);
            }

            return sb.ToString();
#endif
        }
    }
}
