// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SparrowPoC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (args.Length > 0)
                    {
                        webBuilder.UseUrls("https://127.0.0.1:" + args[0] + "/");
                    }
                    webBuilder.UseStartup<Startup>();
                });
    }
}
