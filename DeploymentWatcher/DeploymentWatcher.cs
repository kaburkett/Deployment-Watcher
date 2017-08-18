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
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.Disposed += Watcher_Disposed;
            watcher.Error += new ErrorEventHandler(OnError);
            DeploymentWatcherService.NewEventLog("Watching initialized with path: " + directoryToWatch);
            
        }

        private void Watcher_Disposed(object sender, EventArgs e)
        {
            DeploymentWatcher watcher = new DeploymentWatcher(directoryToWatch);
            watcher.Watch();
        }
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            FindRunAndDelete(e.FullPath);
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            FindRunAndDelete(e.FullPath);
        }

        private void FindRunAndDelete(string fullpath)
        {
            string batchPath = fullpath;
            DeploymentWatcherService.NewEventLog("Change detected! Command about to run: " + batchPath);
            ExecuteCommand(batchPath);
            DeploymentWatcherService.NewEventLog("Delete contents: " + directoryToWatch);
            System.IO.DirectoryInfo di = new DirectoryInfo(directoryToWatch);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void OnError(object source, ErrorEventArgs e)
        {
            DeploymentWatcherService.NewEventLog("Error Happened! Error: " + e);
        }

        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                DeploymentWatcherService.NewEventLog("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                DeploymentWatcherService.NewEventLog("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            DeploymentWatcherService.NewEventLog("ExitCode: {0}"+ process.ExitCode);
            process.Close();
        }

    }
}
