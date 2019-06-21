using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Globalization;
using Microsoft.Hpc.Scheduler.Session;
using System.Threading;
using System.Text;
using System.Net;

namespace TestClient
{
    
    internal static class Utils
    {
        
        internal static void Log(string msg, params object[] arg)
        {
            Console.WriteLine("{0}: {1}", DateTime.Now.ToString("o", CultureInfo.GetCultureInfo("en-us")), string.Format(msg, arg));
        }

        internal static NetTcpBinding CreateNetTcpBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

            binding.OpenTimeout =
                binding.SendTimeout =
                binding.ReceiveTimeout = TimeSpan.FromMinutes(1000);

            binding.MaxReceivedMessageSize =
                binding.MaxBufferPoolSize =
                binding.MaxBufferSize =
                binding.ReaderQuotas.MaxArrayLength =
                binding.ReaderQuotas.MaxBytesPerRead =
                binding.ReaderQuotas.MaxDepth =
                binding.ReaderQuotas.MaxNameTableCharCount =
                binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

            return binding;
        }

        internal static BasicHttpBinding CreateHttpBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.UseDefaultWebProxy = false;
            binding.BypassProxyOnLocal = true;
            binding.OpenTimeout =
                 binding.SendTimeout =
                 binding.ReceiveTimeout = TimeSpan.FromMinutes(1000);

            binding.MaxReceivedMessageSize =
                binding.MaxBufferPoolSize =
                binding.MaxBufferSize =
                binding.ReaderQuotas.MaxArrayLength =
                binding.ReaderQuotas.MaxBytesPerRead =
                binding.ReaderQuotas.MaxDepth =
                binding.ReaderQuotas.MaxNameTableCharCount =
                binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

