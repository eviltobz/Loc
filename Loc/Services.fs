module Services

open System.ServiceProcess
open UI


type ClientService = {troller:ServiceController; name:string; instance:string; client:string}
type Environment = {client:string; services:seq<ClientService>}
type InstanceSelector =
    | All
    | None
    | Instance of string



let private PrintServiceAt (cs : ClientService) index =
    let service = cs.troller
//    let colour = match service.Status with
//                    | ServiceControllerStatus.Running -> GREEN
//                    | ServiceControllerStatus.Stopped -> DARKGREY
//                    | _ -> System.Console.ForegroundColor
//    cprintfat index colour "%O - %s$%s" service.Status cs.name cs.instance
    let colour = match service.Status with
                    | ServiceControllerStatus.Running -> _GREEN
                    | ServiceControllerStatus.Stopped -> _DARKGREY
                    | _ -> _DEFAULT
    WriteAt index [colour (service.Status.ToString() + " - " + cs.name + "$" + cs.instance)]

let private PrintService (cs : ClientService) =
    let service = cs.troller
//    let colour = match service.Status with
//                    | ServiceControllerStatus.Running -> GREEN
//                    | ServiceControllerStatus.Stopped -> DARKGREY
//                    | _ -> System.Console.ForegroundColor
//    cprintf colour "%O - %s" service.Status cs.name
//    printfn "$%s" cs.instance
    let colour = match service.Status with
                    | ServiceControllerStatus.Running -> _GREEN
                    | ServiceControllerStatus.Stopped -> _DARKGREY
                    | _ -> _DEFAULT
//    cprintf colour "%O - %s" service.Status cs.name
//    printfn "$%s" cs.instance
    WriteLine [colour(service.Status.ToString() +  " - " + cs.name); _DEFAULT("$" + cs.instance)]

let private PrintEnvironment (env : Environment) =
    printfn "%d services" (Seq.length env.services)
    env.services 
        |> Seq.iter PrintService

    
let private getLocIpAddress() =
    let computerName = Config.get.LocAddress
    let hostEntry = System.Net.Dns.GetHostEntry computerName
    let ip = hostEntry.AddressList.[0]
    ip.ToString()
    
let private getAllServices() = 
    ServiceController.GetServices(getLocIpAddress())

let private extractServiceDetails (service : ServiceController) =
    let name = service.ServiceName
    let instanceStart = name.IndexOf('$')
    let endIndex = name.IndexOf("-LOC", instanceStart + 1, System.StringComparison.InvariantCultureIgnoreCase)
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
    getAllEnvironments() |> Seq.find (fun x-> x.client = Config.get.CurrentClient)

//let private stopService (s:ClientService) =
//    let t =s.troller
//    match t.Status with
//    | ServiceControllerStatus.Running -> 
//        cprintfn RED "Stopping %s" t.DisplayName
//        t.Stop()
//    | _ -> 
//        PrintService s

let private stopServiceA (s:ClientService) index =
    async {
        do! Async.SwitchToThreadPool()
        let t =s.troller
        match t.Status with
        | ServiceControllerStatus.Running -> 
//            cprintfat index RED "Stopping %s" t.DisplayName
            WriteAt index [_RED ("Stopping " + t.DisplayName)]
            t.Stop()
        | _ -> 
            PrintServiceAt s index
        ()
    }


//let private startService (s:ClientService) =
//    let t =s.troller
//    match t.Status with
//    | ServiceControllerStatus.Stopped -> 
//        cprintfn GREEN "Starting %s" t.ServiceName
//        t.Start()
//        cprintflast GREEN "Started - %s" t.ServiceName
//    | _ -> 
//        printfn "%s - Running" t.ServiceName


let private startServiceA (s:ClientService) index =
    async {
        let context = System.Threading.SynchronizationContext()
        let t =s.troller
        match t.Status with
        | ServiceControllerStatus.Stopped -> 
//            cprintfat index RED "Starting %s" t.ServiceName
//            do! Async.SwitchToNewThread()
            do! Async.SwitchToThreadPool()
//            cprintfat index RED "Starting %s (%d)" t.ServiceName System.Threading.Thread.CurrentThread.ManagedThreadId
            WriteAt index [_RED ("Starting " + t.ServiceName)]
            t.Start()
//            do! Async.SwitchToContext context
//            cprintfat index GREEN "Started - %s (%d)" t.ServiceName System.Threading.Thread.CurrentThread.ManagedThreadId
            WriteAt index [_RED ("Started - " + t.ServiceName)]
        | _ -> 
//            cprintfat index DEFAULT "%s - Running" t.ServiceName
                ()
    }
                        
    
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
//    cprintfn System.ConsoleColor.DarkYellow  "Installed LOC clients:"
    WriteLine [_DARKYELLOW "Installed LOC clients:"]
    getAllClients() |> Seq.iter System.Console.WriteLine

let PrintAllRunningServices() =
//    cprintfn DARKYELLOW "Listing all running services for all clients:"
    WriteLine [_DARKYELLOW "Listing all running services for all clients:"]
    getAllEnvironments() 
    |> Seq.iter (fun env ->
        let running = env.services |> Seq.where (fun s->s.troller.Status = ServiceControllerStatus.Running)
        printfn "%s %d/%d services running" env.client (Seq.length running) (Seq.length env.services)
        running |> Seq.iter PrintService
        )

let StopAllClients() =
//    cprintfn DARKYELLOW "Stopping all 15below services"
    WriteLine [_DARKYELLOW "Stopping all 15below services"]
