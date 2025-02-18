﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ElectronNET.MSBuild
{
    public class ProcessHelper
    {
        private readonly static Regex ErrorRegex = new Regex(@"\berror\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static int CmdExecute(TaskLoggingHelper log, string command, string workingDirectoryPath, bool output = true, bool waitForExit = true)
        {
            using (Process cmd = new Process())
            {
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

                int returnCode = 0;

                if (output)
                {
                    cmd.OutputDataReceived += (s, e) =>
                    {
                        // (sometimes error messages are only visbile here)
                        // poor mans solution, we just seek for the term 'error'

                        // we can't just use cmd.ExitCode, because
                        // we delegate it to cmd.exe, which runs fine
                        // but we can catch any error here and return
                        // 1 if something fails
                        if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
                        {
                            if (ErrorRegex.IsMatch(e.Data))
                            {
                                returnCode = 1;
                            }

                            log.LogMessage(MessageImportance.Normal, e.Data);
                        }

                    };
                    cmd.ErrorDataReceived += (s, e) =>
                    {
                        // poor mans solution, we just seek for the term 'error'

                        // we can't just use cmd.ExitCode, because
                        // we delegate it to cmd.exe, which runs fine
                        // but we can catch any error here and return
                        // 1 if something fails
                        if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
                        {
                            if (ErrorRegex.IsMatch(e.Data))
                            {
                                returnCode = 1;
                            }

                            log.LogMessage(MessageImportance.Normal, e.Data);
                        }

                    };
                }

                cmd.Start();
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();

                cmd.StandardInput.WriteLine(command);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();

                if (waitForExit)
                {
                    cmd.WaitForExit();
                }

                return returnCode;
            }
        }
    }
}
