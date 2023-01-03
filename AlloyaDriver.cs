using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Diagnostics;
using System.Timers;

namespace AlloyaChecks
{
    public class AlloyaDriver
    {
        //private static Properties.Settings props = Properties.Settings.Default;

        public Logger log = Logger.Instance;
        public Utility util = new Utility();


        // TODO : rewrite this whole thing
        /* public string driverURL = "https://premierview.alloyacorp.org/";
         public string edgeDriverPath = props.EdgeDriverPath;
         private string _USER = props.Username;    //"imotyashok";
         private string _PASS; // = props.Password;    // "znc#dja23"; 
         private string _q1 = props.Q1;
         private string _q2 = props.Q2;
         private string _q3 = props.Q3;
         private string _q1_answer = props.Q1Answer;
         private string _q2_answer = props.Q2Answer;
         private string _q3_answer = props.Q3Answer;
         private string _location = props.Location;
         private string _location_index = props.LocationIndex;*/

        public string driverURL = "https://premierview.alloyacorp.org/";
        public string edgeDriverPath;
        private string _USER;
        private string _PASS;
        private string _q1;
        private string _q2;
        private string _q3;
        private string _q1_answer;
        private string _q2_answer;
        private string _q3_answer;
        private string _location;
        private string _location_index; 

        public Timer timer;
        //    private DateTime StopTime;
        private int TimeLeft = 60;
        public bool TimerRunning = false;
        public bool TokenValidation = false;

        Dictionary<string, string> securityQuestions;
        EdgeDriver driver;
        public bool DriverInitialized { get; set; }

        private static DateTime StartTime { get; set; }

        public AlloyaDriver() // Constructor; initialize driver here   
        {
            // TODO : initialize the settings from registry here 
            edgeDriverPath = util.getRegistryKeyValue(UserSettings.EdgeDriverPath.ToString());
            _USER = util.getRegistryKeyValue(UserSettings.Username.ToString());
            _PASS = util.getRegistryKeyValue(UserSettings.Password.ToString());
            _q1 = util.getRegistryKeyValue(UserSettings.Q1.ToString());
            _q2 = util.getRegistryKeyValue(UserSettings.Q2.ToString());
            _q3 = util.getRegistryKeyValue(UserSettings.Q3.ToString());
            _q1_answer = util.getRegistryKeyValue(UserSettings.Q1Answer.ToString());
            _q2_answer = util.getRegistryKeyValue(UserSettings.Q2Answer.ToString());
            _q3_answer = util.getRegistryKeyValue(UserSettings.Q3Answer.ToString());
            _location = util.getRegistryKeyValue(UserSettings.Location.ToString());
            _location_index = util.getRegistryKeyValue(UserSettings.LocationIndex.ToString()); 
        }

        public void InitializeDriver()
        {
            // Set timestamp of when we began the driver 
            StartTime = DateTime.Now;

            // edgeDriverPath = edgeDriverPath;
            try
            {
                driver = new EdgeDriver(edgeDriverPath);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                driver.Url = driverURL;
                DriverInitialized = true;
            }
            catch (Exception e)
            {
                DriverInitialized = false;
                var errorMessage = "Couldn't initialize web driver: \n" + e.Message + "\n";
                log.WriteErrorLog(errorMessage);
            }

            try
            {
                securityQuestions = new Dictionary<string, string>
                {   //// Static
                    //{ "What is your pet's name?", "snozzgobbler" },
                    //{ "In what city were you born?", "ternopil" },
                    //{ "In what city did you start high school?", "chicago" }

                    // Dynamic
                    {_q1, _q1_answer},
                    {_q2, _q2_answer},
                    {_q3, _q3_answer}
                };
            }
            catch (Exception e)
            {
                log.WriteErrorLog("Couldn't initialize security questions: \n" + e.Message + "\n");
            }
        }

