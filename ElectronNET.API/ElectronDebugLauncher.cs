using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        private TaskCompletionSource<string> _socketPortPromise = new TaskCompletionSource<string>();
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
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var hostFolder = _configuration["electronHostFolder"] ?? GetHostFolder();
            var binFolder = GetBinFolder();
            Console.WriteLine($"Using Electron App Host Folder: {hostFolder}");
            Console.WriteLine($"Using ASP.NET Core backend binary folder: {binFolder}");
            _process = HandleElectronProcess($"{Path.Combine(hostFolder, "node_modules\\.bin\\electron.cmd")} \"{Path.Combine(hostFolder, "main.js")}\" vs-debug-bin={binFolder}", ".");
            var socketPort = await _socketPortPromise.Task;
            BridgeSettings.SocketPort = socketPort;
        }

        private string GetHostFolder()
        {            
            if (!Directory.Exists("./obj/Host"))
            {
                throw new InvalidOperationException("you need to set the working directory to \".\" (project folder) or the --electronHostFolder in commandLineArgs to for current launch profile");
            }
            var cwd = Directory.GetCurrentDirectory();
            return Path.Combine(cwd,"obj\\Host\\");
        }

        private string GetBinFolder()
        {
            return AppDomain.CurrentDomain.BaseDirectory;            
        }

        private Process HandleElectronProcess(string command, string workingDirectoryPath)
        {
            Process cmd = new Process();
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                cmd.StartInfo.FileName = "cmd.exe";
            }
            else
            {
                // works for OSX and Linux (at least on Ubuntu)
                cmd.StartInfo.FileName = "bash";
            }

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = workingDirectoryPath;
            cmd.EnableRaisingEvents = true;

            cmd.OutputDataReceived += (s, e) =>
            {
                if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
                {
                    if (e.Data.StartsWith("Electron Socket IO Port: "))
                    {
                        var socketPort = e.Data.Replace("Electron Socket IO Port: ", "").Trim();
                        _socketPortPromise.SetResult(socketPort);
                    }
                    Console.WriteLine("Electron:stdout:" + e.Data);
                }
            };
            cmd.ErrorDataReceived += (s, e) =>
            {
                if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
                {
                    if (!_socketPortPromise.Task.IsCompleted)
                    {
                        _socketPortPromise.SetCanceled();
                    }
                    Console.WriteLine("Electron:stderr:" + e.Data);
                }
            };
            cmd.Exited += (s, e) =>
            {
                if (!_socketPortPromise.Task.IsCompleted)
                {
                    _socketPortPromise.SetCanceled();
                }
                Console.WriteLine("Electron:exited");
                _hostApplicationLifetime.StopApplication();
            };

            cmd.Start();
            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            return cmd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process != null && !_process.HasExited)
            {
                // close electron windows if the backend process is exiting
                foreach (var window in Electron.WindowManager.BrowserWindows)
                {
                    window.Close();
                }
            }
            return Task.CompletedTask;
        }
    }
}
