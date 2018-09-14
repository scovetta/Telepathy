//------------------------------------------------------------------------------
// <copyright file="BlobTransferFileTransferEntry.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Represents a single file transfer entry.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Represents a single file transfer entry.
    /// </summary>
    public class BlobTransferFileTransferEntry
    {
        /// <summary>
        /// Restart journal format version.
        /// </summary>
        private const int FormatVersion = 1;

        /// <summary>
        /// Size of enum type in buffer.
        /// </summary>
        private const int EnumTypeSize = 2;

        /// <summary>
        /// Size of offset type in buffer.
        /// Offset should be int64 type.
        /// </summary>
        private const int OffsetSize = 8;

       /// <summary>
       /// Copy Id string is 36 charactors which is 72 bytes in Unicode.
       /// </summary>
        private const int CopyIdSize = 72;

        /// <summary>
        /// Copy Id string is 19 charactors which is 38 bytes in Unicode.
        /// </summary>
        private const int ETagSize = 38;

        /// <summary>
        /// Copy Id string is 9 charactors which is 18 bytes in Unicode.
        /// </summary>
        private const int BlockIdPrefixSize = 18;
                
        // Format of file transfer entries stored in restartable log is:
        // Format Version:                              4 bytes
        // Transfer status:                             2 bytes
        // source blob type:                            2 bytes
        // Last modified time:                          8 bytes
        // Source snapshot time:                        8 bytes
        // Transferred offset:                          8 bytes
        // Upload window count:                         2 bytes 
        // Upload window:                               128 * 8 bytes
        // Copy id:                                     72 + 4bytes
        // Block id prefix:                             18 + 4bytes
        // Etag:                                        38 + 4bytes
        // Source relative path:                        
        // Destination relative path: 

        /// <summary>
        /// Static part includes all but strings source relative path and destination relative path
        /// whose length changes.
        /// </summary>
        private const int StaticPartSize = 1206;

        /// <summary>
        /// Position from where to write strings in buffer, right after Upload window.
        /// </summary>
        private const int StringPartPosition = 1058;

        /// <summary>
        /// Allocate granularity.
        /// In our case, string length of source/destination path is the only variability.
        /// 128 bytes is 64 characters in Unicode. 
        /// For my estimation, 32 characters more for each path string is not too mach,
        /// and in this way, we don't need to reallocate too many times.
        /// </summary>
        private const int AllocateGranularity = 128;

        /// <summary>
        /// Encoding used to encode strings.
        /// Be careful with changing this, for our buffer size is calculated according to it.
        /// </summary>
        private readonly Encoding encoding = Encoding.Unicode;

        /// <summary>
        /// The transfer status of this file entry.
        /// </summary>
        private volatile BlobTransferEntryStatus status;
        
        /// <summary>
        /// Prefix for block ids if it is a block blob upload operation.
        /// </summary>
        private string blockIdPrefix;

        /// <summary>
        /// The blob set this file transfer entry belongs to.
        /// </summary>
        private DeletionBlobSet blobSet;

        /// <summary>
        /// Object to be locked on when access this transfer entry.
        /// </summary>
        private object entryLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferEntry"/> class.
        /// </summary>
        /// <param name="infoBuffer">Memory buffer which contains transfer entry information.</param>
        public BlobTransferFileTransferEntry(byte[] infoBuffer)
        {
            this.ReadFromBuffer(infoBuffer);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferEntry"/> class.
        /// </summary>
        /// <param name="sourceRelativePath">File transfer relative source path.</param>
        /// <param name="destinationRelativePath">File transfer relative destination path.</param>
        /// <param name="lastModified">The last updated time of the file entry in UTC time.</param>
        /// <param name="sourceSnapshotTime">File transfer source blob snapshot time. Set null for file system source entries.</param>
        /// <param name="sourceBlobType">File transfer source blob type. Set null for file system source entries.</param>
        /// <param name="copyId">Indicates the copy id. Set null if there's no copy operation happenning.</param>
        /// <param name="etag">Indicates the ETag. Set null for file system source entries.</param>
        /// <param name="blockIdPrefix">Prefix for block id.</param>
        /// <param name="status">File transfer status.</param>
        /// <param name="checkPoint">File transfer check point, represent which blocks have been transferred.</param>
        public BlobTransferFileTransferEntry(
            string sourceRelativePath,
            string destinationRelativePath,
            DateTimeOffset? lastModified,
            DateTimeOffset? sourceSnapshotTime,
            BlobType sourceBlobType,
            string copyId,
            string etag,
            string blockIdPrefix,
            BlobTransferEntryStatus status,
            BlobTransferFileTransferEntryCheckPoint checkPoint)
        {
            this.CopyId = copyId;
            this.ETag = etag;
            this.blockIdPrefix = blockIdPrefix;

            this.SourceRelativePath = sourceRelativePath;
            this.DestinationRelativePath = destinationRelativePath;

            this.LastModified = lastModified;
            this.SourceSnapshotTime = sourceSnapshotTime;

            this.SourceBlobType = sourceBlobType;

            this.Status = BlobTransferEntryStatus.NotStarted;
            this.CheckPoint = checkPoint == null ?
                new BlobTransferFileTransferEntryCheckPoint() : checkPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferEntry"/> class.
        /// </summary>
        /// <param name="sourceRelativePath">File transfer relative source path.</param>
        /// <param name="destinationRelativePath">File transfer relative destination path.</param>
        /// <param name="lastModified">The last updated time of the file entry in UTC time.</param>
        /// <param name="sourceSnapshotTime">File transfer source blob snapshot time. Set null for file system source entries.</param>
        /// <param name="sourceBlobType">File transfer source blob type. Set null for file system source entries.</param>
        internal BlobTransferFileTransferEntry(
            string sourceRelativePath,
            string destinationRelativePath,
            DateTimeOffset? lastModified,
            DateTimeOffset? sourceSnapshotTime,
            BlobType sourceBlobType)
            : this(
                sourceRelativePath, 
                destinationRelativePath, 
                lastModified, 
                sourceSnapshotTime, 
                sourceBlobType, 
                null, 
                null, 
                null,
                BlobTransferEntryStatus.NotStarted, 
                null)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferEntry"/> class.
        /// </summary>
        internal BlobTransferFileTransferEntry()
            : this(null, null, null, null, BlobType.Unspecified)
        {
        }

        /// <summary>
        /// Gets file transfer relative source path.
        /// </summary>
        /// <value>Relative source path of the transfer entry.</value>
        public string SourceRelativePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets file transfer relative destination path.
        /// </summary>
        /// <value>Relative destination path of the transfer entry.</value>
        public string DestinationRelativePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last updated time of the file entry in UTC time.
        /// </summary>
        /// <value>The last modified time of the source.</value>
        public DateTimeOffset? LastModified
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets file transfer source blob snapshot time. Always null for file system source entries.
        /// </summary>
        /// <value>Snapshot time of the source. Null if the source is not a blob snapshot.</value>
        public DateTimeOffset? SourceSnapshotTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets file transfer source blob type. Always unspecified for file system source entries.
        /// </summary>
        /// <value>Type of the source blob. Always BlobType.Unspecified for file system source.</value>
        public BlobType SourceBlobType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating the transfer status of this file entry.
        /// </summary>
        /// <value>Indicates this transfer status.</value>
        public BlobTransferEntryStatus Status
        {
            get
            {
                return this.status;
            }

            internal set
            {
                if (Enum.IsDefined(typeof(BlobTransferEntryStatus), value))
                {
                    this.status = value;
                }
                else
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.UndefinedTransferEntryStatusException,
                        value));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file transfer has finished.
        /// </summary>
        /// <value>Indicates this transfer has fully finished.</value>
        public bool EntryTransferFinished
        {
            get
            {
                return BlobTransferEntryStatus.Finished == this.Status;
            }
        }

        /// <summary>
        /// Gets a value indicating the copy id of this file entry if it is a blob copy operation.
        /// </summary>
        /// <value>Indicates the copy id.</value>
        public string CopyId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating the prefix for block ids if it is a block blob upload operation.
        /// </summary>
        /// <value>Indicates the prefix for block ids.</value>
        public string BlockIdPrefix
        {
            get
            {
                return this.blockIdPrefix;
            }

            internal set
            {
                if (null == this.blockIdPrefix)
                {
                    this.blockIdPrefix = value;
                }
                else
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.TransferEntryPropertyCanBeSetOnlyOnceException,
                        "BlockIdPrefix"));
                }
            }
        }

        /// <summary>
        /// Gets ETag of this transfer entry. Always null for file system entry.
        /// </summary>
        /// <value>ETag of this transfer entry. Always null for file system entry.</value>
        public string ETag
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets object to be locked on when access this transfer entry.
        /// </summary>
        /// <value>Object to be locked on when access this transfer entry.</value>
        internal object EntryLock
        {
            get
            {
                return this.entryLock;
            }
        }

        /// <summary>
        /// Gets or sets transfer status of this file entry.
        /// </summary>
        /// <value>Transfer status of this file entry.</value>
        internal BlobTransferFileTransferEntryCheckPoint CheckPoint
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the blob set this file transfer entry belongs to. Always null for file system source entries.
        /// </summary>
        /// <value>Blob set this entry belongs to. Always null for file system source.</value>
        internal DeletionBlobSet BlobSet
        {
            get
            {
                return this.blobSet;
            }

            set
            {
                if (null == this.blobSet)
                {
                    this.blobSet = value;
                }
                else
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.TransferEntryPropertyCanBeSetOnlyOnceException,
                        "BlobSet"));
                }
            }
        }

        /// <summary>
        /// Get the check point for restart information.
        /// </summary>
        /// <returns>Check point for restart information. </returns>
        public BlobTransferFileTransferEntryCheckPoint GetCheckPoint()
        {
            lock (this.entryLock)
            {
                return new BlobTransferFileTransferEntryCheckPoint(this.CheckPoint);
            }
        }

        /// <summary>
        /// Gets memory buffer of this transfer entry. Access to the memory buffer is not thread safe, 
        /// user have to guarantee that only one thread to access this buffer at the same time.
        /// </summary>
        /// <param name="entryBuffer">Memory buffer to write transfer entry information into.</param>
        /// <param name="infoLength">Actually written length in entryBuffer, caller should just keep this part persistent.</param>
        public void GetInfoBuffer(ref byte[] entryBuffer, out int infoLength)
        {
            infoLength = this.CalculateBufferSize();

            this.AllocateBuffer(ref entryBuffer, infoLength);

            MemoryStream entryMemoryStream = new MemoryStream(entryBuffer);

            entryMemoryStream.Position = 0;
            entryMemoryStream.Write(BitConverter.GetBytes(FormatVersion), 0, sizeof(int));

            entryMemoryStream.Write(BitConverter.GetBytes((short)this.status), 0, EnumTypeSize);
            entryMemoryStream.Write(BitConverter.GetBytes((short)this.SourceBlobType), 0, EnumTypeSize);

            this.WriteDateTime(entryMemoryStream, this.LastModified);
            this.WriteDateTime(entryMemoryStream, this.SourceSnapshotTime);

            BlobTransferFileTransferEntryCheckPoint checkPoint = this.GetCheckPoint();

            entryMemoryStream.Write(BitConverter.GetBytes(checkPoint.EntryTransferOffset), 0, sizeof(long));

            if (null != checkPoint.UploadWindow)
            {
                entryMemoryStream.Write(BitConverter.GetBytes((short)checkPoint.UploadWindow.Count), 0, sizeof(short));

                foreach (long uploadOffset in checkPoint.UploadWindow)
                {
                    entryMemoryStream.Write(BitConverter.GetBytes(uploadOffset), 0, sizeof(long));
                }
            }
            else
            {
                // Just write upload window count here, no need for clear for we won't read it anyway.
                entryMemoryStream.Write(BitConverter.GetBytes((short)0), 0, sizeof(short));
            }

            entryMemoryStream.Position = StringPartPosition;

            // To make write string easier, clear buffer here.
            Array.Clear(entryBuffer, StringPartPosition, infoLength - StringPartPosition);

            this.WriteString(entryMemoryStream, this.CopyId, CopyIdSize);
            this.WriteString(entryMemoryStream, this.ETag, ETagSize);
            this.WriteString(entryMemoryStream, this.blockIdPrefix, BlockIdPrefixSize);
            this.WriteString(entryMemoryStream, this.SourceRelativePath, -1);
            this.WriteString(entryMemoryStream, this.DestinationRelativePath, -1);
        }

        /// <summary>
        /// Gets memory buffer of this transfer entry. Access to the memory buffer is not thread safe, 
        /// user have to guarantee that only one thread to access this buffer at the same time.
        /// </summary>
        /// <param name="entryBuffer">Memory buffer to get transfer entry information from.</param>
        private void ReadFromBuffer(byte[] entryBuffer)
        {
            int position = 0;

            if (FormatVersion != BitConverter.ToInt32(entryBuffer, position))
            {
                throw new InvalidDataException(
                        Resources.RestartableInfoCorruptedException);
            }

            position += sizeof(int);

            this.status = (BlobTransferEntryStatus)Enum.ToObject(typeof(BlobTransferEntryStatus), BitConverter.ToInt16(entryBuffer, position));
            position += sizeof(short);

            this.SourceBlobType = (BlobType)Enum.ToObject(typeof(BlobType), BitConverter.ToInt16(entryBuffer, position));
            position += sizeof(short);

            this.LastModified = this.ReadDateTime(entryBuffer, ref position);

            this.SourceSnapshotTime = this.ReadDateTime(entryBuffer, ref position);

            long entryTransferOffset = BitConverter.ToInt64(entryBuffer, position);
            position += OffsetSize;

            short uploadWindowCount = BitConverter.ToInt16(entryBuffer, position);
            position += sizeof(short);

            if (uploadWindowCount > BlobTransferFileTransferEntryCheckPoint.MaxUploadWindowLength)
            {
                throw new InvalidDataException(
                        Resources.RestartableInfoCorruptedException);
            }

            List<long> uploadWindow = null;

            if (uploadWindowCount > 0)
            {
                uploadWindow = new List<long>();

                while (uploadWindowCount > 0)
                {
                    uploadWindow.Add(BitConverter.ToInt64(entryBuffer, position));
                    position += OffsetSize;
                    --uploadWindowCount;
                }
            }

            this.CheckPoint = new BlobTransferFileTransferEntryCheckPoint(entryTransferOffset, uploadWindow);

            position = StringPartPosition;

            this.CopyId = this.ReadString(entryBuffer, ref position, CopyIdSize);
            this.ETag = this.ReadString(entryBuffer, ref position, ETagSize);
            this.blockIdPrefix = this.ReadString(entryBuffer, ref position, BlockIdPrefixSize);
            this.SourceRelativePath = this.ReadString(entryBuffer, ref position, -1);
            this.DestinationRelativePath = this.ReadString(entryBuffer, ref position, -1);
        }

        /// <summary>
        /// Calculate buffer size needed to write this transfer entry information.
        /// </summary>
        /// <returns>Buffer size needed to write this transfer entry information.</returns>
        private int CalculateBufferSize()
        {
            return StaticPartSize
                + this.encoding.GetBytes(this.SourceRelativePath).Length
                + this.encoding.GetBytes(this.DestinationRelativePath).Length;
        }

        /// <summary>
        /// Write string to MemoryStream.
        /// </summary>
        /// <param name="ms">MemoryStream to be written into.</param>
        /// <param name="input">String to be written.</param>
        /// <param name="length">Length to be reserved when the input string is null.
        /// If length should be determined by the input string, input -1 here.</param>
        private void WriteString(MemoryStream ms, string input, int length)
        {
            Debug.Assert(
                !((null == input) && (-1 == length)),
                "Must have a reserved length when trying to write a temporally not exist string");

            if (null == input)
            {
                ms.Write(BitConverter.GetBytes(length), 0, sizeof(int));
                ms.Position += length;
                return;
            }

            byte[] stringBytes = this.encoding.GetBytes(input);

            Debug.Assert(
                (-1 == length) || (length == stringBytes.Length),
                "The reserved length should be exact the same with the string buffer");

            ms.Write(BitConverter.GetBytes(stringBytes.Length), 0, sizeof(int));
            ms.Write(stringBytes, 0, stringBytes.Length);
        }

        /// <summary>
        /// Read string from a buffer.
        /// </summary>
        /// <param name="memBuffer">Memory buffer to be read from.</param>
        /// <param name="position">Position from where to read a string in memBuffer.</param>
        /// <param name="length">Length of string information in memBuffer.</param>
        /// <returns>String read from the buffer.</returns>
        private string ReadString(byte[] memBuffer, ref int position, int length)
        {
            int stringLength = BitConverter.ToInt32(memBuffer, position);

            if (stringLength <= 0)
            {
                throw new InvalidDataException(
                    Resources.RestartableInfoCorruptedException);
            }

            position += sizeof(int);

            Debug.Assert(
                (-1 == length) || (length == stringLength),
                "String length read from buffer should be exact the same with the specified one");

            bool nullString = true;
            for (int i = position; i < position + stringLength; ++i)
            {
                if (0 != memBuffer[i])
                {
                    nullString = false;
                }
            }

            string readString = null;

            if (!nullString)
            {
                readString = this.encoding.GetString(memBuffer, position, stringLength);
            }
            else
            {
                Debug.Assert(-1 != length, "That should never be a null string when we need to read its length from buffer");
            }

            position += stringLength;

            return readString;
        }

        /// <summary>
        /// Write date time information to memory buffer whose content will be written into retartable log.
        /// </summary>
        /// <param name="ms">Memory stream to deal with memory buffer which store the transfer information.</param>
        /// <param name="input">DateTimeOffset to be written into restartale log.</param>
        private void WriteDateTime(MemoryStream ms, DateTimeOffset? input)
        {
            if (null == input)
            {
                ms.Write(BitConverter.GetBytes((long)-1), 0, sizeof(long));
            }
            else
            {
                ms.Write(BitConverter.GetBytes(input.GetValueOrDefault().ToFileTime()), 0, sizeof(long));
            }
        }

        /// <summary>
        /// Read datetime information from a restartable log file.
        /// </summary>
        /// <param name="membuff">Memory buffer which store the whole file transfer information.</param>
        /// <param name="offset">Offset in memory buffer where the date time information is stored.</param>
        /// <returns>Returns date time read.</returns>
        private DateTimeOffset? ReadDateTime(byte[] membuff, ref int offset)
        {
            long fileTime = BitConverter.ToInt64(membuff, offset);
            offset += sizeof(long);

            if (-1 == fileTime)
            {
                return null;
            }

            return DateTimeOffset.FromFileTime(fileTime);
        }

        private void AllocateBuffer(ref byte[] buffer, int length)
        {
            if ((null == buffer)
                || (buffer.Length < length))
            {
                int allocateLength = ((length / AllocateGranularity) + 1) * AllocateGranularity;
                buffer = new byte[allocateLength];
            }
        }

        /// <summary>
        /// TODO: remove this ugly class.
        /// The deletion blob set consists of a root blob and all its snapshots.
        /// When BlobTransferOptions.MoveFile is set, the source blobs need to be removed. However, for a blob with snapshots, 
        /// we cannot remove it until we finish transferring all its snapshots (if we need to transfer its snapshots).
        /// The blob transferring is orderless because of different blob size and completion in transferring. So we introduce 
        /// this class to controll the deletion of a blob and its snapshots. For a deletion blob, the deletion of the root 
        /// blob is done by the last finished item.
        /// </summary>
        internal class DeletionBlobSet
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeletionBlobSet"/> class.
            /// </summary>
            /// <param name="rootBlob">The root blob of this deletion blob set.</param>
            /// <param name="count">Number of blobs in this blob set.</param>
            public DeletionBlobSet(ICloudBlob rootBlob, int count)
            {
                Debug.Assert(null != rootBlob, "RootBlob cannot be null.");
                Debug.Assert(!rootBlob.SnapshotTime.HasValue, "RootBlob cannot be snapshot blob.");
                Debug.Assert(0 < count, "Blob set count must be positive.");

                this.RootBlob = rootBlob;
                this.CountDown = new CountdownEvent(count);
            }

            /// <summary>
            /// Gets the root blob to perform deleting operation.
            /// </summary>
            /// <value>Root blob.</value>
            public ICloudBlob RootBlob
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets countdown event to track the number of the blob set.
            /// </summary>
            /// <value>Countdown event to track the number of the blob set..</value>
            public CountdownEvent CountDown
            {
                get;
                private set;
            }
        }
    }
}
