﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bloatynosy
{
    public static class WindowsHelper
    {
        private static readonly ErrorHelper logger = ErrorHelper.Instance;

        // Command Prompt (to be replaced with wt.exe)
        public static void RunCmd(string args)
        {
            ProcStart(HelperTool.Utils.Paths.ShellCommandPrompt, args);
        }

        public static void RunWT(string args)
        {
            ProcStart(HelperTool.Utils.Paths.ShellWT, args);
        }

        public static void ProcStart(string name, string args)
        {
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = name,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                string line = null;
                while (!proc.StandardOutput.EndOfStream)
                {
                    line += Environment.NewLine + proc.StandardOutput.ReadLine();
                }
                proc.WaitForExit();
                logger.Log($"{name} {args} {line}");
            }
            catch
            {
                logger.Log($"Could not start {name} {args}.");
            }
        }

        public static void IsServiceRunning(string service)
        {
            logger.Log($"\tCheck if {service} service is running");
            RunCmd($"/c sc query {service} | find \"RUNNING\"");
        }

        public static void DisableService(string service)
        {
            RunCmd($"/c net stop {service}");
            ProcStart(HelperTool.Utils.Paths.ShellPS, $"-command \"Set-Service -Name {service} -StartupType disabled\"");
            logger.Log($"Disable {service} service");
        }

        public static void EnableService(string service)
        {
            RunCmd($"/c net start {service}");
            ProcStart(HelperTool.Utils.Paths.ShellPS, $"-command \"Set-Service -Name {service} -StartupType auto\"");
            logger.Log($"Enable {service} service");
        }

        public static async Task RunPowerShellScript(string scriptFilePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    Arguments = $"-executionpolicy bypass -file \"{scriptFilePath}\"",
                    CreateNoWindow = false, // Show the console window
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await Task.Run(() => process.WaitForExit());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running PowerShell plugin: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}