//------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Global FxCop suppressions.
// </summary>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#uploadedBlockIds")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#DownloadBlockListCallback(System.IAsyncResult)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#unprocessedBlocks")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#UploadCallback(System.IAsyncResult)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#GetUploadAction()")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlockBlobUploadController.#.ctor(Microsoft.Hpc.Azure.DataMovement.BlobTransferFileTransferEntry,Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob,System.String,System.IO.Stream,System.Boolean,System.Boolean,Microsoft.Hpc.Azure.DataMovement.BlobTransferOptions,Microsoft.Hpc.Azure.DataMovement.MemoryManager,System.Action`1<System.Object>,System.Action`3<System.Object,System.Double,System.Double>,System.Action`2<System.Object,System.Exception>,System.Object)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#availablePageRangeData")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#SetPageRangeDownloadHasWork()")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#DownloadPageRangeCallback(System.IAsyncResult)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#GetDownloadPageBlobAction()")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#WritePageRangeDataCallback(System.IAsyncResult)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#GetPageRangesCallback(System.IAsyncResult)")]
[module: SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Scope = "member", Target = "Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController.#BeginDownloadPageRange(Microsoft.Hpc.Azure.DataMovement.TransferControllers.BlobDownloadController+DownloadPageState)")]
