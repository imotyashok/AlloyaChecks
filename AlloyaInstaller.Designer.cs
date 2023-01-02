
namespace AlloyaChecks
{
    partial class AlloyaServiceInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceAlloyaProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceAlloyaInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceAlloyaProcessInstaller
            // 
            this.serviceAlloyaProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceAlloyaProcessInstaller.Password = null;
            this.serviceAlloyaProcessInstaller.Username = null;
            this.serviceAlloyaProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceAlloyaInstaller_AfterInstall);
            // 
            // serviceAlloyaInstaller
            // 
            this.serviceAlloyaInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceAlloyaProcessInstaller});
            this.serviceAlloyaInstaller.ServiceName = "Alloya Checks Service";
            this.serviceAlloyaInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceAlloyaProcessInstaller_AfterInstall);
            // 
            // AlloyaServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceAlloyaProcessInstaller,
            this.serviceAlloyaInstaller});

        }

        #endregion

        public System.ServiceProcess.ServiceProcessInstaller serviceAlloyaProcessInstaller;
        public System.ServiceProcess.ServiceInstaller serviceAlloyaInstaller;
    }
}