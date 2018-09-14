using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Hpc.Scheduler.Store;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{
    class LineBuffer
    {
        const byte Return = 10;
        const byte NewLine = 13;

        Stream buffer = new MemoryStream();
        Encoding encoding;

        internal LineBuffer(Encoding encoding)
        {
            this.encoding = encoding;
        }

        internal IEnumerable<string> ProcessOutput(byte[] data)
        {
            IEnumerable<string> lines = null;

            //write it to the line buffer
            int lastNewLine = Array.FindLastIndex<byte>(data, IsNewLine);

            //if there is new line in the data, write everything before last new line and process new line
            if (lastNewLine != -1)
            {
                buffer.Write(data, 0, lastNewLine + 1);
                lines = FlushLines();
            }

            //write everything after the new line to stream
            buffer.Write(data, lastNewLine + 1, data.Length - lastNewLine - 1);

            return lines;
        }

        static bool IsNewLine(byte b)
        {
            return b == Return || b == NewLine;
        }

        internal IEnumerable<string> FlushLines()
        {
            List<string> lines = new List<string>();
            Stream stream = Interlocked.Exchange<Stream>(ref buffer, new MemoryStream());
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream, encoding);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }

        internal Encoding Encoding
        {
            get { return encoding; }
        }
    }

    class NodeData
    {
        LineBuffer outBuffer;
        string nodeName;
        bool eofSent;
        bool finished;
        NodeLocation location;
        IClusterTask task;
        Encoding encoding;
        TaskState _lastState = TaskState.Configuring;

        internal NodeData(string nodeName, Encoding encoding, NodeLocation location)
        {
            this.nodeName = nodeName;
            this.location = location;
            this.encoding = encoding;
            this.outBuffer = new LineBuffer(encoding);
        }

        internal string NodeName
        {
            get { return nodeName; }
        }

        internal LineBuffer OutBuffer
        {
            get { return outBuffer; }
        }

        internal bool EofSent
        {
            get { return eofSent; }
            set { eofSent = value; }
        }

        internal bool Finished
        {
            get { return finished; }
            set { finished = value; }
        }

        internal IClusterTask Task
        {
            get { return task; }
            set { task = value; }
        }

        internal NodeLocation Location
        {
            get { return location; }
        }

        internal TaskState LastState
        {
            get { return _lastState; }
            set { _lastState = value; }
        }

    }
}