//    let environments = getAllEnvironments()
//    environments |> Seq.iter (fun e -> e.services |> Seq.iter (fun s-> stopService s)) 

    let startIndex = UI.NextLineIndex() 
    let initialState = 
        getAllEnvironments()
        |> Seq.map (fun e -> 
            e.services 
            |> Seq.map (fun s ->
//                cprintfn DARKRED "Queueing %s" s.troller.ServiceName
                WriteLine [_DARKRED ("Queueing " + s.troller.ServiceName)]
                s))
        |> Seq.collect (fun x -> x)
    let job = async {
        let! starts = 
            initialState
            |> Seq.mapi (fun i s -> stopServiceA s (i + startIndex))
            |> Async.Parallel
        ()
        } 
    job |> Async.RunSynchronously 
    
let DetailCurrentClient() =
//    cprintfn DARKYELLOW "Currently selected client is %s" Config.get.CurrentClient
    WriteLine [_DARKYELLOW ("Currently selected client is " + Config.get.CurrentClient)]
    let client = getCurrentClientEnvironment()
    PrintEnvironment client

//let StopCurrentClientSERIAL() =
//    cprintfn DARKYELLOW "Stopping all services for %s" Config.get.CurrentClient
//    let client = getCurrentClientEnvironment()
//    client.services |> Seq.iter (fun s-> stopService s)

let StopCurrentClient() =
//    cprintfn DARKYELLOW "Stopping all services for %s" Config.get.CurrentClient
    WriteLine [_DARKYELLOW ("Stopping all services for " + Config.get.CurrentClient)]
//    let client = getCurrentClientEnvironment()
//    client.services |> Seq.iter (fun s-> stopService s)

    let startIndex = UI.NextLineIndex() 
    let initialState = 
        getCurrentClientEnvironment().services
        |> Seq.map (fun s -> 
//            cprintfn DARKRED "Queueing %s" s.troller.ServiceName
            WriteLine [_DARKRED ("Queueing " + s.troller.ServiceName)]
            s)
    let job = async {
        let! starts = 
            initialState
            |> Seq.mapi (fun i s -> stopServiceA s (i + startIndex))
            |> Async.Parallel
        ()
        } 
    job |> Async.RunSynchronously



//let StartServicesSERIAL (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) =
//    let shouldStartService = isServiceSelected misc gimps renderers
//
//    let stoppy = System.Diagnostics.Stopwatch()
//    stoppy.Start()
//
//    cprintfn DARKYELLOW "Starting %s services:" Config.get.CurrentClient
//    printfn "%O%O %O" (if misc then "Misc " else "") (instanceStatus "Gimps" gimps) (instanceStatus "Renderers" renderers)
//    let startIndex = UI.NextLineIndex() 
//    getCurrentClientEnvironment().services 
//        |> Seq.map (fun s -> (shouldStartService s, s)) 
//        |> Seq.iter (fun (shouldStart, cs) -> 
//            match shouldStart with
//            | true -> startService cs
//            | false -> cprintfn DARKGREY "Skipped - %s" cs.troller.ServiceName)
//
//
//    stoppy.Stop()
//    cprintfn DARKYELLOW "Sequential Duration: %dms" stoppy.ElapsedMilliseconds
//
//    ()

let StartServices (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) =
    let shouldStartService = isServiceSelected misc gimps renderers

    let min = System.Threading.ThreadPool.SetMinThreads(5, 5)
    let max = System.Threading.ThreadPool.SetMaxThreads(8, 8)

    let stoppy = System.Diagnostics.Stopwatch()
    stoppy.Start()

//    cprintfn DARKYELLOW "Starting %s services:" Config.get.CurrentClient
    WriteLine [_DARKYELLOW ("Starting " + Config.get.CurrentClient + " services:")]
    printfn "%O%O %O" (if misc then "Misc " else "") (instanceStatus "Gimps" gimps) (instanceStatus "Renderers" renderers)
    let startIndex = UI.NextLineIndex() 
    let client = getCurrentClientEnvironment()
    let currentState = 
        client.services 
        |> Seq.map (fun s -> (shouldStartService s, s)) 
        |> Seq.map (fun x -> 
            let shouldStart, cs = x
            match shouldStart with
            | true ->
                match cs.troller.Status with
                | ServiceControllerStatus.Stopped -> WriteLine [_DARKRED ("Queueing " + cs.troller.ServiceName)]
                | _ -> WriteLine [_DEFAULT (cs.troller.ServiceName + " - Running" )]
            | false -> WriteLine [_DARKGREY ("Skipped - " + cs.troller.ServiceName)]
            x)

    let job = async {
        let! starts = 
            currentState
            |> Seq.mapi (fun i (shouldStart, cs) -> 
                match shouldStart with
                | true -> startServiceA cs (i + startIndex)
                | false -> async { () } )
            |> Async.Parallel
        ()
        } 
    job |> Async.RunSynchronously

    stoppy.Stop()
    WriteLine [_DARKYELLOW ("Parallelly Duration: " + stoppy.ElapsedMilliseconds.ToString() + "ms.")]

//    System.Threading.ThreadPool.GetMaxThreads(ref maxWork, ref maxComp)
//    System.Threading.ThreadPool.GetMinThreads(ref minWork, ref minComp)
//    System.Threading.ThreadPool.GetAvailableThreads(ref aWork, ref aComp)
//    printfn "POOL - Workers:%d-%d/%d Completion:%d-%d/%d" aWork minWork maxWork aComp minComp maxComp

    ()




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


