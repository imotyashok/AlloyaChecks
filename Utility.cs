using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlloyaChecks
{
    public class Utility
    {
        public Logger log = Logger.Instance;
        public enum UserSettings
        {
            Username,
            Password,
            Q1,
            Q2,
            Q3,
            Q1Answer,
            Q2Answer,
            Q3Answer,
            Location,
            LocationIndex,
            EdgeDriverPath,
            PrinterServerName,
            GenericPrinterName,
            EpsonPrinterName,
            GenericPrinterPort,
            ReceiptPath
        }

        private static string registrySubkeyPath = "SYSTEM\\CurrentControlSet\\Services\\Alloya Checks Service\\Credentials";
        private RegistryKey AlloyaRegistry = null; 

        public bool IsKeyEmpty(string key)
        {
            if (AlloyaRegistry == null)
            {
                log.WriteErrorLog("Alloya Registry is not initialized"); 
            } 

            if (AlloyaRegistry.GetValue(key) == null || AlloyaRegistry.GetValue(key).ToString() == "{value not set}")
            {
                return false;
            }
            return true; 
        }

        public void InitializeWindowsRegistry()
        {
            RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            AlloyaRegistry = localMachine.OpenSubKey(registrySubkeyPath, true);
            try
            {
                if (AlloyaRegistry == null)
                {
                    AlloyaRegistry = localMachine.CreateSubKey(registrySubkeyPath);
                }

                foreach (var setting in Enum.GetNames(typeof(UserSettings)))
                {
                    if (AlloyaRegistry.GetValue(setting) == null)
                    {
                        AlloyaRegistry.SetValue(setting, "{value not set}");
                    }
                }
            } catch (Exception e)
            {
                log.WriteErrorLog("Couldn't create Alloya registry and write keys to it: " + e.ToString()); 
            }
        }
    }
}