        private bool IsElementPresent(By by)
        {// Helper function to check if selenium element exists 
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private void TokenMainPageHelper()
        {// Helper function for getting people who have tokens to the main Scan screen; starts from Discover Premier View page 
            try
            {
               // OLD: for some reason FindElementByXPath() doesn't work
               // driver.FindElementByXPath("/html/body/div[1]/form/div[3]/div[1]/div/div/div/ul/li/a").Click();
               // driver.FindElementByXPath("/html/body/div[1]/form/div[3]/div[2]/div[1]/div/div[3]/div/div[1]/input").Click();
                driver.FindElement(By.XPath("/html/body/div[1]/form/div[3]/div[1]/div/div/div/ul/li/a")).Click();
                driver.FindElement(By.XPath("/html/body/div[1]/form/div[3]/div[2]/div[1]/div/div[3]/div/div[1]/input")).Click();

                //// Clicking on Scan button 
                //driver.FindElementByXPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li[1]/ul/li[3]/a").Click();

            }
            catch (Exception e)
            {
                log.WriteErrorLog("Couldn't get to Scan Checks page: " + e.Message + "\n");
            }
        }

        public bool CheckTimer()
        {
            Debug.WriteLine("Timer running value: " + TimerRunning.ToString());
            return TimerRunning;
        }

        public void TokenTimeout(object sender, EventArgs e)
        {// Check every 5 seconds up to 2 mins if they submitted their token by checking if they're on the next page 
         // if (DateTime.Now >= StopTime)
            Debug.WriteLine(">> Timer Tick: " + TimeLeft.ToString());
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                //   TimerRunning = false; 
                Debug.WriteLine("Two minutes up -- stop timer and exit");
                timer.Stop();
                timer.Dispose();
                log.WriteErrorLog("Timer ran out for token ");
                Environment.Exit(0);
            }
            else
            {// Check if they're on next page 
                Debug.WriteLine("Checking if user input token..." + DateTime.Now.ToString("MM/dd-yyyy hh:mm:ss tt"));
                // They can be on the main page immediately, apparently... 
                if (IsElementPresent(By.Id("react_0HMEMEINI4NH0")))
                {// They're on the main page (skipped security questions); proceed to get them to scan screen 
                    Debug.WriteLine("On main page");
                    timer.Stop();
                    timer.Dispose();
                    //    TimerRunning = false; 

                    // Navigate to Scan checks page; receipt info will populate from there
                    TokenMainPageHelper();
                }
                else if (IsElementPresent(By.Id("Question1")))
                {// They're on the security questions page; enter in answers 
                    Debug.WriteLine("On security questions page");
                    timer.Stop();
                    timer.Dispose();
                    TimerRunning = false;

                    // Proceed to answer security questions --------------------------------------------------------------------------
                    try
                    {
                        // first question path 
                        string q1 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[3]/label")).Text;
                        IWebElement q1_box = driver.FindElement(By.Id("Answer1"));
                        q1_box.SendKeys(securityQuestions[q1]);

                        // second question
                        string q2 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[4]/label")).Text;
                        IWebElement q2_box = driver.FindElement(By.Id("Answer2"));
                        q2_box.SendKeys(securityQuestions[q2]);

                        // third question 
                        string q3 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[5]/label")).Text;
                        IWebElement q3_box = driver.FindElement(By.Id("Answer3"));
                        q3_box.SendKeys(securityQuestions[q3]);

                        // Submit 
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[7]/div/input")).Click();

                        // Navigate to Scan checks page; receipt info will populate from there
                        TokenMainPageHelper();

                    }
                    catch (Exception ex)
                    {
                        log.WriteErrorLog("Couldn't answer security questions: \n" + ex.Message + "\n");
                    }
                }
            }
        }

        public void LogIn()
        {
            try
            {
                IWebElement username = driver.FindElement(By.Id("Username"));
                username.SendKeys(_USER);
                IWebElement password = driver.FindElement(By.Id("Password"));
                password.SendKeys(_PASS);
                driver.FindElement(By.XPath("/html/body/div/div/div[2]/div[4]/div[2]/form/div[6]/input")).Click();
            }
            catch (Exception e)
            {
                log.WriteErrorLog("Couldn't log in to Alloya: \n" + e.Message + "\n");
            }

            // CHECKING FOR TOKEN -----------------------------------------------------------------------
            if (IsElementPresent(By.Id("TwoFactorCode")))
            {// Token identification required -- wait for 1 min for them to put in the code 
                Debug.WriteLine("Token needed! Starting timer...");

                TimerRunning = true;

                TokenValidation = true;
                //    StopTime = DateTime.Now.AddMinutes(2);
                //     TimeLeft = 60; 

                //    timer = new Timer();
                //    timer.Tick += TokenTimeout;
                //    timer.Interval = 1000; // Update every 1 second 
                //    timer.Start();
            }
            else
            {
                Debug.WriteLine("No token; proceed normally");
                // CHECKING FOR SECURITY QUESTIONS ----------------------------------------------------------
                try
                {
                    if (IsElementPresent(By.XPath("/html/body/div/div/div/div/div[1]/form/h1")))
                    {// Do the security questions;  TODO : refactor later? 
                        // first question path 
                        string q1 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[3]/label")).Text;
                        IWebElement q1_box = driver.FindElement(By.Id("Answer1"));
                        q1_box.SendKeys(securityQuestions[q1]);

                        // second question
                        string q2 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[4]/label")).Text;
                        IWebElement q2_box = driver.FindElement(By.Id("Answer2"));
                        q2_box.SendKeys(securityQuestions[q2]);

                        // third question 
                        string q3 = driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[5]/label")).Text;
                        IWebElement q3_box = driver.FindElement(By.Id("Answer3"));
                        q3_box.SendKeys(securityQuestions[q3]);

                        // Submit 
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/div[1]/form/div[7]/div/input")).Click();
                    }
                }
                catch (Exception e)
                {
                    log.WriteErrorLog("Couldn't answer security questions: \n" + e.Message + "\n");
                }
            }
        }

