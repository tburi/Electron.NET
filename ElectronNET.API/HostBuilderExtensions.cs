using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace ElectronNET.API
{
    /// <summary>
    /// 
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Add Electron support for this .NET Core Project.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="args">The arguments.</param>        
        /// <returns></returns>
        public static IHostBuilder UseElectron(this IHostBuilder builder, string[] args)
        {
            var vsDebug = args.Any(arg => arg.Contains("vs-debug", StringComparison.InvariantCultureIgnoreCase));
            if (vsDebug)
            {
                builder.SetupVsDebug(args);
                return builder;
            }
            foreach (string argument in args)
            {
                if (argument.ToUpper().Contains("ELECTRONPORT"))
                {
                    BridgeSettings.SocketPort = argument.ToUpper().Replace("/ELECTRONPORT=", "");
                    Console.WriteLine("Use Electron Port: " + BridgeSettings.SocketPort);
                }
                else if (argument.ToUpper().Contains("ELECTRONWEBPORT"))
                {
                    BridgeSettings.WebPort = argument.ToUpper().Replace("/ELECTRONWEBPORT=", "");
                }
            }

            if (HybridSupport.IsElectronActive)
            {
                builder.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory);
                builder.ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseUrls("http://127.0.0.1:" + BridgeSettings.WebPort);
                });
            }
            return builder;
        }

        private static IHostBuilder SetupVsDebug(this IHostBuilder builder, string[] args)
        {
            BridgeSettings.IsVsDebugEnabled = true;

            var aspNetBackendPort = args.SingleOrDefault(arg => arg.StartsWith("--vs-debug-port", StringComparison.InvariantCultureIgnoreCase))
                                       ?.Split("=")
                                       ?.ElementAtOrDefault(1)
                                       ?? "5000";
            BridgeSettings.WebPort = aspNetBackendPort;
    
            builder.ConfigureServices((context, services) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddHostedService<ElectronDebugLauncher>();
                }
            });
            builder.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory);
            builder.ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseUrls("http://127.0.0.1:" + BridgeSettings.WebPort);
            });

            return builder;
        }
    }
}
