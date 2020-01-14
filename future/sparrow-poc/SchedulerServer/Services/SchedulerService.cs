// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Worker;

namespace Scheduler
{
    public class SchedulerService : Scheduler.SchedulerBase
    {
        private static readonly string[] NodeLists = new string[]
        {
            "https://localhost:50054", "https://localhost:50055", "https://localhost:50056", "https://localhost:50057",
            "https://localhost:50058"
        };

        private static readonly GrpcChannel[] Channels = new GrpcChannel[]
        {
            GrpcChannel.ForAddress("https://localhost:50054"), GrpcChannel.ForAddress("https://localhost:50055"),
            GrpcChannel.ForAddress("https://localhost:50056"), GrpcChannel.ForAddress("https://localhost:50057"),
            GrpcChannel.ForAddress("https://localhost:50058")
        };

        private static ConcurrentQueue<TaskReply> _tasks = new ConcurrentQueue<TaskReply>();

        private static ConcurrentDictionary<int, TaskCompletionSource<HelloReply>> _tcs =
            new ConcurrentDictionary<int, TaskCompletionSource<HelloReply>>();

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine(request.Scheduler + " received task " + request.Id);
            _tasks.Enqueue(new TaskReply
            {
                Id = request.Id,
                Node = string.Empty,
                Data = "Mocked data for task " + request.Id + " from scheduler " + request.Scheduler,
                Timestamp = "-1",
                Scheduler = request.Scheduler
            });

            TaskCompletionSource<HelloReply> tcs1 = new TaskCompletionSource<HelloReply>();
            Task<HelloReply> t1 = tcs1.Task;
            _tcs.GetOrAdd(request.Id, tcs1);

            for (int i = 0; i < NodeLists.Length; i++)
            {
                ProbeRequest probeRequest = new ProbeRequest
                {
                    Id = request.Id, Scheduler = request.Scheduler, Node = NodeLists[i],
                    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString()
                };
                SendProbeAsync(i, probeRequest, null);
            }

            return t1;
        }

        public override Task<TaskReply> GetTask(TaskRequest request, ServerCallContext context)
        {
            if (_tasks.TryDequeue(out TaskReply task))
            {
                task.Node = request.Node;
                task.Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
                return Task.FromResult(task);
            }

            return Task.FromResult(new TaskReply
            {
                Id = -1,
                Node = request.Node,
                Data = string.Empty,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString(),
                Scheduler = request.Scheduler
            });
        }

        public override Task<Empty> CompleteTask(HelloReply reply, ServerCallContext context)
        {
            if (_tcs.TryRemove(reply.Id, out TaskCompletionSource<HelloReply> targetTsc))
            {
                targetTsc.SetResult(reply);
            }

            return Task.FromResult(new Empty { });
        }

        public async Task<Empty> SendProbeAsync(int i, ProbeRequest request, ServerCallContext context)
        {
            var client = new Worker.Worker.WorkerClient(Channels[i]);
            await client.SendProbeAsync(request);
            return new Empty { };
        }
    }
}