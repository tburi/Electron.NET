using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ElectronNET.API
{
    /// <summary>
    /// 
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Use a Electron support for this .NET Core Project.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IHostBuilder UseElectron(this IHostBuilder builder, string[] args)
        {
            builder.ConfigureServices((context, services) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddHostedService<ElectronDebugLauncher>();
                }
            });

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
    }
}
