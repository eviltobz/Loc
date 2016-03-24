module Services

open System.ServiceProcess
open UI


type ClientService = {troller:ServiceController; name:string; instance:string; client:string}
type Environment = {client:string; services:seq<ClientService>}
type InstanceSelector =
    | All
    | None
    | Instance of string


let private ignoreCase = System.StringComparison.InvariantCultureIgnoreCase

let private PrintService (cs : ClientService) =
    let service = cs.troller
    let colour = match service.Status with
                    | ServiceControllerStatus.Running -> GREEN
                    | ServiceControllerStatus.Stopped -> DARKGREY
                    | _ -> System.Console.ForegroundColor
//    cprintfn colour "%s %O >> %s" service.ServiceName service.Status cs.client
    cprintf colour "%O - %s" service.Status cs.name
    printfn "$%s >> %s" cs.instance cs.client

let private PrintEnvironment (env : Environment) =
    printfn "%d services for %s" (Seq.length env.services) env.client
    env.services 
        |> Seq.iter PrintService

//let private PrintServices (services : seq<ClientService>) =
let private PrintEnvironments (envs : seq<Environment>) =
//    services |> Seq.iter PrintService
//    services 
//        |> Seq.groupBy (fun c->c.client) 
//        |> Seq.iter (fun (client, services)-> 
//            printfn "Services for %s" client
//            services |> Seq.iter PrintService)
    envs |> Seq.iter PrintEnvironment
//        printfn "%d services for %s" (Seq.length e.services) e.client
//        e.services 
//            |> Seq.iter PrintService)


    
let private getLocIpAddress() =
    let computerName = Config.get.LocAddress //"loc-tc-01"
    let hostEntry = System.Net.Dns.GetHostEntry computerName
    let ip = hostEntry.AddressList.[0]
    ip.ToString()
    
let private getAllServices() = 
    ServiceController.GetServices(getLocIpAddress())

let private extractServiceDetails (service : ServiceController) =
    let name = service.ServiceName
    let instanceStart = name.IndexOf('$')
    let endIndex = name.IndexOf("-LOC", instanceStart + 1, ignoreCase)
    match endIndex with
    | -1 -> (null, null, null)
    | x ->
        let startIndex = name.ToCharArray() |> Seq.take endIndex |> Seq.findIndexBack (fun x -> x = '.' || x = '$') |> fun x -> x + 1
        (name.Substring(0, startIndex-1), name.Substring(startIndex), name.Substring(startIndex, endIndex - startIndex).ToUpper())

let private getAllEnvironments() = 
    getAllServices() 
        |> Seq.map (fun x -> 
            let (service, instance, client) = extractServiceDetails(x) 
            {troller=x; name=service; instance=instance; client=client})
        |> Seq.where (fun x -> not(System.String.IsNullOrWhiteSpace(x.client)) )
        |> Seq.sortBy (fun x -> x.client)

        |> Seq.groupBy (fun x->x.client)
        |> Seq.map (fun (client, services)-> {client=client; services = services})




let private getCurrentClientEnvironment() =
//    getAllEnvironments() |> Seq.where (fun x -> System.String.Equals(x.client, Config.get.CurrentClient, ignoreCase))
    getAllEnvironments() |> Seq.find (fun x-> x.client = Config.get.CurrentClient)

let private stopService (s:ClientService) =
    let t =s.troller
    match t.Status with
    | ServiceControllerStatus.Running -> 
        cprintfn RED "Stopping %s" t.DisplayName
        t.Stop()
    | _ -> 
        PrintService s

let private startService (s:ClientService) =
    let t =s.troller
    match t.Status with
    | ServiceControllerStatus.Stopped -> 
        cprintfn GREEN "Starting %s" t.ServiceName
        t.Start()
        cprintflast GREEN "Started - %s" t.ServiceName
    | _ -> 
        printfn "%s - Running" t.ServiceName
                    
    
let instanceStatus title selector =
    match selector with
    | All -> title + ":All "
    | None -> ""
    | Instance x -> title + ":" + x

let isInstanceSelected (service:ClientService) selector = 
    match selector with
    | All -> true
    | None -> false
    | Instance x -> service.instance.ToLower().Contains(x.ToLower())

let isServiceSelected (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) (service:ClientService) =
    let servicename = service.name.ToLower()
    match servicename with
    | renderer when renderer.Contains("renderingmanager") -> isInstanceSelected service renderers
    | gimp when gimp.Contains("gdsinteractionhost") -> isInstanceSelected service gimps
    | other -> misc

    



// PUBLIC API BITS

let getAllClients() =
    getAllEnvironments() |> Seq.map (fun x -> x.client) |> Seq.distinct |> Seq.sort

// do we want to _just_ have a print???? want client selection to have an interacty mode.
let PrintAllClients() =  
    cprintfn System.ConsoleColor.DarkYellow  "Installed LOC clients:"
    getAllClients() |> Seq.iter System.Console.WriteLine

let PrintAllRunningServices() =
    cprintfn DARKYELLOW "Running services for all clients:"
    getAllEnvironments() |> Seq.map (fun e -> {client=e.client; services = e.services |> Seq.where (fun s->s.troller.Status = ServiceControllerStatus.Running)}) |> PrintEnvironments 

let StopAllClients() =
    cprintfn DARKYELLOW "Stopping all 15below services"
    let environments = getAllEnvironments()
    environments |> Seq.iter (fun e -> e.services |> Seq.iter (fun s-> stopService s)) 
    

let StopCurrentClient() =
    cprintfn DARKYELLOW "Stopping all services for %s" Config.get.CurrentClient
    let services = getCurrentClientEnvironment()
    services.services |> Seq.iter (fun s-> stopService s)


let StartServices (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) =
    let shouldStartService = isServiceSelected misc gimps renderers

    cprintfn DARKYELLOW "Starting %s services:" Config.get.CurrentClient
    printfn "%O%O %O" (if misc then "Misc " else "") (instanceStatus "Gimps" gimps) (instanceStatus "Renderers" renderers)
    getCurrentClientEnvironment().services |> Seq.map (fun s -> (shouldStartService s, s)) |> Seq.iter (fun (shouldStart, cs) -> 
        match shouldStart with
        | true -> startService cs
        | false -> cprintfn DARKGREY "Skipped - %s" cs.troller.ServiceName)

    printfn "yeah, whatever"



let Haxx =
//    printfn "********All Clients"
//    getAllClients() |> Seq.iter System.Console.WriteLine
//
    printfn "********Current Client Services"
    getCurrentClientEnvironment() |> PrintEnvironment
//
//    printfn "********Stop all running envs"
//    StopAllClients()

//    StartServices false (Instance "aw4")  (Instance "rM1")
//    StartServices true None None

//    StartServices false (Instance "aw4")  (Instance "rM1")
    StopCurrentClient()
//    StartServices true None None
//    StartServices false (Instance "aw4")  (Instance "rM1")
//    StopAllClients()
//    PrintAllRunningServices()

    printfn "********Current Client Services"
    getCurrentClientEnvironment() |> PrintEnvironment



//    cprintfn RED "a line"
//    printfn "another line"
//    let num = pHack DARKYELLOW "ugh, %s" "format strings :("
//    printfn "and something else after..."
//    System.Threading.Thread.Sleep(1000)
//    cprintfat num GREEN "w00tage"
//
//    cprintflast DARKYELLOW "but was this the last line? %d" System.Console.CursorTop
//    System.Threading.Thread.Sleep(1000)


