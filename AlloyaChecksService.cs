using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace AlloyaChecks
{
    public partial class AlloyaChecksService : ServiceBase
    {
        public Logger log = Logger.Instance;
        public Utility Util = new Utility();

        private AlloyaDriver alloyaDriver;

        public AlloyaChecksService()
        {
            InitializeComponent(); 
        }

        protected override void OnStart(string[] args)
        {
            // This is where main functionality happens (aka "RunProgram() method in old)
            log.WriteInfoLog("Started Alloya Checks service");
            RunProgram(); 
        }

        protected override void OnStop()
        {
            if (alloyaDriver != null)
            {
                alloyaDriver.KillDriver();
            }
            log.WriteInfoLog("Stopped Alloya Checks service");
        }

        private void RunProgram()
        {
            // Initialize the Alloya Checks Service > Credentials registry in Registry Editor 
            Util.InitializeWindowsRegistry();

            if (Util.allSettingsExist())
            {
                try
                {
                    var receiptDir = new DirectoryInfo(Util.getRegistryKeyValue(UserSettings.ReceiptPath.ToString())); //  Props.ReceiptPath); ;
                    var receipt_file = receiptDir.GetFiles("*.txt").OrderByDescending(f => f.LastWriteTime).First();

                    ReceiptParser parser = new ReceiptParser();
                    parser.Read(receipt_file.FullName);
                    // TODO : CHECK IF THE BELOW WORKS 
                    if (parser.isCheckReceipt)
                    {
                        parser.Parse();

                        ReceiptData r_data = parser.RData;

                        //string message_str =
                        //    "Filename: >>>"+receipt_file.Name+"<<< \n"+
                        //    "Account: >>> " + r_data.AccountNum + " <<< \n" +
                        //    "Num checks: >>>" + r_data.NumberOfChecks + "<<< \n" +
                        //    "Total amt: >>>" + r_data.TotalAmount + "<<< \n" +
                        //    "Num checks inhouse: >>>" + r_data.NumberOfChecksInhouse + "<<< \n" +
                        //    "Total amt inhouse: >>>" + r_data.TotalAmountInhouse + "<<< \n";

                        //// TODO : get rid of this for production 
                        //MessageBox.Show("Receipt info: \n" + message_str);

                        alloyaDriver.InitializeDriver();
                        alloyaDriver.LogIn();
                        if (alloyaDriver.TokenValidation)
                        {
                            alloyaDriver.SetTimer(); 
                        }
                        else
                        {
                            alloyaDriver.inputReceiptInfo(r_data);
                        }

                        if (alloyaDriver.HasError)
                        {
                            log.WriteErrorLog("Couldn't run Alloya");
                        }
                    }
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("Couldn't run program: \n" + e.ToString());
                }
            }
            else
            {
                log.WriteErrorLog("There are settings missing; please add values to the keys in the registry"); 
            }
        }
    }
}
