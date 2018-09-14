//--------------------------------------------------------------------------
// <copyright file="FileUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     File utility
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// File operation utility class
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Default buffer size: 64K
        /// </summary>
        private const int BufferSize = 64 * 1024;

        /// <summary>
        /// TODO: Only recognize "\r\n" as newlines currently.
        /// </summary>
        private static string Newline = "\r\n";

        /// <summary>
        /// Copy remainder chars from fs to outStream. Stream fs will be closed when CopyFile
        /// returns successfully. BOM (if exists) will be written to the output stream.
        /// Only recognize "\r\n" as newlines currently.
        /// </summary>
        /// <param name="fs">target file stream</param>
        /// <param name="outStream">output stream</param>
        /// <param name="inEncoding">input encoding, default set to null to auto detect encoding from fs</param>
        /// <param name="outEncoding">output encoding, default set to null to use the same encoding as fs</param>
        public static void CopyFile(Stream fs, Stream outStream, Encoding inEncoding = null, Encoding outEncoding = null)
        {
            using (StreamReader fIn = null == inEncoding ?
                new StreamReader(fs, true) :
                new StreamReader(fs, inEncoding))
            {
                if (!fIn.EndOfStream)
                {
                    MemoryStream tempStream = null;
                    try
                    {
                        tempStream = new MemoryStream();
                        using (StreamWriter fOut = new StreamWriter(tempStream, outEncoding ?? fIn.CurrentEncoding))
                        {
                            var tempTempStream = tempStream;
                            tempStream = null;

                            while (!fIn.EndOfStream)
                            {
                                fOut.Write((char)fIn.Read());
                            }

                            fOut.Flush();
                            tempTempStream.WriteTo(outStream);
                        }
                    }
                    finally
                    {
                        if (tempStream != null)
                            tempStream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Read the first "nLines" lines from a file. Stream fs will be closed when HeadFile 
        /// returns successfully. The returned offset may be incorrect if stream fs does not 
        /// support Seek(). BOM (if exists) will be written to the output stream (if specified).
        /// Only recognize "\r\n" as newlines currently.
        /// </summary>
        /// <param name="fs">target file stream</param>
        /// <param name="nLines">number of lines to read</param>
        /// <param name="outStream">output stream</param>
        /// <param name="inEncoding">input encoding, default set to null to auto detect encoding from fs</param>
        /// <param name="outEncoding">output encoding, default set to null to use the same encoding as fs</param>
        /// <returns>position in the file after the "nLines" lines end</returns>
        public static long HeadFile(Stream fs, int nLines, Stream outStream, Encoding inEncoding = null, Encoding outEncoding = null)
        {
            if (0 == nLines)
            {
                return 0;
            }

            using (StreamReader fIn = null == inEncoding ?
                new StreamReader(fs, true) :
                new StreamReader(fs, inEncoding))
            {
                // Try to get preamble length.
                int preambleLen = 0;
                if (CheckPreamble(fIn))
                {
                    preambleLen = fIn.CurrentEncoding.GetPreamble().Length;
                }

                long contentStartPos = preambleLen;
                long charsRead = 0;

                // Generate chars arrays for supported newlines.
                char[] NewlineChars = Newline.ToCharArray();

                // Reverse string for matching.
                Array.Reverse(NewlineChars);

                int newlineBufferLen = NewlineChars.Length;

                // A cycle buffer with a copy after it. Reads in the first cycle
                // could always get the correct substring.
                char[] newlineBuffer = new char[newlineBufferLen * 2];

                Action<char> WriteToNewlineBuffer = delegate(char b)
                {
                    // Write from end to front in a cycle.
                    int pt = newlineBufferLen - (int)(charsRead % newlineBufferLen) - 1;
                    newlineBuffer[pt] = b;
                    newlineBuffer[pt + newlineBufferLen] = b;
                };

                Func<int> MatchNewlinePatterns = delegate()
                {
                    int len = NewlineChars.Length;
                    if (charsRead < len)
                    {
                        return 0;
                    }

                    int pt = newlineBufferLen - (int)(charsRead % newlineBufferLen) - 1;
                    return CheckArrayMatch(len, NewlineChars, 0, newlineBuffer, pt) ? len : 0;
                };

                MemoryStream tempStream = null;
                StreamWriter fOut = null;
                if (null != outStream)
                {
                    tempStream = new MemoryStream();
                    fOut = new StreamWriter(tempStream, outEncoding ?? fIn.CurrentEncoding);
                }

                long endPos = contentStartPos;

                try
                {
                    int newlinesRead = 0;

                    while (newlinesRead < nLines)
                    {
                        int c = fIn.Read();
                        if (-1 == c)
                        {
                            break;
                        }

                        charsRead++;
                        endPos += fIn.CurrentEncoding.GetByteCount(new char[] { (char)c });

                        if (null != fOut)
                        {
                            fOut.Write((char)c);
                        }

                        WriteToNewlineBuffer((char)c);

                        if (MatchNewlinePatterns() > 0)
                        {
                            newlinesRead++;
                        }
                    }
                }
                finally
                {
                    if (null != fOut)
                    {
                        fOut.Flush();
                        tempStream.WriteTo(outStream);

                        tempStream.Dispose();
                        fOut.Dispose();
                    }
                }

                return endPos;
            }
        }

        /// <summary>
        /// Read the last "nLines" lines from a file. Stream fs will be closed when TailFile 
        /// returns successfully. Stream fs must support Seek() and Length. BOM (if exists) will 
        /// be written to the output stream (if specified).
        /// Only recognize "\r\n" as newlines currently.
        /// </summary>
        /// <param name="fs">target file stream</param>
        /// <param name="nLines">number of lines to read</param>
        /// <param name="outStream">output stream</param>
        /// <param name="inEncoding">input encoding, default set to null to auto detect encoding from fs</param>
        /// <param name="outEncoding">output encoding, default set to null to use the same encoding as fs</param>
        /// <returns>position in the file where the last "nLines" lines start</returns>
        public static long TailFile(FileStream fs, int nLines, Stream outStream, Encoding inEncoding = null, Encoding outEncoding = null)
        {
            if (0 == nLines || 0 == fs.Length)
            {
                return fs.Length;
            }

            // This StreamReader is only used for detecting input encoding.
            using (StreamReader fIn = null == inEncoding ?
                new StreamReader(fs, true) :
                new StreamReader(fs, inEncoding))
            {
                // Try to get preamble length.
                int preambleLen = 0;
                if (CheckPreamble(fIn))
                {
                    preambleLen = fIn.CurrentEncoding.GetPreamble().Length;
                }

                // Generate bytes arrays for supported newlines.
                byte[] NewlineBytes = fIn.CurrentEncoding.GetBytes(Newline.ToCharArray());
                int newlineBytesLen = NewlineBytes.Length;

                byte[] buffer = new byte[BufferSize + newlineBytesLen];
                long bytesAvailable = fs.Length - preambleLen;
                long contentStartPos = preambleLen;
                long bytesRead = 0;

                // Divide bytes into blocks by BufferSize and 
                // move readPos to the start of last block.
                long readPos = fs.Length - ((bytesAvailable - 1) % BufferSize + 1);
                long startPos = fs.Length;

                int newlinesRead = 0;

                while (readPos >= contentStartPos && newlinesRead < nLines)
                {
                    fs.Seek(readPos, SeekOrigin.Begin);

                    int curRead = ReadBytes(fs, buffer, BufferSize);
                    int index = curRead - 1;

                    while (index >= 0 && newlinesRead < nLines)
                    {
                        bytesRead++;

                        if (bytesRead >= newlineBytesLen)
                        {
                            if (CheckArrayMatch(newlineBytesLen, NewlineBytes, 0, buffer, index))
                            {
                                startPos = readPos + index + newlineBytesLen;

                                // Ignore the newline at the end of the file.
                                if (startPos < fs.Length)
                                {
                                    newlinesRead++;
                                }
                            }
                        }

                        index--;
                    }

                    Buffer.BlockCopy(buffer, 0, buffer, BufferSize, newlineBytesLen);

                    readPos -= BufferSize;
                }

                if (newlinesRead < nLines)
                {
                    startPos = contentStartPos;
                }

                fs.Seek(startPos, SeekOrigin.Begin);

                if (null != outStream)
                {
                    // Determine whether auto-detecting succeeded.
                    // UTF8 is the default encoding.
                    if (Encoding.UTF8 == fIn.CurrentEncoding &&
                        0 == preambleLen &&
                        null == outEncoding)
                    {
                        // Copy all bytes directly if failed in auto-detecting and 
                        // output encoding is not specified.
                        fs.CopyTo(outStream);
                    }
                    else
                    {
                        CopyFile(fs, outStream, fIn.CurrentEncoding, outEncoding);
                    }
                }

                return startPos;
            }
        }

        /// <summary>
        /// Read at most "len" bytes from a file
        /// </summary>
        /// <param name="fs">target file stream</param>
        /// <param name="buffer">buffer to contain the bytes</param>
        /// <param name="len">number of bytes to read</param>
        /// <returns>number of bytes read</returns>
        public static int ReadBytes(Stream fs, byte[] buffer, int len)
        {
            int bytesRead = 0;
            while (bytesRead < len)
            {
                int bytes = fs.Read(buffer, bytesRead, (int)(len - bytesRead));
                if (bytes == 0)
                {
                    break;
                }

                bytesRead += bytes;
            }

            return bytesRead;
        }

        /// <summary>
        /// Check whether we can get a preamble from input stream reader
        /// </summary>
        /// <param name="fIn">input stream reader</param>
        /// <returns>true if we can</returns>
        private static bool CheckPreamble(StreamReader fIn)
        {
            Stream fs = fIn.BaseStream;

            if (!fs.CanSeek)
            {
                return false;
            }

            // Try to get current encoding.
            fs.Seek(0, SeekOrigin.Begin);
            fIn.Peek();

            byte[] preamble = fIn.CurrentEncoding.GetPreamble();
            int preambleLen = preamble.Length;

            long offsetAfterFirstRead = fs.Position;

            // Check whether stream head match.
            fs.Seek(0, SeekOrigin.Begin);

            byte[] preambleCheck = new byte[preambleLen];
            fs.Read(preambleCheck, 0, preambleLen);
            fs.Seek(offsetAfterFirstRead, SeekOrigin.Begin);

            return CheckArrayMatch(preambleLen, preambleCheck, 0, preamble, 0);
        }

        /// <summary>
        /// Check whether subarray of arrayA starting from offsetA matchs with that of arrayB
        /// starting from offsetB
        /// </summary>
        /// <typeparam name="T">array element type</typeparam>
        /// <param name="lenToCompare">length of the subarray</param>
        /// <param name="arrayA">first array</param>
        /// <param name="offsetA">subarray starting point in first array</param>
        /// <param name="arrayB">second array</param>
        /// <param name="offsetB">subarray starting point in second array</param>
        /// <returns>true if match</returns>
        private static bool CheckArrayMatch<T>(int lenToCompare, T[] arrayA, int offsetA, T[] arrayB, int offsetB)
        {
            if (lenToCompare > arrayA.Length - offsetA || lenToCompare > arrayB.Length - offsetB)
            {
                return false;
            }

            for (int i = 0; i < lenToCompare; i++)
            {
                if (!arrayA[offsetA + i].Equals(arrayB[offsetB + i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
