using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;

namespace LocTools
{
    public class ServiceCommands
    {
        private readonly Writer output;
        private readonly Services service;
        private readonly Configuration config;

        public ServiceCommands(Writer output, Services service, Configuration config)
        {
            this.output = output;
            this.service = service;
            this.config = config;
        }


        public void Help(Hax hax)
        {
            output.Line("Todo... helpy output", ConsoleColor.Yellow);
        }

        public void ListAll()
        {
            output.WriteAllClients(service.GetAllClients(), config.Client);
        }

        public void StopAll()
        {
            var services = service.GetAll15bServices();
            StopServices(services);
        }

        private void StopServices(IEnumerable<ServiceController> services)
        {
            foreach (var s in services)
            {
                if (s.Status == ServiceControllerStatus.Running)
                {
                    output.Line("Stopping " + s.ServiceName, ConsoleColor.DarkRed);
                    s.Stop();
                }
                else
                    output.Service(s);
            }
        }

        public void ClientDetails()
        {
            output.Line("Client details for " + config.Client);
            output.Indent();
            foreach (var s in service.GetClientServices(config.Client))
            {
                output.Service(s);
            }
            output.Outdent();
        }

        public void SetClient(string clientParam)
        {
            var clients = service.GetAllClients();

            if (string.IsNullOrWhiteSpace(clientParam))
            {
                output.WriteAllClients(clients, config.Client);
                return;
            }

            var clientName = clientParam.ToUpper();

            if (clients.Contains(clientName))
            {
                output.Line("Setting active client from \"" + config.Client + "\" to \"" + clientName + "\"");
                config.Client = clientName;
            }
            else
            {
                output.Line("\"" + clientName + "\" is not a valid client", ConsoleColor.Red);
                output.WriteAllClients(clients, config.Client);
            }
        }

        // specify an index for gimps & renderers? eg. LXA has bus gimps on AW5-8, no good way to start that.
        // have some persisted config for different clients to remember default service options?
        public void Start(bool misc, bool basicRenderer, bool allRenderers, bool basicGimp, bool allGimps)
        {
            output.Line("Start selected services for " + config.Client);
            output.Line(misc + "," + allRenderers + "," + basicRenderer + "," + allGimps + "," + basicGimp);
            var services = this.service.GetClientServices(config.Client);
            foreach (var s in services)
            {
                var shouldStart = ShouldStartService(misc, basicRenderer, allRenderers, basicGimp, allGimps, s);

                if (shouldStart)
                    StartService(s);
                else
                    output.Line("Skipped " + s.DisplayName, ConsoleColor.DarkGray);
            }
        }

        private static bool ShouldStartService(bool misc, bool basicRenderer, bool allRenderers, bool basicGimp, bool allGimps, ServiceController s)
        {
            var name = s.ServiceName.ToLower();
            if (name.Contains("renderingmanager"))
            {
                var isValidRenderer = (basicRenderer && (name.EndsWith("-rm1") || name.EndsWith("-rb1")));
                if (allRenderers || isValidRenderer)
                    return true;
                else
                    return false;
            }
            else if (name.Contains("gdsinteractionhost"))
            {
                var lastSegment = name.Substring(name.LastIndexOf('-'));
                var isValidGimp = (basicGimp && (lastSegment.Contains("1")));
                if (allGimps || isValidGimp)
                    return true;
                else
                    return false;
            }
            return misc;
        }

        private void StartService(ServiceController s)
        {
            // Try asyncifying startups so it can get some parallelism and whatnot
            // then think of a good way to show that in the ui ;)
            // is there a good commandline way to change lines that you've displayed?...
            // http://stackoverflow.com/questions/12538675/how-to-update-command-line-output-in-place-from-c-sharp
            // http://stackoverflow.com/questions/3407548/c-sharp-console-set-cursor-position-to-the-last-visible-line
            if (s.Status == ServiceControllerStatus.Stopped)
            {
                output.Line("Starting " + s.DisplayName, ConsoleColor.Green);
                s.Start();
            }
            else
            {
                output.Line(s.ServiceName + " - " + s.Status, ConsoleColor.Gray);
            }
        }

        // Add more one-off managementy stuff, like set all services for the client to manual instead of automatic start

        // log purge - list a service - have it get the log path (find in service's app.config? or guess by convention?) stop service, wipe log, start service

        // deploy - config some octopushability like my lxatemplate mini project ?? or just leave as additional clientspecific scripts?

        // can we do the servicey things & whatnot remotely? so the app could actually live on my dev box & be configged to point at the LOC environment?
        // that would enhance the combo things of build/nuget package, mini-deploy, resend notification to a single task, rather than some here, some there :)

        public void Stop()
        {
            StopServices(service.GetClientServices(config.Client));
        }
    }
}