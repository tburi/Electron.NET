using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ElectronNET.API
{
    /// <summary>
    /// 
    /// </summary>
    public class ElectronDebugLauncher : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private Process _process;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="hostApplicationLifetime"></param>
        public ElectronDebugLauncher(IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime)
        {
            _configuration = configuration;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var hostFolder = _configuration["hostFolder"];
            var binFolder = _configuration["api-bin-folder"] ?? GetBinFolder();
            Console.WriteLine($"Using hostFolder: {hostFolder}");
            Console.WriteLine($"Using ASP.NETCore backend binary folder: {binFolder}");
            _process = ProcessHelper.CmdExecute($"{hostFolder}node_modules\\.bin\\electron.cmd \"{hostFolder}main.js\" vs-debug-bin={binFolder}",".", true);
            _process.WaitForExit();
            _hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        private string GetBinFolder()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {            
            return Task.CompletedTask;
        }
    }
}