            return binding;
        }

        private static int TryParseTaskInstanceId(string taskInstanceId)
        {
            int res = 0;
            int.TryParse(taskInstanceId, out res);
            return res;
        }

        internal static ResultData CreateResultData(ComputeWithInputDataResponse response)
        {
            return new ResultData(response.ComputeWithInputDataResult.requestStartTime,
                                  response.ComputeWithInputDataResult.requestEndTime,
                                  response.ComputeWithInputDataResult.commonDataAccessStartTime,
                                  response.ComputeWithInputDataResult.commonDataAccessStopTime,
                                  TryParseTaskInstanceId(response.ComputeWithInputDataResult.CCP_TASKINSTANCEID),
                                  response.ComputeWithInputDataResult.sendStart,
                                  DateTime.Now);
        }

        internal static ResultData CreateResultData(ComputeWithInputDataPathResponse response)
        {
            return new ResultData(response.ComputeWithInputDataPathResult.requestStartTime,
                                  response.ComputeWithInputDataPathResult.requestEndTime,
                                  TryParseTaskInstanceId(response.ComputeWithInputDataPathResult.CCP_TASKINSTANCEID),
                                  response.ComputeWithInputDataPathResult.sendStart,
                                  DateTime.Now);
        }

        internal static ResultData CreateDummyResultData()
        {
            return new ResultData(DateTime.Now,
                                  DateTime.Now,
                                  DateTime.Now,
                                  DateTime.Now,
                                  -1,
                                  DateTime.Now,
                                  DateTime.Now);
        }

        internal static string[] GetOuputString(StatisticData data)
        {
            return new string[]
                                        {
                                            string.Format("Session {0} with {1}-{2} cores.", data.SessionId, (int)data.StartInfo.MinimumUnits, (int)data.StartInfo.MaximumUnits),
                                            string.Format("Number of Batches/Clients: {0}.", data.Client),
                                            string.Format("Number of Requests: {0}, Failed Requests: {1}.", data.Count, data.FaultCount),
                                            string.Format("Request Run Time: {0} milliseconds;  Request Data Size: {1} bytes; Common Data Size: {2} bytes; Response Data Size: {3} bytes.", data.Milliseconds, sizeof(int) + data.InputDataSize, data.CommonDataSize, data.OutputDataSize),
                                            string.Format("Used core: {0}", data.Used_cores),
                                            string.Format("Elapsed: {0} milliseconds.", (long)data.SessionEnd.Subtract(data.SessionStart).TotalMilliseconds),
                                            string.Format("Efficiency of CPU: {0}%.", data.EfficiencyTotal * 100),
                                            string.Format("Overall message throughput: {0}.", data.OverallThroughput),
                                            string.Format("Repro command: {0}.", data.Command),
                                        };
        }

        internal static void DrawLine(Graphics graphics, Color color, int x1, int y1, int x2, int y2)
        {
            using (Pen pen = new Pen(color))
            {
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        

        internal static void LogOutput(StatisticData data, string filename)
        {
            bool append = false;
            if (File.Exists(filename + ".txt")) append = true;
            StringBuilder sbCategory = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();
            foreach (KeyValuePair<string, object> output in data.OutputLogs)
            {
                if (!append) sbCategory.Append("\t" + output.Key);
                sbValue.Append("\t" + output.Value.ToString());
            }
            using (StreamWriter writer = new StreamWriter(filename + ".txt", append))
            {
                if (!append)
                {
                    writer.WriteLine(sbCategory.ToString().TrimStart('\t'));
                }
                writer.WriteLine(sbValue.ToString().TrimStart('\t')); 
                writer.Close();
            }
            //using (StreamWriter writer = new StreamWriter(filename + "-reqData.txt", true))
            //{
            //    writer.WriteLine("======= Repro command =========");
            //    writer.WriteLine(data.Command);
            //    writer.WriteLine("======= Detailed requests info =========");
            //    foreach (ResultData resultData in data.ResultCollection.OrderBy(ResultData => ResultData.Start))
            //    {
            //        writer.WriteLine(resultData);
            //    }
            //    writer.Close();
            //}
        }

        //internal static StatisticData LoadOutputLog(string path)
        //{
        //    if (Directory.Exists(path))
        //    {
        //        Directory.GetFiles(path)
        //    }
        //}

        //internal static StatisticData LoadOutputFile(string filepath)
        //{
        //    StatisticData data = new StatisticData();

        //}

        internal static void DrawString(Graphics graphics, string s, Font font, Color color, PointF point)
        {
            using (Brush brush = new SolidBrush(color))
            {
                graphics.DrawString(s, font, brush, point);
            }
        }


        internal static void FillRectangle(Graphics graphics, Color color, int x, int y, int width, int height)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, x, y, width, height);
            }
        }

        internal static void SaveDetail(StatisticData data, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename + ".csv"))
            {
                foreach (var re in data.ResultCollection)
                {
                    writer.WriteLine(re.ToString().TrimStart('\t'));
                }
            }
        }

        internal static void DrawChart(StatisticData data, string filename)
        {
            const int STANDARD_WIDTH = 1000;
            const int STANDARD_HEIGHT = 400;

            const string legendLine = "-------------";

            int MAX_WIDTH_ZOOM_OUT = (int)(data.Milliseconds / 5);
            if (MAX_WIDTH_ZOOM_OUT == 0)
            {
                MAX_WIDTH_ZOOM_OUT = 1;
            }

            int widthZoomOutLevel = 10;
            int heightZoomInLevel = 1;
            int leftWidth = 50;
            int rightWidth = 50;
            int widthOffset = 50;
            int topHeight = 50;
            int bottomHeight = 260;
            int heightOffset = 50;

            


            // adjust width and height
            TimeSpan E2ERrange;
            E2ERrange = data.SessionEnd.Subtract(data.SessionStart);
            DateTime startTime = data.SessionStart;
            if (Program.sleep_before_sending > 0)
            {
                TimeSpan sleepSpan = TimeSpan.FromMilliseconds(Program.sleep_before_sending);
                E2ERrange -= sleepSpan;
                startTime += sleepSpan;
            }

            int width = (int)E2ERrange.TotalMilliseconds / widthZoomOutLevel + leftWidth + rightWidth;
            if (width > STANDARD_WIDTH)
            {
                int currentZoom = (int)E2ERrange.TotalMilliseconds / (STANDARD_WIDTH - leftWidth - rightWidth);
                if (currentZoom <= MAX_WIDTH_ZOOM_OUT)
                {
                    width = STANDARD_WIDTH;
                    widthZoomOutLevel = currentZoom == 0 ? 1 : currentZoom;
                }
                else
                {
                    width = (int)E2ERrange.TotalMilliseconds / MAX_WIDTH_ZOOM_OUT + leftWidth + rightWidth;
                    widthZoomOutLevel = MAX_WIDTH_ZOOM_OUT;
                }
            }
            else
            {
                width = STANDARD_WIDTH;
            }

            int height = (int)data.StartInfo.MaximumUnits + topHeight + bottomHeight;

            if (height < STANDARD_HEIGHT)
            {
                heightZoomInLevel = (STANDARD_HEIGHT - topHeight - bottomHeight) / (int)data.StartInfo.MaximumUnits;
                heightZoomInLevel = heightZoomInLevel == 0 ? 1 : heightZoomInLevel;
                height = STANDARD_HEIGHT;
            }

            using (Bitmap image = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(image))
            using (Font normalFont = new Font("Arial", 10))
            using (Font smallFont = new Font(normalFont.FontFamily, normalFont.Size * 0.8f, normalFont.Style))
            {
                // mapping taskId->colorId
                Dictionary<int, int> colorCollection = new Dictionary<int, int>();

                Dictionary<int, int> Y = new Dictionary<int, int>();

                Color[] colors = new Color[]
                                        {
                                            Color.Red,
                                            Color.Purple,
                                            Color.Blue,
                                            Color.SkyBlue,
                                            Color.Gold,
                                            Color.Yellow,
                                            Color.Orange,
                                            Color.YellowGreen,
                                            Color.Green,
                                            Color.Brown
                                        };

                for (int i = 0; i <= (int)data.StartInfo.MaximumUnits; i++)
                {
                    colorCollection.Add(i, 0);
                }

                
                int drawHeight = height - bottomHeight;

                // draw request line
                foreach (ResultData resultData in data.ResultCollection.OrderBy(ResultData => ResultData.Start))
                {
                    
                    Color color = colors[colorCollection[resultData.TaskId]];

                    // change to next color when next time draw the line
                    colorCollection[resultData.TaskId] = (colorCollection[resultData.TaskId] + 1) % colors.Length;

                    if (!Y.ContainsKey(resultData.TaskId))
                    {
                        drawHeight -= heightZoomInLevel;
                        Y.Add(resultData.TaskId, drawHeight);
                    }

                    FillRectangle(graphics,
                                  color,
                                  (int)resultData.Start.Subtract(startTime).TotalMilliseconds / widthZoomOutLevel + leftWidth,
                                  Y[resultData.TaskId],
                                  ((int)resultData.End.Subtract(resultData.Start).TotalMilliseconds / widthZoomOutLevel) + 1,
                                  heightZoomInLevel);


                    
                }

                // draw axis
                DrawLine(graphics, Color.Black, leftWidth, height - bottomHeight, width, height - bottomHeight);
                DrawLine(graphics, Color.Black, leftWidth, topHeight, leftWidth, height - bottomHeight);

                // draw horizontal coordinate TODO: value
                for (int i = leftWidth; i < width - rightWidth; i += widthOffset)
                {
                    DrawLine(graphics, Color.Black, i, height - bottomHeight, i, height - bottomHeight - 3);
                    DrawString(graphics, ((i - leftWidth) * widthZoomOutLevel).ToString(), smallFont, Color.Black, new PointF(i, height - bottomHeight + 10));
                }

                // draw vertical coordinate TODO: value
                for (int i = height - bottomHeight; i >= topHeight; i -= heightOffset)
                {
                    DrawString(graphics, ((int)((height - bottomHeight - i) / heightZoomInLevel)).ToString(), smallFont, Color.Black, new PointF(20, i));
                }

                

                // draw boundary line
                int y = height - bottomHeight;

                int x = (int)data.SendStart.Subtract(startTime).TotalMilliseconds / widthZoomOutLevel + leftWidth;
                DrawLine(graphics, Color.Green, x, topHeight, x, y);

                if (data.ReqEom > DateTime.MinValue)
                {
                    x = (int)data.ReqEom.Subtract(startTime).TotalMilliseconds / widthZoomOutLevel + leftWidth;
                    DrawLine(graphics, Color.Orange, x, topHeight, x, y);
                }

                x = (int)data.RetrieveEnd.Subtract(startTime).TotalMilliseconds / widthZoomOutLevel + leftWidth;
                DrawLine(graphics, Color.Brown, x, topHeight, x, y);

                x = (int)data.SessionEnd.Subtract(startTime).TotalMilliseconds / widthZoomOutLevel + leftWidth;
                DrawLine(graphics, Color.Blue, x, topHeight, x, y);

                // draw statistic info
                string[] strings = Utils.GetOuputString(data);

                int stringOffset = 15;
                drawHeight = height - bottomHeight + stringOffset * 2;
                foreach (string str in strings)
                {
                    DrawString(graphics, str, normalFont, Color.Black, new PointF(leftWidth, drawHeight));
                    drawHeight += stringOffset;
                }

                // draw legend
                Dictionary<string, Color> dic = new Dictionary<string, Color>();
                dic.Add("Begin to send requests: ", Color.Green);
                if (data.ReqEom > DateTime.MinValue)
                {
                    dic.Add("Begin to call EOM: ", Color.Orange);
                }

                dic.Add("All requests returned: ", Color.Brown);
                dic.Add("Session closed: ", Color.Blue);

                foreach (KeyValuePair<string, Color> pair in dic)
                {
                    DrawString(graphics, pair.Key + legendLine, normalFont, pair.Value, new PointF(leftWidth, drawHeight));
                    drawHeight += stringOffset;
                }
                if (!File.Exists(filename + ".png")) image.Save(filename + ".png", ImageFormat.Png);
                else
                {
                    int i = 0;
                    while (i < int.MaxValue && File.Exists(filename + i.ToString() + ".png")) i++;
                    image.Save(filename + i.ToString() + ".png");
                }

            }
        }
    }

    internal class ResponseHandlerBase
    {
        public BrokerClient<IService1> client;
        internal string clientId;
        public int faultCalls = 0;
        public List<ResultData> results = new List<ResultData>();
        internal AutoResetEvent batchDone = new AutoResetEvent(false);
        protected bool ignoreRetryOperationError = false;

        public ResponseHandlerBase(BrokerClient<IService1> client, string clientId, bool ignoreRetryOperationError)
        {
            this.client = client;
            this.clientId = clientId;
            this.ignoreRetryOperationError = ignoreRetryOperationError;
        }

        public void WaitOne()
        {
            batchDone.WaitOne();
        }
    }

    internal class ComputeWithInputDataResponseHandler : ResponseHandlerBase
    {
        private long commonData_Size;

        public ComputeWithInputDataResponseHandler(BrokerClient<IService1> client, string clientId, long commonData_Size, bool ignoreRetryOperationError)
            : base(client, clientId, ignoreRetryOperationError)
        {
            this.commonData_Size = commonData_Size;
        }


        public void ResponseHandler<T>(BrokerResponse<ComputeWithInputDataResponse> response)
        {
            lock (results)
            {
                try
                {
                    if (commonData_Size > 0 && response.Result.ComputeWithInputDataResult.commonDataSize != commonData_Size)
                    {
                        throw new Exception(string.Format("Common data is corrupted: expected: {0}, actual: {1}", commonData_Size, response.Result.ComputeWithInputDataResult.commonDataSize));
                    }
                    results.Add(Utils.CreateResultData(response.Result));
                }
                catch (WebException e)
                {
                    if (e.Response is HttpWebResponse)
                    {
                        HttpWebResponse httpresponse = e.Response as HttpWebResponse;
                        using (StreamReader reader = new StreamReader(httpresponse.GetResponseStream()))
                        {
                            Utils.Log("Unexpected WebException when client {0} sending requests: {1}", clientId, httpresponse.StatusCode, reader.ReadToEnd());
                        }
                        results.Add(Utils.CreateDummyResultData());
                        Interlocked.Increment(ref faultCalls);
                    }
                    else throw e;
                }
                catch (RetryOperationException)
                {
                    if (ignoreRetryOperationError)
                    {
                        results.Add(Utils.CreateDummyResultData());
                    }
                    else throw;
                }
                catch (Exception e)
                {
                    Utils.Log(string.Format("Client {0}, Unexpected Exception happened: {1}", clientId, e.ToString()));
                    results.Add(Utils.CreateDummyResultData());
                    Interlocked.Increment(ref faultCalls);
                }
                if (response.IsLastResponse)
                {
                    Utils.Log("Client {0}: All requests returned.", clientId);
                    batchDone.Set();
                }
            }
        }
    }

    internal class ComputeWithInputDataPathResponseHandler : ResponseHandlerBase
    {
        public ComputeWithInputDataPathResponseHandler(BrokerClient<IService1> client, string clientId, bool ignoreRetryOperationError)
            : base(client, clientId, ignoreRetryOperationError)
        {
        }


        public void ResponseHandler<T>(BrokerResponse<ComputeWithInputDataPathResponse> response)
        {
            lock (results)
            {
                try
                {
                    results.Add(Utils.CreateResultData(response.Result));
                }
                catch (WebException e)
                {
                    if (e.Response is HttpWebResponse)
                    {
                        HttpWebResponse httpresponse = e.Response as HttpWebResponse;
                        using (StreamReader reader = new StreamReader(httpresponse.GetResponseStream()))
                        {
                            Utils.Log("Unexpected WebException when client {0} sending requests: {1}", clientId, httpresponse.StatusCode, reader.ReadToEnd());
                        }
                        results.Add(Utils.CreateDummyResultData());
                        Interlocked.Increment(ref faultCalls);
                    }
                    else throw e;
                }
                catch (RetryOperationException)
                {
                    if (ignoreRetryOperationError)
                    {
                        results.Add(Utils.CreateDummyResultData());
                    }
                    else throw;
                }
                catch (Exception e)
                {
                    Utils.Log(e.ToString());
                    results.Add(Utils.CreateDummyResultData());
                    Interlocked.Increment(ref faultCalls);
                }
                if (response.IsLastResponse)
                {
                    Utils.Log("Client {0}: All requests returned.", clientId);
                    batchDone.Set();
                }
            }
        }
    }
}
