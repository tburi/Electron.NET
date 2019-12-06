using ElectronNET.MSBuild;
using ElectronNET.MSBuild.Actions;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.IO;

namespace ElectronNET.MSBuildTasks
{
    public class PrepareElectronApp : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ProjectDir { get; set; }

        public override bool Execute()
        {
            // Debugger.Launch();
            Log.LogMessage(MessageImportance.High, "Preparing Electron files...");

            string aspCoreProjectPath = ProjectDir;
            Log.LogMessage(MessageImportance.High, $"aspCoreProjectPath {aspCoreProjectPath}");

            string tempPath = Path.Combine(aspCoreProjectPath, "obj", "Host");
            if (Directory.Exists(tempPath) == false)
            {
                Directory.CreateDirectory(tempPath);
            }

            DeployEmbeddedElectronFiles.Do(tempPath);

            var nodeModulesDirPath = Path.Combine(tempPath, "node_modules");
            Log.LogMessage(MessageImportance.High, "node_modules missing in: " + nodeModulesDirPath);
            Log.LogMessage(MessageImportance.High, "Start npm install...");

            ProcessHelper.CmdExecute(Log, "npm install", tempPath);
            Log.LogMessage(MessageImportance.High, "ElectronHostHook handling started...");

            string electronhosthookDir = Path.Combine(aspCoreProjectPath, "ElectronHostHook");

            if (Directory.Exists(electronhosthookDir))
            {
                string hosthookDir = Path.Combine(tempPath, "ElectronHostHook");
                DirectoryCopy.Do(electronhosthookDir, hosthookDir, true, new List<string>() { "node_modules" });

                Log.LogMessage(MessageImportance.High, "Start npm install for typescript & hosthooks...");
                ProcessHelper.CmdExecute(Log, "npm install", hosthookDir);

                // ToDo: Not sure if this runs under linux/macos
                ProcessHelper.CmdExecute(Log, @"npx tsc -p ../../ElectronHostHook", tempPath);
            }

            return true;
        }
    }
}
