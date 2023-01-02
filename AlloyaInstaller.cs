using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace AlloyaChecks
{
    [RunInstaller(true)]
    public partial class AlloyaServiceInstaller : System.Configuration.Install.Installer
    {
        public AlloyaServiceInstaller()
        {
            InitializeComponent();
        }

        private void serviceAlloyaInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceAlloyaProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
