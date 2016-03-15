module Services

open System.ServiceProcess

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

let cprintf c fmt = 
    Printf.kprintf
        (fun s ->
            let old = System.Console.ForegroundColor
            try
              System.Console.ForegroundColor <- c;
              System.Console.Write s
            finally
              System.Console.ForegroundColor <- old)
        fmt

let ignoreCase = System.StringComparison.InvariantCultureIgnoreCase

let PrintService (service : ServiceController) =
    let colour = match service.Status with
                    | ServiceControllerStatus.Running -> System.ConsoleColor.DarkGreen
                    | ServiceControllerStatus.Stopped -> System.ConsoleColor.DarkRed
                    | _ -> System.Console.ForegroundColor
    cprintf colour "%s %O" service.ServiceName service.Status

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
    let services = ServiceController.GetServices()
    PrintServices services