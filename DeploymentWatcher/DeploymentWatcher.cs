using System;
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

        private void FindRunAndDelete(string batchPath)
        {
            //create new log file for each change detected
            FileStream fs = new FileStream(DeploymentWatcherService.temp_log_path, FileMode.Create);
            fs.Close();
            
            DeploymentWatcherService.NewEventLog("Running: cmd.exe /c " + batchPath +" /Y", true);
            ExecuteCommand(batchPath);

            //delete all files in directory
            DeploymentWatcherService.NewEventLog("Delete contents: " + directoryToWatch, true);
            DeleteDirectory(DeploymentWatcherService.path);
            Directory.CreateDirectory(DeploymentWatcherService.path);

            //once directory is cleared out, move over the log file
            File.Move(DeploymentWatcherService.temp_log_path, DeploymentWatcherService.path + "\\DeploymentLog.log"); 
        }

        private static void OnError(object source, ErrorEventArgs e)
        {
            DeploymentWatcherService.NewEventLog("Error Happened! Error: " + e, true);
        }

        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command + "/Y");
            processInfo.WorkingDirectory = "C:\\Windows\\system32";
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

            process.WaitForExit();

            //write cmd output to text file
            DeploymentWatcherService.NewEventLog("============== CONSOLE OUTPUT ============"+System.Environment.NewLine+outputData, true);
            DeploymentWatcherService.NewEventLog("============== CONSOLE ERRORS ============"+System.Environment.NewLine+errorData, true);
            DeploymentWatcherService.NewEventLog("ExitCode: "+ process.ExitCode, true);
            process.Close();
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer. 
        /// (I took this from SO for complete recursive delete)
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

    }
}
