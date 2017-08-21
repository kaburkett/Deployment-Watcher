using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentWatcher
{
    public partial class DeploymentWatcherService : ServiceBase
    {
        public static System.Diagnostics.EventLog eventLog;
        public static string path = "C:\\testDeploymentPath";
        public static string temp_log_path = "C:\\DeploymentWatcher\\temp.log";

        /* NewEventLog makes troubleshooting this service easier.
         * message = comment to write
         * write(true = write to file log, false = write to system events)
         */
        public static void NewEventLog(string message, bool write)
        {
            //write to system event log > application
            if (write == false)
            {
                eventLog = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("DeploymentWatcher"))
                {
                    System.Diagnostics.EventLog.CreateEventSource("DeploymentWatcher", "Application");
                }
                eventLog.Source = "DeploymentWatcher";
                eventLog.Log = "Application";
                eventLog.WriteEntry(message);
            }            

            //append to temporary deployment log that we will show user
            //in order to write a line to the log it has to be directly related to the deployment and not the service
            //thus, the write has to be done after the file is initialized in DeploymentWatcher.FindRunAndDelete
            if (write == true)
            {
                using (var fs = new FileStream(temp_log_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    message = message + System.Environment.NewLine;
                    byte[] messageBytes = UnicodeEncoding.Unicode.GetBytes(message.ToCharArray());                    
                    fs.Write(messageBytes, 0, messageBytes.Length);
                    fs.Close();
                }
            }            
        }

        public DeploymentWatcherService()
        {
            InitializeComponent();           
        }

        protected override void OnStart(string[] args)
        {
            //initialize deployment watcher here
            DeploymentWatcher watcher = new DeploymentWatcher(path);
            watcher.Watch();
            NewEventLog("Watcher started monitoring path: " + path, false);
        }

        protected override void OnStop()
        {
            NewEventLog("DeploymentWatcher stopped", false);
        }
    }
}
