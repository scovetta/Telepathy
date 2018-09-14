using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.Hpc.EchoSvcClient
{
    internal class ServiceTestResult
    {
        const string CcpServiceTestTag = "CcpServiceTest";
        const string DateTag = "Date";

        const string HeadNodeTag = "HeadNode";
        const string BrokerNodeTag = "BrokerNode";
        const string TimeOutTag = "Timeout";
        const string MessageCountTag = "MessageCount";

        const string BrokerConfiguredTag = "BrokerConfigured";
        const string NumSuccessTag = "NumberOfSuccessfulNodes";
        const string NumFailedTag = "NumberOfFailedNodes";

        const string NetTcpTag = "NetTcp";
        const string HttpTag = "Http";

        const string SuccessNodesTag = "SuccessfulNodes";
        const string FailedNodesTag = "FailedNodes";
        const string NodeTag = "Node";
        const string NameTag = "Name";

        const string InitConfigTag = "InitialConfigs";
        const string AggResultTag = "AggregatedResults";
        const string DetailsTag = "DetailResults";

        private string headnode;
        private string brokernode;
        private int timeout;
        private int msgcnt;

        private bool brokerConfigured;

        private List<NodeTestResult> nodeResults;
        private Dictionary<string, string> netTcpFailedNodes;
        private Dictionary<string, string> httpFailedNodes;
        private List<string> netTcpSuccessNodes;
        private List<string> httpSuccessNodes;
        private int[] netTcpLatencyZones = new int[3] { 0, 0, 0 };
        private int[] httpLatencyZones = new int[3] { 0, 0, 0 };

        public Dictionary<string, string> HttpFailedNodes
        {
            get { return httpFailedNodes; }
        }

        public Dictionary<string, string> NetTcpFailedNodes
        {
            get { return netTcpFailedNodes; }
        }

        public List<string> HttpSuccessNodes
        {
            get { return httpSuccessNodes; }
        }

        public List<string> NetTcpSuccessNodes
        {
            get { return netTcpSuccessNodes; }
        }

        public List<NodeTestResult> NodeResults
        {
            get { return nodeResults; }
        }

        public int[] NetTcpLatencyZones
        {
            get { return netTcpLatencyZones; }
        }

        public int[] HttpLatencyZones
        {
            get { return httpLatencyZones; }
        }

        public bool BrokerConfigured
        {
            get { return brokerConfigured; }
        }

        public ServiceTestResult(
            string headnode,
            string brokernode,
            int timeout,
            int msgcnt,
            bool brokerConfigured)
        {
            nodeResults = new List<NodeTestResult>();
            netTcpFailedNodes = new Dictionary<string, string>();
            netTcpSuccessNodes = new List<string>();
            httpFailedNodes = new Dictionary<string, string>();
            httpSuccessNodes = new List<string>();

            this.headnode = headnode;
            this.brokernode = brokernode;
            this.timeout = timeout;
            this.msgcnt = msgcnt;
            this.brokerConfigured = brokerConfigured;
        }

        public void AddNodeTestResult(NodeTestResult nodeResult)
        {
            nodeResults.Add(nodeResult);

            if (nodeResult.HttpSuccess)
            {
                httpSuccessNodes.Add(nodeResult.NodeName);
                httpLatencyZones[getLatencyIndex(nodeResult.HttpLatency)]++;
            }
            else
            {
                httpFailedNodes.Add(nodeResult.NodeName, nodeResult.HttpException);
            }

            if (nodeResult.NetTcpSuccess)
            {
                netTcpSuccessNodes.Add(nodeResult.NodeName);
                netTcpLatencyZones[getLatencyIndex(nodeResult.NetTcpLatency)]++;
            }
            else
            {
                netTcpFailedNodes.Add(nodeResult.NodeName, nodeResult.NetTcpException);
            }

        }

        private int getLatencyIndex(double latency)
        {
            if (latency <= 5)
            {
                return 0;
            }

            if (latency > 10)
            {
                return 2;
            }

            return 1;
        }

        internal void PersistToXml(string outputFile)
        {
            XmlWriter writer = XmlWriter.Create(outputFile);

            writer.WriteStartDocument();
            writer.WriteStartElement(CcpServiceTestTag);
            writer.WriteAttributeString(DateTag, DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());

            writeInitialConfigs(writer);
            writeAggregateResults(writer);
            writeDetails(writer);

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public static ServiceTestResult LoadFromXml(string inputFile)
        {
            XmlTextReader reader = new XmlTextReader(inputFile);
            ServiceTestResult result = null;
            List<NodeTestResult> nodeResults = new List<NodeTestResult>();
            while (reader.Read())
            {
                if (reader.Name == InitConfigTag && reader.NodeType == XmlNodeType.Element)
                {
                    result = readInitialConfigs(reader);
                }

                if (reader.Name == DetailsTag && reader.NodeType == XmlNodeType.Element)
                {
                    readDetails(reader, nodeResults);
                }

            }

            foreach (NodeTestResult res in nodeResults)
            {
                result.AddNodeTestResult(res);
            }
            reader.Close();

            return result;
        }

        private void writeInitialConfigs(XmlWriter writer)
        {
            writer.WriteStartElement(InitConfigTag);

            writer.WriteAttributeString(HeadNodeTag, headnode);
            writer.WriteAttributeString(BrokerNodeTag, brokernode);
            writer.WriteAttributeString(TimeOutTag, timeout.ToString());
            writer.WriteAttributeString(MessageCountTag, msgcnt.ToString());
            writer.WriteAttributeString(BrokerConfiguredTag, brokerConfigured.ToString());

            writer.WriteEndElement();
        }

        private static ServiceTestResult readInitialConfigs(XmlTextReader reader)
        {
            return new ServiceTestResult(
                reader.GetAttribute(HeadNodeTag),
                reader.GetAttribute(BrokerNodeTag),
                int.Parse(reader.GetAttribute(TimeOutTag)),
                int.Parse(reader.GetAttribute(MessageCountTag)),
                bool.Parse(reader.GetAttribute(BrokerConfiguredTag)));
        }

        private void writeAggregateResults(XmlWriter writer)
        {
            writer.WriteStartElement(AggResultTag);

            {
                writer.WriteStartElement(HttpTag);
                writer.WriteAttributeString(NumFailedTag, httpFailedNodes.Count.ToString());
                writer.WriteAttributeString(NumSuccessTag, httpSuccessNodes.Count.ToString());

                {
                    writer.WriteStartElement(SuccessNodesTag);

                    foreach (string node in httpSuccessNodes)
                    {
                        writeNodeElement(writer, node);
                    }
                    writer.WriteEndElement();
                }
                {
                    writer.WriteStartElement(FailedNodesTag);

                    foreach (string node in httpFailedNodes.Keys)
                    {
                        writeNodeElement(writer, node);
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

            }

            {
                writer.WriteStartElement(NetTcpTag);
                writer.WriteAttributeString(NumFailedTag, netTcpFailedNodes.Count.ToString());
                writer.WriteAttributeString(NumSuccessTag, netTcpSuccessNodes.Count.ToString());

                {
                    writer.WriteStartElement(SuccessNodesTag);

                    foreach (string node in netTcpSuccessNodes)
                    {
                        writeNodeElement(writer, node);
                    }
                    writer.WriteEndElement();
                }
                {
                    writer.WriteStartElement(FailedNodesTag);

                    foreach (string node in netTcpFailedNodes.Keys)
                    {
                        writeNodeElement(writer, node);
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

            }
            writer.WriteEndElement();
        }

        private void writeNodeElement(XmlWriter writer, string node)
        {
            writer.WriteStartElement(NodeTag);
            writer.WriteAttributeString(NameTag, node);
            writer.WriteEndElement();
        }

        private void writeDetails(XmlWriter writer)
        {
            writer.WriteStartElement(DetailsTag);
            foreach (NodeTestResult result in nodeResults)
            {
                result.WriteResult(writer);
            }
            writer.WriteEndElement();
        }

        private static void readDetails(XmlTextReader reader, List<NodeTestResult> results)
        {
            while (reader.Read())
            {
                if (reader.Name == NodeTestResult.TestStartTag &&
                    reader.NodeType == XmlNodeType.Element)
                {
                    results.Add(NodeTestResult.ReadResult(reader));
                }

                if (reader.Name == DetailsTag && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }
        }
    }

}
