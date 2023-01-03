using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AlloyaChecks
{
    public enum CheckType
    {
        Inhouse,
        Outside
    }

    public class ReceiptData
    {// All properties need to be strings since they'll be used for input in Alloya/Selenium 
        public Logger log = Logger.Instance;

        public string AccountNum;
        public string SubAccountNum1;
        public string SubAccountNum2;
        public string TotalAmount = "0.00";
        public string NumberOfChecks = "0";

        public string TotalAmountInhouse = "0.00";
        public string NumberOfChecksInhouse = "0";

        public ReceiptData() { }
    }

    public class ReceiptParser
    { // Reads and parses a txt 
        public Logger log = Logger.Instance;

        private string RawReceipt;

        private int AccountStartIndex1 = 356;
        private int AccountStartIndex2 = 520; // If there's a second acct, the substr after this will be
                                              // the acct num; if there ISN'T, the substr will be "Cash In: "  
        private int CheckStartIndex1 = 589;
        private int CheckStartIndex2 = 753;
        private int CheckLineLength = 41;

        private List<string> ChecksList;
        private List<string> ChecksListInhouse;

        public bool isCheckReceipt = false;

        public ReceiptData RData; // = new ReceiptData(); 
        public ReceiptParser()
        {
            RData = new ReceiptData();
            ChecksList = new List<string>();
            ChecksListInhouse = new List<string>();
        }

        public void Read(string receipt_file_path)
        {// Takes in receipt txt file and saves receipt content string 
            try
            {
                List<string> lines = File.ReadLines(receipt_file_path).Select(line => line.TrimStart(' ')).ToList();
                RawReceipt = String.Join("", lines);
                Debug.WriteLine("Trimmed line -->" + RawReceipt + "<--");

                if (RawReceipt.Contains("CHECK"))
                {
                    isCheckReceipt = true;
                }
            }
            catch (Exception ex)
            {
                log.WriteErrorLog("Couldn't read in receipt file: \n" + ex.Message + "\n");
            }
        }

        public void Parse()
        {// Parses the receipt string and outputs a ReceiptData object 
            string account = GetAccountString();
            Debug.WriteLine("ACCOUNT NUM: " + account);

            ParseChecks();

            int check_start_index = RawReceipt.IndexOf("CHECK");

            int numChecks = GetNumberOfChecks(CheckType.Outside);
            //Debug.WriteLine("Num checks: >>>" + numChecks.ToString() + "<<<");

            string totalAmount = GetTotalAmount(CheckType.Outside);
            //Debug.WriteLine("Total amt: >>>" + totalAmount.ToString() + "<<<");

            int numChecksInhouse = GetNumberOfChecks(CheckType.Inhouse);
            //Debug.WriteLine("Num checks: >>>" + numChecksInhouse.ToString() + "<<<");

            string totalAmountInhouse = GetTotalAmount(CheckType.Inhouse);
        }

        public void ParseChecks()
        {
            int beginIndex = RawReceipt.IndexOf("Checks Received") + "Checks Received".Length;

            //string raw_checks = RawReceipt.Substring(check_start_index);
            string raw_checks = RawReceipt.Substring(beginIndex);
            Debug.WriteLine(">>>" + raw_checks + "<<<" + "\n");

            // There will be a "         Signature Required" remainder at end of checks string
            int remainder = raw_checks.Length % CheckLineLength;
            int checksStringLength = raw_checks.Length - remainder;

            for (int i = 0; i < checksStringLength; i += CheckLineLength)
            {
                string check = raw_checks.Substring(i, CheckLineLength);
                Debug.WriteLine(">>>" + check + "<<<");
                if (check.Contains("MEMBER"))
                {// checks that start with this are inhouse checks    
                    ChecksListInhouse.Add(check);
                }
                else
                {
                    ChecksList.Add(check);
                }
            }
        }

        public bool CheckForSecondAccount()
        {// Checks if RawReceipt has a second account num or if checks start at position accountStartIndex2
            // Return false for now; TODO : implement later  
            bool secondAccount = false;
            string firstAcct = RawReceipt.Substring(CheckStartIndex1, 5);
            Debug.WriteLine("CHECKING FOR SECOND ACCOUNT -- Check string: " + firstAcct);
            if (!firstAcct.Contains("CHECK"))
            {
                Debug.WriteLine("RECEIPT HAS SECOND ACCOUNT \n");
                secondAccount = true;
            }
            return secondAccount;
        }

        public string GetAccountString()
        {
            // Example string: "... 1 Account: 164004Account Number               Beg. Balance ...";
            // Need to extract acct num 
            int beginIndex = RawReceipt.IndexOf("Account: ") + "Account: ".Length;
            int endIndex = RawReceipt.IndexOf("Account Number");
            string account_num = RawReceipt.Substring(beginIndex, endIndex - beginIndex);  // Trust me bro  
            RData.AccountNum = account_num;

            return account_num;
        }

        public int GetNumberOfChecks(CheckType check_type)
        {
            int checks;
            if (check_type == CheckType.Outside)
            {
                checks = ChecksList.Count();
                // Adding value to our receipt data object 
                RData.NumberOfChecks = checks.ToString();
            }
            else
            {
                checks = ChecksListInhouse.Count();
                // Adding value to our receipt data object 
                RData.NumberOfChecksInhouse = checks.ToString();
            }
            //Debug.WriteLine("Outside check num total: "+RData.NumberOfChecks);
            //Debug.WriteLine("Inhouse check num total: " + RData.NumberOfChecksInhouse);
            return checks;
        }

        public string GetTotalAmountHelper(List<string> checks_list)
        {
            decimal total_amount = 0.00m;
            Debug.WriteLine("Checks list length: " + checks_list.Count());
            for (int i = 0; i < checks_list.Count(); i++)
            {
                Debug.WriteLine(">>> Checks List Item # " + i + ":");
                Debug.WriteLine(checks_list[i]);
                var split_line = Regex.Split(checks_list[i], @"\s{2,}");      // ["CHECK #", "0.01"]; check amount in second   
                if (split_line.Length == 2 && split_line.ToString().ToUpper().Contains("CHECK"))
                {
                    Debug.WriteLine("Getting check amount...");
                    decimal check_amount = decimal.Parse(split_line[1], System.Globalization.CultureInfo.InvariantCulture);
                    total_amount += check_amount;

                    Debug.WriteLine("Total amount: " + total_amount.ToString());
                }
                else
                {// TODO : delete this part 
                    Debug.WriteLine("NEVER PARSED CHECK AMOUNT");
                }
            }

            return total_amount.ToString();
        }

        public string GetTotalAmount(CheckType check_type)
        {
            string total_amount;
            if (check_type == CheckType.Outside)
            {
                total_amount = GetTotalAmountHelper(ChecksList);
                RData.TotalAmount = total_amount.ToString();
            }
            else
            {
                total_amount = GetTotalAmountHelper(ChecksListInhouse);
                RData.TotalAmountInhouse = total_amount.ToString();
            }
            return total_amount;
        }
    }
}

