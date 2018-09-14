//------------------------------------------------------------------------------
// <copyright file="BlobTransferFileTransferStatus.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Represents the restartable mode file transfer state.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    /// <summary>
    /// Represents the restartable mode file transfer state.
    /// </summary>
    public class BlobTransferFileTransferStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferStatus"/> class.
        /// </summary>
        public BlobTransferFileTransferStatus()
        {
            this.FileEntries = new BlobTransferFileTransferEntries();
            this.Initialized = false;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferFileTransferStatus"/> class.
        /// </summary>
        /// <param name="fileEntries">File entry list object to initialize the FileEntries property with.</param>
        /// <param name="initalized">Whether or not has been finished on initializing BlobTransferFileTransferStatus.</param>
        public BlobTransferFileTransferStatus(BlobTransferFileTransferEntries fileEntries, bool initalized)
        {
            this.FileEntries = fileEntries;
            this.Initialized = initalized;
        }

        /// <summary>
        /// Gets or sets the dictionary containing file transfer entries for restartable mode.
        /// </summary>
        /// <value>The collection of file transfer entries.</value>
        public BlobTransferFileTransferEntries FileEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether FileEntries is initialized.
        /// </summary>
        /// <value>Indicates if the FileEntries field has been fully initialized.</value>
        public bool Initialized
        {
            get;
            set;
        }
    }
}
