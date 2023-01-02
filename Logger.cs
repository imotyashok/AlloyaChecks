using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlloyaChecks
{
    public sealed class Logger
    {
        private String eventLogSource = "Alloya Checks Service";
        private String eventLogName = "Alloya Checks Service Log";

        private static Logger instance = null;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                    instance = new Logger();
                return instance;
            }
        }
        private Logger()
        {
            if (!EventLog.SourceExists(eventLogSource))
            {
                EventLog.CreateEventSource(eventLogSource, eventLogName);
                return;
            }
        }
        public void WriteInfoLog(string message)
        {
            EventLog.WriteEntry(eventLogSource, message, EventLogEntryType.Information);
        }

        public void WriteErrorLog(string message)
        {
            EventLog.WriteEntry(eventLogSource, message, EventLogEntryType.Error);
        }
    }
}
