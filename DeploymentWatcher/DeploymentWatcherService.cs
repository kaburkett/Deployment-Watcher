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

        public static void NewEventLog(string message)
        {
            //setup event log to track events using eventLog.WriteEntry();
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("DeploymentWatcher"))
            {
                System.Diagnostics.EventLog.CreateEventSource("DeploymentWatcher", "Application");
            }
            eventLog.Source = "DeploymentWatcher";
            eventLog.Log = "Application";
            eventLog.WriteEntry(message);
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
            NewEventLog("Watcher started monitoring path: " + path);
        }

        protected override void OnStop()
        {
            NewEventLog("DeploymentWatcher stopped");
        }
    }
}
