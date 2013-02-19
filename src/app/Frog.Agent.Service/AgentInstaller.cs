using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace Frog.Agent.Service
{
    [RunInstaller(true)]
    public partial class AgentInstaller : System.Configuration.Install.Installer
    {
        private readonly ServiceInstaller serviceInstaller;

        public AgentInstaller()
        {
            InitializeComponent();
            var processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
        private String ServiceName
        {
            get
            {
                var serviceName = Context.Parameters["ServiceName"];

                if (serviceName == null)
                {
                    Console.WriteLine("Usage: InstallUtil /ServiceName=[Service Name] Runz.Agent.Service.exe");
                    throw new InstallException("ServiceName parameter missing");
                }

                return serviceName;
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            serviceInstaller.ServiceName = ServiceName;

            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            serviceInstaller.ServiceName = ServiceName;

            base.Uninstall(savedState);
        }
    }
}
