//using System;
//using System.Collections.Generic;
//using System.ServiceProcess;

//namespace LocTools
//{
//    public abstract class Command
//    {
//        protected Command(CommandSelector context)
//        {
//            Context = context;
//        }

//        protected CommandSelector Context { get; private set; }

//        private void DoWrite(string message, ConsoleColor colour)
//        {
//            var old = Console.ForegroundColor;
//            Console.ForegroundColor = colour;
//            Console.WriteLine(message);
//            Console.ForegroundColor = old;
//        }
//        protected void Write(string message, ConsoleColor? colour = null)
//        {
//            if (colour == null)
//                colour = Console.ForegroundColor;

//            DoWrite(message, colour.Value);
//        }

//        protected void WriteService(ServiceController service)
//        {
//            ConsoleColor? lineColour = null;
//            if (service.Status == ServiceControllerStatus.Running)
//                lineColour = ConsoleColor.Green;
//            if (service.Status == ServiceControllerStatus.Stopped)
//                lineColour = ConsoleColor.DarkGray;

//            Write(service.Status + " - " + service.ServiceName, lineColour);
//        }

//        protected void WriteAllClients(IEnumerable<string> clients)
//        {
//            Write("Listing all clients installed");
//            foreach (var client in clients)
//            {
//                Write("  " + client);
//            }
//        }

//        public abstract void Process();
//    }



    //[Cmdlet(VerbsOther.Use, "LocOLD")]
    //public class CommandSelector : Cmdlet, IDynamicParameters
    //{
    //    [Parameter(Position = 0, Mandatory=false)]
    //    public string CommandName { get; set; }

    //    private Command selectedCommand;
    //    public string CurrentClient { get { return Environment.GetEnvironmentVariable("LocToolsActiveClient"); }}

    //    protected override void ProcessRecord()
    //    {
    //        selectedCommand.Process();
    //    }

    //    private void DebugOut(string message)
    //    {
    //        //WriteObject(message);
    //        Console.WriteLine(message);
    //    }


    //    private IEnumerable<string> GetAllClients()
    //    {
    //        ServiceController[] a = System.ServiceProcess.ServiceController.GetServices();
    //        var b = a.Where(s=>s.ServiceName.IndexOf('$') > 0 && s.ServiceName.IndexOf("-LOC", StringComparison.InvariantCultureIgnoreCase) > s.ServiceName.IndexOf('$'));
    //        var clients = b.Select(s =>
    //        {
    //            var name = s.ServiceName;
    //            int instanceStart = name.IndexOf('$');
    //            int end = name.IndexOf("-LOC", instanceStart, StringComparison.InvariantCultureIgnoreCase);
    //            int start = end;
    //            while (name[start - 1] != '.' && name[start - 1] != '$')
    //                start--;
    //            var retval = name.Substring(start, end - start);
    //            return retval.ToUpper();

    //        }).Distinct();
    //        return clients;
    //    }

    //    private IEnumerable<ServiceController> GetCurrentClientServices()
    //    {
    //        var a = System.ServiceProcess.ServiceController.GetServices();
    //        return a.Where(s=>s.ServiceName.Contains(CurrentClient));
    //    }
    //    private IEnumerable<ServiceController> GetAll15bServices()
    //    {
    //        var a = System.ServiceProcess.ServiceController.GetServices();
    //        var b = a.Where(s=>s.ServiceName.IndexOf('$') > 0 && s.ServiceName.IndexOf("-LOC", StringComparison.InvariantCultureIgnoreCase) > s.ServiceName.IndexOf('$'));
    //        return b;
    //    }

    //    private class CommandInstance
    //    {
    //        public readonly string Name;
    //        public readonly bool RequiresActiveClient;
    //        public readonly Func<Command> Instantiator;

    //        public CommandInstance(string name, bool requiresActiveClient, Func<Command> instantiator)
    //        {
    //            Name = name;
    //            RequiresActiveClient = requiresActiveClient;
    //            Instantiator = instantiator;
    //        }
    //    }

    //    public object GetDynamicParameters()
    //    {
    //        DebugOut("in get dyn params");

    //        if (string.IsNullOrEmpty(CommandName))
    //            CommandName = "";

    //        var selector = CommandName.ToLower();
    //        DebugOut("selector = " + selector);

    //        var commands = new CommandInstance[]
    //        {
    //            new CommandInstance("Client", false, () => new SetClientCommand(this, GetAllClients())),
    //            new CommandInstance("ListAll", false, () => new ListAllCommand(this, GetAllClients())),
    //            new CommandInstance("StopAll", false, () => new StopAllCommand(this, GetAll15bServices())),

    //            new CommandInstance("Details", true, () => new ClientDetailsCommand(this, GetCurrentClientServices())),
    //        };

    //        var selected = commands.FirstOrDefault(c => c.Name.ToLower() == selector);
    //        if (selected != null)
    //            if (string.IsNullOrWhiteSpace(CurrentClient) && selected.RequiresActiveClient)
    //            {
    //                Console.WriteLine("Can't run command " + CommandName + " without an active client.");
    //                selectedCommand = new ListAllCommand(this, GetAllClients());
    //            }
    //            else
    //                selectedCommand = selected.Instantiator.Invoke();
    //        else
    //            selectedCommand = new HelpCommand(this, commands.Select(c=>c.Name));

    //        return selectedCommand;
    //    }

    //}



//}