        public void inputReceiptInfo(ReceiptData r_data)
        {// There will be 3 cases: all outside checks, all inhouse checks, or mix of both
            int alloyaCase = 0;
            if (r_data.NumberOfChecks != "0" && r_data.NumberOfChecksInhouse == "0")
            {// All outside checks
                alloyaCase = 1;
            }
            else if (r_data.NumberOfChecks == "0" && r_data.NumberOfChecksInhouse != "0")
            {// All inhouse checks
                alloyaCase = 2;
            }
            else if (r_data.NumberOfChecksInhouse != "0" && r_data.NumberOfChecks != "0")
            {// Mix of both 
                alloyaCase = 3;
            }

            if (TokenValidation)
            {
                Debug.WriteLine(">>> Clicking on Scan button");
                // Need to switch to new tab first: 
                // var new_tab = driver.WindowHandles[1];
                driver.SwitchTo().Window(driver.WindowHandles.Last());
                // Clicking on Scan button

                //driver.FindElementByXpath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li[1]/ul/li[3]/a").Click();
                // OLD // driver.FindElementByLinkText("Scan").Click();
                driver.FindElement(By.LinkText("Scan")).Click();

            }
            else
            {
                //driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li/ul/li[3]/a")).Click();
                // OLD // driver.FindElementByLinkText("Scan").Click();
                driver.FindElement(By.LinkText("Scan")).Click();
            }

            try
            {
                // Clicking on Scan button
                //   driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li/ul/li[3]/a")).Click();
                driver.FindElement(By.Id("location_dropdown_chosen")).Click();
                driver.FindElement(By.XPath("//*[@data-option-array-index='" + _location_index + "']")).Click();

                // NEW ---------------------------------------------
                IWebElement amount_box = driver.FindElement(By.Id("AddBatchBatchAmount"));
                IWebElement total_count_box = driver.FindElement(By.Id("AddBatchItemCount"));
                IWebElement account = driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[8]/div[1]/input"));

                switch (alloyaCase)
                {
                    case 0:     // A case wasn't set; throw an error 
                        log.WriteErrorLog("Unable to distinguish between types of checks (inhouse or outside \n");
                        break;
                    case 1:     // All outside checks 
                        amount_box.SendKeys(r_data.TotalAmount);
                        total_count_box.SendKeys(r_data.NumberOfChecks);

                        driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[10]/div/div/input[1]")).Click();
                        account.SendKeys(r_data.AccountNum);
                        break;
                    case 2:     // All inhouse checks  
                        amount_box.SendKeys(r_data.TotalAmountInhouse);
                        total_count_box.SendKeys(r_data.NumberOfChecksInhouse);
                        driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[10]/div/div/input[2]")).Click();
                        account.SendKeys(r_data.AccountNum);
                        break;
                    case 3:     // Mixed 
                        // First, input outside checks.. 
                        amount_box.SendKeys(r_data.TotalAmount);
                        total_count_box.SendKeys(r_data.NumberOfChecks);
                        driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[10]/div/div/input[1]")).Click();
                        account.SendKeys(r_data.AccountNum);
                        // Next, input inhouse checks in a new tab 
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                        driver.Navigate().GoToUrl(driverURL);

                        // Have to log in and everything... 
                        LogIn();

                        if (TokenValidation)
                        {// Click Scan button; non-token validation has different path for Scan button  
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li[1]/ul/li[3]/a")).Click();
                        }
                        else
                        {// Click Scan button 
                            driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li/ul/li[3]/a")).Click();
                        }
                        //driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div[2]/div[2]/div/ul[2]/li/ul/li[3]/a")).Click();
                        driver.FindElement(By.Id("location_dropdown_chosen")).Click();
                        driver.FindElement(By.XPath("//*[@data-option-array-index='" + _location_index + "']")).Click();
                        amount_box = driver.FindElement(By.Id("AddBatchBatchAmount"));
                        total_count_box = driver.FindElement(By.Id("AddBatchItemCount"));
                        account = driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[8]/div[1]/input"));
                        amount_box.SendKeys(r_data.TotalAmountInhouse);
                        total_count_box.SendKeys(r_data.NumberOfChecksInhouse);
                        account.SendKeys(r_data.AccountNum);
                        driver.FindElement(By.XPath("/html/body/div[3]/div/div/div[2]/div[3]/div/div[8]/div[2]/div[2]/div[10]/div/div/input[2]")).Click();

                        break;
                }
            }
            catch (Exception e)
            {
                //  Errors.Add("Couldn't input receipt information: \n" + e.Message + "\n");
                log.WriteErrorLog("Couldn't input receipt information: \n" + e.Message + "\n");
            }
        }

        public void KillDriver()
        {// Fully dispose of any and all drivers
            if (DriverInitialized)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception e) { }
                try
                {
                    driver.Dispose();
                }
                catch (Exception e) { }
            }
        }

        /* Security questions
         * 
         * What is your pet's name?
         * What is your maternal grandmother's first name?
         * What is your paternal grandfather's first name?
         * What is your mother's middle name?
         * What is your father's middle name?
         * In what city did you start high school?
         * In what city did you work for the first time?
         * In what city were you born?
         * What is your favorite make of car?
         * What is your favorite sport?
         */

    }
}