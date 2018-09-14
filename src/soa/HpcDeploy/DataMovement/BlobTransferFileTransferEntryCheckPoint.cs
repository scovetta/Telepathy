//------------------------------------------------------------------------------
// <copyright file="BlobTransferFileTransferEntryCheckPoint.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Represents a single file transfer entry's check point,
//      includes position of transfered bytes and upload window.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a single file transfer entry's check point, 
    /// includes position of transfered bytes and upload window.
    /// </summary>
    public class BlobTransferFileTransferEntryCheckPoint
    {
        /// <summary>
        /// Indicate the max length that an upload window can be.
        /// </summary>
        public const int MaxUploadWindowLength = 128;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferEntryCheckPoint"/> class.
        /// </summary>
        /// <param name="entryTransferOffset">Transferred offset of this transfer entry.</param>
        /// <param name="uploadWindow">Upload window of this transfer entry.</param>
        public BlobTransferFileTransferEntryCheckPoint(long entryTransferOffset, List<long> uploadWindow)
        {
            this.EntryTransferOffset = entryTransferOffset;
            if (null != uploadWindow)
            {
                this.UploadWindow = new List<long>(uploadWindow);
            }
            else
            {
                this.UploadWindow = null;
            }
        }

        internal BlobTransferFileTransferEntryCheckPoint(BlobTransferFileTransferEntryCheckPoint checkPoint)
            : this(checkPoint.EntryTransferOffset, checkPoint.UploadWindow)
        {
        }

        internal BlobTransferFileTransferEntryCheckPoint()
            : this(0, null)
        {
        }

        /// <summary>
        /// Gets transferred offset of this transfer entry.
        /// </summary>
        /// <value>Transferred offset of this transfer entry.</value>
        public long EntryTransferOffset
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets upload window of this transfer entry.
        /// </summary>
        /// <value>Upload window of this transfer entry.</value>
        public List<long> UploadWindow
        {
            get;
            internal set;
        }

        internal void Clear()
        {
            this.EntryTransferOffset = 0;

            if (null != this.UploadWindow)
            {
                this.UploadWindow.Clear();
            }
        }
    }
}
