// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Scheduler;

namespace SchedulerClient
{
    public class Program
    {
        private static readonly string[] Schedulers = new string[]
            {"https://localhost:50051", "https://localhost:50052", "https://localhost:50053"};

        private static readonly GrpcChannel[] Channels = new GrpcChannel[]
        {
            GrpcChannel.ForAddress("https://localhost:50051"), GrpcChannel.ForAddress("https://localhost:50052"),
            GrpcChannel.ForAddress("https://localhost:50053")
        };

        public static void Main(string[] args)
        {
            const int taskNum = 1000;
            const int estimateTaskExecuteTime = 1000;
            const int totalCoreNum = 10;
            List<AsyncUnaryCall<HelloReply>> replies = new List<AsyncUnaryCall<HelloReply>>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Start to send requests");
            for (int i = 0; i < taskNum; i++)
            {
                int num = GetRandomNum();
                var client = new Scheduler.Scheduler.SchedulerClient(Channels[num]);
                Console.WriteLine("Send request to " + Schedulers[num]);
                var reply = client.SayHelloAsync(new HelloRequest {Id = i, Scheduler = Schedulers[num]});
                replies.Add(reply);
            }
            Console.WriteLine("End to send requests");
            Task resultTask = Task.WhenAll(replies.Select(reply => reply.ResponseAsync))
                .ContinueWith(task =>
                {
                    stopwatch.Stop();
                    StreamWriter file = new StreamWriter(@".\TestFolder\result.txt", true);
                    file.WriteLine("*******************Start******************");
                    Dictionary<string, int> result = new Dictionary<string, int>();
                    for (int i = 0; i < taskNum; i++)
                    {
                        var res = replies[i].ResponseAsync.Result;
                        if (result.ContainsKey(res.Node))
                        {
                            result[res.Node]++;
                        }
                        else
                        {
                            result.Add(res.Node, 1);
                        }
                    }

                    foreach (KeyValuePair<string, int> item in result)
                    {
                        file.WriteLine(item.Key + " executed " + item.Value + " tasks.");
                    }

                    long actualTime = stopwatch.ElapsedMilliseconds;
                    long elapsedTime = (taskNum / totalCoreNum) * estimateTaskExecuteTime;
                    file.WriteLine("Actual elapsed time is " + actualTime + "ms");
                    file.WriteLine("Supposed elapsed time is " + elapsedTime + "ms");
                    file.WriteLine("Efficiency in this test is " + (((double) elapsedTime / actualTime) * 100) + "%");
                    file.WriteLine("*******************End******************");
                    file.Flush();
                    file.Close();
                });

            try
            {
                resultTask.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static int GetRandomNum()
        {
            Random rnd = new Random();
            return rnd.Next(0, 3);
        }
    }
}