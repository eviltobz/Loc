module Services

open System.ServiceProcess
open UI

//        public IReadOnlyCollection<string> GetAllClients()
//        {
//            ServiceController[] a = System.ServiceProcess.ServiceController.GetServices();
//            var b = a.Where(s => s.ServiceName.IndexOf('$') > 0 && s.ServiceName.IndexOf("-LOC", StringComparison.InvariantCultureIgnoreCase) > s.ServiceName.IndexOf('$'));
//            var clients = b.Select(s =>
//            {
//                var name = s.ServiceName;
//                int instanceStart = name.IndexOf('$');
//                int end = name.IndexOf("-LOC", instanceStart, StringComparison.InvariantCultureIgnoreCase);
//                int start = end;
//                while (name[start - 1] != '.' && name[start - 1] != '$')
//                    start--;
//                var retval = name.Substring(start, end - start);
//                return retval.ToUpper();
//
//            }).Distinct();
//            return clients.ToArray();
//        }

let ignoreCase = System.StringComparison.InvariantCultureIgnoreCase

let PrintService (service : ServiceController) =
    let colour = match service.Status with
                    | ServiceControllerStatus.Running -> System.ConsoleColor.DarkGreen
                    | ServiceControllerStatus.Stopped -> System.ConsoleColor.DarkRed
                    | _ -> System.Console.ForegroundColor
    cprintfn colour "%s %O" service.ServiceName service.Status

let PrintServices (services : ServiceController[]) =
    services |> Seq.iter PrintService


let TrimServiceName (service : ServiceController) = 
    let fullName = service.ServiceName
    let instanceStart = fullName.IndexOf('$')
    let instanceEnd = fullName.IndexOf("-LOC", instanceStart, ignoreCase)

    let mutable start = instanceEnd

    while fullName.Chars(start - 1) <> '.' && fullName.Chars(start - 1) <> '$' do
        start <- start - 1


    let retval = fullName.Substring(start, instanceEnd - start).ToUpper()
    printfn "%s >> %s" fullName retval 

    retval


let GetAllClients =
    let services = ServiceController.GetServices()
    let filtered = services |> Seq.where (fun s -> 
        (s.ServiceName.IndexOf('$') > 0) 
        && (s.ServiceName.IndexOf("-LOC", ignoreCase) > s.ServiceName.IndexOf('$')) )
    let clients = filtered |> Seq.map TrimServiceName

    //"todo..."
    services |> Seq.map (fun x -> x.ServiceName) |> Seq.distinct

let Haxx =
    let computerName = "loc-tc-01.15below.local"
    let computerName = "cascade.15below.local"

//    let options = System.Management.ConnectionOptions()
//    options.Username <- "\\Administrator"
//    options.Password <- "Candide!"
//    options.Impersonation <- System.Management.ImpersonationLevel.Impersonate
//
//    let scope = System.Management.ManagementScope("\\\\" + computerName + "\\root\\cimv2", options)
//    scope.Connect()
//
//    let troller = new ServiceController()
//    troller.MachineName <- computerName

    // if this fails to connect check:
    // * local user is set as admin on the box
    // * firewall isn't blocking remote management
    let services = ServiceController.GetServices(computerName)
    PrintServices services