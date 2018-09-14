using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.Hpc.EchoSvcClient
{
    internal class NodeTestResult
    {
        const string NodeNameTag = "NodeName";
        const string SuccessTag = "Success";
        const string ExceptionTag = "Exception";
        const string LatencyTag = "Latency";
        const string NetTcpTag = "NetTcp";
        const string HttpTag = "Http";
        public const string TestStartTag = "Test";

        private string node = null;
        private bool netTcpSuccess = false;
        private bool httpSuccess = false;
        private double netTcpLatency = -1.0;
        private double httpLatency = -1.0;
        private string netTcpException = "";
        private string httpException = "";

        public NodeTestResult(string node)
        {
            this.node = node;
        }

        public String NodeName
        {
            get { return node; }
        }

        public bool NetTcpSuccess
        {
            get { return netTcpSuccess; }
        }

        public bool HttpSuccess
        {
            get { return httpSuccess; }
        }

        public double NetTcpLatency
        {
            get { return netTcpLatency; }
        }

        public double HttpLatency
        {
            get { return httpLatency; }
        }

        public string NetTcpException
        {
            get { return netTcpException; }
        }

        public string HttpException
        {
            get { return httpException; }
        }

        public void SetNetTcpSuccess(double latency)
        {
            netTcpSuccess = true;
            netTcpLatency = latency;
        }

        public void SetNetTcpFail(string exceptionMsg)
        {
            netTcpSuccess = false;
            netTcpException = exceptionMsg;
        }

        public void SetHttpSuccess(double latency)
        {
            httpSuccess = true;
            httpLatency = latency;
        }

        public void SetHttpFail(string exceptionMsg)
        {
            httpSuccess = false;
            httpException = exceptionMsg;
        }

        public void WriteResult(XmlWriter writer)
        {
            writer.WriteStartElement(TestStartTag);
            writer.WriteAttributeString(NodeNameTag, node);

            writer.WriteStartElement(NetTcpTag);
            writer.WriteAttributeString(SuccessTag, netTcpSuccess.ToString());
            writer.WriteAttributeString(LatencyTag, netTcpLatency.ToString());
            writer.WriteAttributeString(ExceptionTag, netTcpException);
            writer.WriteEndElement();

            writer.WriteStartElement(HttpTag);
            writer.WriteAttributeString(SuccessTag, httpSuccess.ToString());
            writer.WriteAttributeString(LatencyTag, httpLatency.ToString());
            writer.WriteAttributeString(ExceptionTag, httpException);
            writer.WriteEndElement();

            writer.WriteEndElement();

        }

        internal static NodeTestResult ReadResult(XmlTextReader reader)
        {
            NodeTestResult result = new NodeTestResult(reader.GetAttribute(NodeNameTag));
            while (reader.Read())
            {
                if (reader.Name == TestStartTag &&
                    reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.Name == NetTcpTag &&
                    reader.NodeType == XmlNodeType.Element)
                {
                    if (bool.Parse(reader.GetAttribute(SuccessTag)))
                    {
                        result.SetNetTcpSuccess(double.Parse(reader.GetAttribute(LatencyTag)));
                    }
                    else
                    {
                        result.SetNetTcpFail(reader.GetAttribute(ExceptionTag));
                    }
                }

                if (reader.Name == HttpTag &&
                    reader.NodeType == XmlNodeType.Element)
                {
                    if (bool.Parse(reader.GetAttribute(SuccessTag)))
                    {
                        result.SetHttpSuccess(double.Parse(reader.GetAttribute(LatencyTag)));
                    }
                    else
                    {
                        result.SetHttpFail(reader.GetAttribute(ExceptionTag));
                    }
                }
            }

            return result;
        }
    }
}
