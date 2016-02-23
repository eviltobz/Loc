//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Management.Automation;
//using System.Runtime.InteropServices.ComTypes;
//using System.ServiceProcess;
//using Microsoft.PowerShell.Commands;

//namespace LocTools
//{
//    public class SetClientCommand : Command
//    {
//        private readonly IEnumerable<string> clients;

//        public SetClientCommand(CommandSelector context, IEnumerable<string> clients) : base(context)
//        {
//            this.clients = clients;
//        }

//        [Parameter(Position = 1)]
//        public string ClientName { get { return _clientName; } set { _clientName = value.ToUpper(); } }
//        private string _clientName;

//        public override void Process()
//        {
//            if (clients.Contains(ClientName))
//            {
//                Write("Setting active client from \"" + Context.CurrentClient + "\" to \"" + ClientName + "\"");
//                System.Environment.SetEnvironmentVariable("LocToolsActiveClient", ClientName);
//            }
//            else
//            {
//                if (String.IsNullOrWhiteSpace(ClientName))
//                    Write("Current active client is \"" + Context.CurrentClient + "\"");
//                else
//                    Write("\"" + ClientName + "\" is not a valid client", ConsoleColor.Red);

//                WriteAllClients(clients);
//            }
//        }
//    }

//    public class ClientDetailsCommand : Command
//    {
//        private readonly IEnumerable<ServiceController> _services;

//        public ClientDetailsCommand(CommandSelector context, IEnumerable<ServiceController> services) : base(context)
//        {
//            _services = services;
//        }

//        public override void Process()
//        {
//            Write("Client details for " + Context.CurrentClient);
//            foreach (var service in _services)
//            {
//                WriteService(service);
//            }
//            Write("...");
//        }

//    }

//    public class ListAllCommand : Command
//    {
//        private readonly IEnumerable<string> clients;

//        public ListAllCommand(CommandSelector context, IEnumerable<string> clients) : base(context)
//        {
//            this.clients = clients;
//        }

//        public override void Process()
//        {
//            WriteAllClients(clients);
//        }
//    }

//    public class StopAllCommand : Command
//    {
//        private readonly IEnumerable<ServiceController> _services;

//        public StopAllCommand(CommandSelector context, IEnumerable<ServiceController> services) : base(context)
//        {
//            _services = services;
//        }

//        public override void Process()
//        {
//            foreach (var service in _services)
//            {
//                if (service.Status == ServiceControllerStatus.Running)
//                {
//                    Write("Stopping " + service.ServiceName, ConsoleColor.DarkRed);
//                    service.Stop();
//                }
//                else
//                    WriteService(service);
//            }
//        }

//    }

//    public class Todo_command : Command
//    {
//        public Todo_command(CommandSelector context) : base(context)
//        {
//        }

//        public override void Process()
//        {
//            Write("To do - implement " + Context.CommandName, ConsoleColor.Red);
//        }
//    }
//}