using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AlloyaChecks
{
    public partial class AlloyaChecksService : ServiceBase
    {
        public Logger log = Logger.Instance;
        public Utility Util = new Utility(); 
        public AlloyaChecksService()
        {
            InitializeComponent(); 
        }

        protected override void OnStart(string[] args)
        {
            // This is where main functionality happens (aka "RunProgram() method in old)
            log.WriteInfoLog("Started Alloya Checks service");
            Util.InitializeWindowsRegistry(); 
        }

        protected override void OnStop()
        {
            log.WriteInfoLog("Stopped Alloya Checks service");
        }

        
    }
}
