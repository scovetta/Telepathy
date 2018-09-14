//------------------------------------------------------------------------------
// <copyright file="MemoryManager.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Class managing large memory allocations.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Class for maintaining a pool of memory buffer objects.
    /// </summary>
    internal class MemoryManager
    {
        private MemoryPool memoryPool;

        public MemoryManager(
            long capacity, int bufferSize)
        {
            long availableCells = capacity / bufferSize;

            int cellNumber = (int)Math.Min((long)BlobTransferConstants.MemoryManagerCellsMaximum, availableCells);

            this.memoryPool = new MemoryPool(cellNumber, bufferSize);
        }

        public byte[] RequireBuffer()
        {
            return this.memoryPool.GetBuffer();
        }

        public void ReleaseBuffer(byte[] buffer)
        {
            this.memoryPool.AddBuffer(buffer);
        }

        private class MemoryPool
        {
            public readonly int BufferSize;

            private int availableCells;
            private int allocatedCells;
            private object cellsListLock;
            private MemoryCell cellsListHeadCell;
            private ConcurrentDictionary<byte[], MemoryCell> cellsInUse;

            public MemoryPool(int cellsCount, int bufferSize)
            {
                this.BufferSize = bufferSize;

                this.availableCells = cellsCount;
                this.allocatedCells = 0;
                this.cellsListLock = new object();
                this.cellsListHeadCell = null;
                this.cellsInUse = new ConcurrentDictionary<byte[], MemoryCell>();
            }

            public byte[] GetBuffer()
            {
                if (this.availableCells > 0)
                {
                    MemoryCell retCell = null;

                    lock (this.cellsListLock)
                    {
                        if (this.availableCells > 0)
                        {
                            if (null != this.cellsListHeadCell)
                            {
                                retCell = this.cellsListHeadCell;
                                this.cellsListHeadCell = retCell.NextCell;
                                retCell.NextCell = null;
                            }
                            else
                            {
                                retCell = new MemoryCell(this.BufferSize);
                                ++this.allocatedCells;
                            }

                            --this.availableCells;
                        }
                    }

                    if (null != retCell)
                    {
                        this.cellsInUse.TryAdd(retCell.Buffer, retCell);
                        return retCell.Buffer;
                    }
                }

                return null;
            }

            public void AddBuffer(byte[] buffer)
            {
                if (null == buffer)
                {
                    throw new ArgumentNullException("buffer");
                }

                MemoryCell cell;
                if (!this.cellsInUse.TryRemove(buffer, out cell))
                {
                    throw new ArgumentException(Resources.BufferNotAllocatedThroughMemoryManagerException, "buffer");
                }

                lock (this.cellsListLock)
                {
                    cell.NextCell = this.cellsListHeadCell;
                    this.cellsListHeadCell = cell;
                    ++this.availableCells;
                }
            }
        }

        private class MemoryCell
        {
            private byte[] buffer;

            public MemoryCell(int size)
            {
                this.buffer = new byte[size];
            }

            public MemoryCell NextCell
            {
                get;
                set;
            }

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }
        }
    }
}
