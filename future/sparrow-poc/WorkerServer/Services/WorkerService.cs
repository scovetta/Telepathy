// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogHelper;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Scheduler;

namespace Worker
{
    public class WorkerService : Worker.WorkerBase
    {
        // Can parallel run 2 requests at one time
        private static int _availableCoreNum = 2;
        private static ConcurrentDictionary<string, string> _schedulers = new ConcurrentDictionary<string, string>();
        private static readonly GrpcChannel Scheduler1 = GrpcChannel.ForAddress("https://localhost:50051");
        private static readonly GrpcChannel Scheduler2 = GrpcChannel.ForAddress("https://localhost:50052");
        private static readonly GrpcChannel Scheduler3 = GrpcChannel.ForAddress("https://localhost:50053");

        private static readonly Dictionary<string, GrpcChannel> Channels = new Dictionary<string, GrpcChannel>()
        {
            {"https://localhost:50051", Scheduler1},
            {"https://localhost:50052", Scheduler2},
            {"https://localhost:50053", Scheduler3}
        };

        public override Task<Empty> SendProbe(ProbeRequest request, ServerCallContext context)
        {
            MakeReservation(request);
            return Task.FromResult(new Empty { });
        }

        public void MakeReservation(ProbeRequest request)
        {
            _schedulers.AddOrUpdate(request.Scheduler, request.Timestamp, (k, v) => request.Timestamp);

            if (_availableCoreNum > 0)
            {
                int temp = _availableCoreNum;
                _availableCoreNum = 0;

                for (int i = 0; i < temp; i++)
                {
                    string index = Logger.GetTimestamp();
                    Task.Run(async () => await GetTaskAsync(request, index));
                }
            }
        }

        public async Task GetTaskAsync(ProbeRequest request, string i)
        {
            if (Channels.TryGetValue(request.Scheduler, out GrpcChannel channel))
            {
                var schedulerClient = new Scheduler.Scheduler.SchedulerClient(channel);
                TaskRequest taskRequest = new TaskRequest {Node = request.Node, Scheduler = request.Scheduler};
                var reply = await schedulerClient.GetTaskAsync(taskRequest);
                // No more task need to be executed
                if (reply.Id == -1)
                {
                    // try to remove current scheduler from schedulers dic according to timestamp
                    if (_schedulers.TryGetValue(request.Scheduler, out string requestTime))
                    {
                        long recordTime = Convert.ToInt64(requestTime);
                        long queryTime = Convert.ToInt64(reply.Timestamp);

                        // only the time record in dics saller than the query task time, which means that no more probe arrive in current node,
                        // then remove idle scheduler from dics
                        if (recordTime < queryTime)
                        {
                            _schedulers.TryRemove(request.Scheduler, out string record);
                        }
                    }

                    // try to change scheduler to acquire tasks
                    if (_schedulers.Count > 0)
                    {
                        string scheduler = _schedulers.Take(1).Select(item => item.Key).First();
                        await GetTaskAsync(new ProbeRequest {Node = request.Node, Scheduler = scheduler}, i);
                    }
                    else
                    {
                        // return core only no more task can be acquired from any scheduler
                        _availableCoreNum++;
                    }
                }
                else
                {
                    await ExecuteTask(reply, schedulerClient, i);
                }
            }
        }

        private async Task ExecuteTask(TaskReply reply, Scheduler.Scheduler.SchedulerClient client, string i)
        {
            await Task.Delay(1000);
            await CompleteTask(reply, client, i);
        }

        private async Task CompleteTask(TaskReply reply, Scheduler.Scheduler.SchedulerClient client, string i)
        {
            HelloReply helloReply = new HelloReply
            {
                Id = reply.Id, Message = "Task " + reply.Id + " executed in node " + reply.Node, Node = reply.Node,
                Scheduler = reply.Scheduler
            };
            Console.WriteLine(Logger.GetTimestamp() + "Task " + reply.Id + " try to be completed");
            await client.CompleteTaskAsync(helloReply);
            await GetTaskAsync(new ProbeRequest {Node = reply.Node, Scheduler = reply.Scheduler}, i);
        }
    }
}