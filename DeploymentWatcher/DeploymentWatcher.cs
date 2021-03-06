﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentWatcher
{
    public class DeploymentWatcher
    {
        FileSystemWatcher watcher;
        string directoryToWatch;
        // Some of the variables used in this class are initialize in DeploymentWatcherService.cs
        public DeploymentWatcher(string path)
        {
            this.watcher = new FileSystemWatcher();
            this.directoryToWatch = path;
        }
        public void Watch()
        {
            watcher.Path = directoryToWatch;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess |
                            NotifyFilters.LastWrite |
                            NotifyFilters.FileName |
                            NotifyFilters.DirectoryName;
            watcher.Filter = "*.cmd";
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.Error += new ErrorEventHandler(OnError);
            DeploymentWatcherService.NewEventLog("Watching initialized with path: " + directoryToWatch, false);
            
        }
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            FindRunAndDelete(e.FullPath);
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            FindRunAndDelete(e.FullPath);
        }

        private void FindRunAndDelete(string cmdPath)
        {
            //create new log file for each change detected
            FileStream fs = new FileStream(DeploymentWatcherService.temp_log_path, FileMode.Create);
            fs.Close();
            
            DeploymentWatcherService.NewEventLog("Running: cmd.exe /c " + cmdPath +" /Y", true);
            ExecuteCommand(cmdPath);

            //delete all files in directory
            DeploymentWatcherService.NewEventLog("Delete contents: " + directoryToWatch, true);
            try
            {
                DeleteDirectory(DeploymentWatcherService.path);
            }
            catch(Exception e)
            {
                DeploymentWatcherService.NewEventLog("Error removing directory, please remove manually: " +System.Environment.NewLine+ e, true);
            }

            //once directory is cleared out, move over the log file
            File.Move(DeploymentWatcherService.temp_log_path, DeploymentWatcherService.path + "\\DeploymentLog.log"); 
        }

        private static void OnError(object source, ErrorEventArgs e)
        {
            DeploymentWatcherService.NewEventLog("Error Happened! Error: " + e.GetException().ToString(), true);

            //once directory is cleared out, move over the log file
            File.Move(DeploymentWatcherService.temp_log_path, DeploymentWatcherService.path + "\\ErrorLog.log");
        }

        static void ExecuteCommand(string cmdPath)
        {
            var processInfo = new ProcessStartInfo();          
            processInfo.WorkingDirectory = "C:\\Windows\\system32";
            processInfo.FileName = "cmd.exe";
            processInfo.Arguments = @"/c " + cmdPath + " /Y";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);
            string outputData = null;
            string errorData = null;
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                outputData += "output>> " + e.Data +System.Environment.NewLine;    
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                errorData += "error >> " + e.Data +System.Environment.NewLine;
            process.BeginErrorReadLine();            


            if (process.WaitForExit(420000))
            {
                //write cmd output to text file
                DeploymentWatcherService.NewEventLog("============== CONSOLE OUTPUT ============" + System.Environment.NewLine + outputData, true);
                DeploymentWatcherService.NewEventLog("============== CONSOLE ERRORS ============" + System.Environment.NewLine + errorData, true);
                DeploymentWatcherService.NewEventLog("============== DEPLOY RESULTS ============" + System.Environment.NewLine + errorData, true);

                if (process.ExitCode == 0)
                {
                    DeploymentWatcherService.NewEventLog("Deployment Successful", true);
                }
                else
                {
                    DeploymentWatcherService.NewEventLog("Deployment Failed With ExitCode: " + process.ExitCode, true);
                }
            }
            else
            {
                //write cmd output to text file
                DeploymentWatcherService.NewEventLog("============== CONSOLE OUTPUT ============" + System.Environment.NewLine + outputData, true);
                DeploymentWatcherService.NewEventLog("============== CONSOLE ERRORS ============" + System.Environment.NewLine + errorData, true);
                DeploymentWatcherService.NewEventLog("============== DEPLOY RESULTS ============" + System.Environment.NewLine + errorData, true);
                DeploymentWatcherService.NewEventLog("Deployment Timed Out (it took more than 7 minutes, so it was closed with the force).", true);
                DeploymentWatcherService.NewEventLog("                                                                         {٩ಠಠ}" + System.Environment.NewLine, true);
            }

            process.Close();
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer. 
        /// https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

    }
}
