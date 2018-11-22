using System;
using System.Globalization;

namespace TestClient
{
    internal class ResultData
    {
        private DateTime start = DateTime.MinValue;

        internal DateTime Start
        {
            get { return start; }
            set { start = value; }
        }


        private DateTime end = DateTime.MaxValue;

        internal DateTime End
        {
            get { return end; }
            set { end = value; }
        }

        private DateTime sendStart = DateTime.MaxValue;

        internal DateTime SendStart
        {
            get { return sendStart; }
            set { sendStart = value; }
        }

        private DateTime received = DateTime.MaxValue;

        internal DateTime Received
        {
            get { return received; }
            set { received = value; }
        }

        private DateTime dataAccessStart = DateTime.MinValue;

        public DateTime DataAccessStart
        {
            get { return dataAccessStart; }
            set { dataAccessStart = value; }
        }

        private DateTime dataAccessStop = DateTime.MinValue;

        public DateTime DataAccessStop
        {
            get { return dataAccessStop; }
            set { dataAccessStop = value; }
        }

        internal int TaskId
        {
            get;
            set;
        }


        internal ResultData()
        { }


        internal ResultData(DateTime start, DateTime end, DateTime dataAccessStart, DateTime dataAccessStop, int tid, DateTime sendStart, DateTime received)
        {
            this.start = start;
            this.end = end;
            this.TaskId = tid;
            this.dataAccessStart = dataAccessStart;
            this.dataAccessStop = dataAccessStop;
            this.sendStart = sendStart;
            this.received = received;
        }

        internal ResultData(DateTime start, DateTime end, int tid, DateTime sendStart, DateTime received)
        {
            this.start = start;
            this.end = end;
            this.TaskId = tid;
            this.sendStart = sendStart;
            this.received = received;
        }

        public override string ToString()
        {
            return string.Format(
                $@"{this.TaskId},{this.sendStart.ToString("o", CultureInfo.GetCultureInfo("en-us"))},{
                        this.Start.ToString("o", CultureInfo.GetCultureInfo("en-us"))
                    },{this.End.ToString("o", CultureInfo.GetCultureInfo("en-us"))},{
                        this.received.ToString("o", CultureInfo.GetCultureInfo("en-us"))
                    }");
        }
    }
}
