module Services

open CurrentConfig
open System.ServiceProcess
open UI
open System


type ClientService = {troller:ServiceController; name:string; instance:string; client:string}
type Environment = {client:string; services:seq<ClientService>}
type InstanceSelector =
    | All
    | None
    | Instance of string

let serviceColours (service:ServiceController) =
    match service.Status with
    | ServiceControllerStatus.Running -> (GREEN, DARKGREEN)
    | ServiceControllerStatus.Stopped -> (DARKGREY, DEFAULT)
    | _ -> (DEFAULT, DEFAULT)

let private FormatServiceName (cs : ClientService) : obj list =
    let service = cs.troller
    let main, instance = serviceColours service
    match cs.instance with
    | null -> [main; service.Status; " - "; cs.name;]
    | _ -> [main; service.Status; " - "; cs.name; instance; "$"; cs.instance;]

let private PrintServiceAt (cs : ClientService) index =
    //let service = cs.troller
    //let main, instance = serviceColours service
    //WriteAt index [main; service.Status; " - "; cs.name; instance; "$"; cs.instance]
    WriteAt index (FormatServiceName cs)

let private PrintService (cs : ClientService) =
    //let service = cs.troller
    //let main, instance = serviceColours service
    //WriteLine [main; service.Status; " - "; cs.name; instance; "$"; cs.instance;]
    WriteLine (FormatServiceName cs)

let private PrintEnvironment (env : Environment) =
    printfn "%d services" (Seq.length env.services)
    env.services
        |> Seq.iter PrintService


let private getLocIpAddress() =
    let computerName = Config.LocAddress
    let hostEntry = System.Net.Dns.GetHostEntry computerName
    let ip = hostEntry.AddressList.[0]
    ip.ToString()

let private getAllServices() =
    try
        ServiceController.GetServices(getLocIpAddress())
    with
        | ex -> failwith ( "getAllServices threw: " + ex.Message + "\nPlease ensure that this domain account is a member of the Administrators group on the target machine" )

let public extractServiceDetails (name : string) =
    //let name = service.ServiceName
    //if name.Contains("-LOC") then
    //    Console.Write(name)
    let instanceStart = name.IndexOf('$')
    let envIndex = name.IndexOf("-LOC", System.StringComparison.InvariantCultureIgnoreCase)
    let getClient () =
        let clientIndex = name.ToCharArray() |> Seq.take envIndex |> Seq.findIndexBack (fun x -> x = '-' || x = '$' || x = '.') |> (+) 1
        name.Substring(clientIndex, envIndex - clientIndex)
    match (envIndex, instanceStart) with
    | (-1, _) -> (null, null, null)
    | (_, -1) -> (name, null, getClient())
    | _ -> (name.Substring(0, instanceStart), name.Substring(instanceStart + 1), getClient())


let private getAllEnvironments() =
    getAllServices()
        |> Seq.map (fun x ->
            let (service, instance, client) = extractServiceDetails(x.ServiceName)
            //if x.ServiceName.Contains("-LOC") then
            //    WriteLine [ YELLOW; " = "; (if service = null then "NULL" else service); ", "; (if instance = null then "NULL" else instance); ", "; (if client = null then "NULL" else client)]
            {troller=x; name=service; instance=instance; client=client})
        |> Seq.where (fun x -> not(System.String.IsNullOrWhiteSpace(x.client)) )
        |> Seq.sortBy (fun x -> (x.client, x.name, x.instance))

        |> Seq.groupBy (fun x->x.client)
        |> Seq.map (fun (client, services)-> {client=client; services = services})




let private getCurrentClientEnvironment() =
    getAllEnvironments() |> Seq.find (fun x-> x.client = Config.CurrentClient)

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
//        do! Async.SwitchToThreadPool()
        let t =s.troller
        match t.Status with
        | ServiceControllerStatus.Running ->
//            cprintfat index RED "Stopping %s" t.DisplayName
            WriteAt index [RED; "Stopping "; t.DisplayName]
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

let private GetThreadInfo() =
    let mutable aW: int ref = ref 0
    let mutable aC: int ref = ref 0
    let mutable nW: int ref = ref 0
    let mutable nC: int ref = ref 0
    let mutable xW: int ref = ref 0
    let mutable xC: int ref = ref 0

    System.Threading.ThreadPool.GetAvailableThreads(aW, aC)
    System.Threading.ThreadPool.GetMinThreads( nW, nC)
    System.Threading.ThreadPool.GetMaxThreads( xW,  xC)

    "A:" + aW.contents.ToString() +
    " Min:" + nW.contents.ToString() +
    " Max:" + xW.contents.ToString()


let private startServiceA (s:ClientService) index =
    async {
//        let origA = GetThreadInfo()
        let t = s.troller
        match t.Status with
        | ServiceControllerStatus.Stopped ->
//            let origB = GetThreadInfo()
//            let tid = System.Threading.Thread.CurrentThread.ManagedThreadId
//            WriteAt index [RED; "Starting "; t.ServiceName;] // GREEN; GetThreadInfo(); " - "; RED; origA; " - "; origB; " tid:"; tid; " --- S!"]
            try
                t.Start()
                WriteAt index [RED; "Starting "; t.ServiceName;] // GREEN; GetThreadInfo(); " - "; RED; origA; " - "; origB; " tid:"; tid; GREEN; " --- R!"]

                t.Refresh()
                let mutable count = 0
                while count <= 20 && t.Status <> ServiceControllerStatus.Running && t.Status <> ServiceControllerStatus.Stopped do
                    System.Threading.Thread.Sleep(500)
                    t.Refresh()
                    WriteAt index [RED; t.Status; " "; t.ServiceName; DARKYELLOW; " "; String.replicate (1+(count%5)) "."; ]
                    count <- count+1
            with
            | ex -> WriteAt index [RED; "Exception starting "; t.ServiceName; DARKRED; " "; ex.Message]

            match t.Status with
            | ServiceControllerStatus.Running -> WriteAt index [GREEN; "Started - "; t.ServiceName;]
            | _ -> WriteAt index [DARKRED; "Couldn't start "; t.ServiceName; " in time. "; DARKYELLOW; "Current status is "; t.Status]
        | ServiceControllerStatus.Running -> WriteAt index [DARKGREEN; t.Status; " "; t.ServiceName]
        | _ -> WriteAt index [RED; "Can't start "; t.ServiceName; DEFAULT; " Current status is "; t.Status]
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

let isServiceSelected (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) (arbitrary:InstanceSelector) (service:ClientService) =
    let servicename = service.name.ToLower()
    match arbitrary with
    | Instance name -> (servicename + "$" + (service.instance.ToLower())).Contains(name)
    | _ ->
        match servicename with
        | name when name.Contains("renderingmanager") -> isInstanceSelected service renderers
        | name when name.Contains("gdsinteractionhost") -> isInstanceSelected service gimps
        | other -> misc



let SetThreadPool size =
//    WriteLine [GREEN; "Starting Threadpool gubbins: "; GetThreadInfo()]
    System.Threading.ThreadPool.SetMinThreads(size,size) |> ignore
    System.Threading.ThreadPool.SetMaxThreads(size,size) |> ignore
//    WriteLine [GREEN; "Altered Threadpool gubbins: "; GetThreadInfo()]

let SetDefaultThreadPool() = SetThreadPool 4

// PUBLIC API BITS

let getAllClients() =
    getAllEnvironments() |> Seq.map (fun x -> x.client) |> Seq.distinct |> Seq.sort

// do we want to _just_ have a print???? want client selection to have an interacty mode. **** OOOOOooooohhhh - computation expressions for interaction loops???
let PrintAllClients() =
    WriteLine [DARKYELLOW; "Installed LOC clients:"]
    getAllClients() |> Seq.iter System.Console.WriteLine

let PrintAllRunningServices() =
//    cprintfn DARKYELLOW "Listing all running services for all clients:"
    WriteLine [DARKYELLOW; "Listing all running services for all clients:"]
    getAllEnvironments()
    |> Seq.iter (fun env ->
        let running = env.services |> Seq.where (fun s->s.troller.Status = ServiceControllerStatus.Running)
        printfn "%s %d/%d services running" env.client (Seq.length running) (Seq.length env.services)
        running |> Seq.iter PrintService
        )

let StopAllClients() =
//    cprintfn DARKYELLOW "Stopping all 15below services"
    WriteLine [DARKYELLOW; "Stopping all 15below services"]
    SetDefaultThreadPool()

//    let environments = getAllEnvironments()
//    environments |> Seq.iter (fun e -> e.services |> Seq.iter (fun s-> stopService s))

    let startIndex = UI.NextLineIndex()
    let initialState =
        getAllEnvironments()
        |> Seq.map (fun e ->
            e.services
            |> Seq.map (fun s ->
//                cprintfn DARKRED "Queueing %s" s.troller.ServiceName
                WriteLine [DARKRED; "Queueing "; s.troller.ServiceName]
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

let StatusCurrentClient() =
//    cprintfn DARKYELLOW "Currently selected client is %s" Config.get.CurrentClient
    WriteLine [DARKYELLOW; "Currently selected client is "; Config.CurrentClient]
    let client = getCurrentClientEnvironment()
    PrintEnvironment client

//let StopCurrentClientSERIAL() =
//    cprintfn DARKYELLOW "Stopping all services for %s" Config.get.CurrentClient
//    let client = getCurrentClientEnvironment()
//    client.services |> Seq.iter (fun s-> stopService s)

let stopServices services =
    SetDefaultThreadPool()
    let startIndex = UI.NextLineIndex()
    let initialState =
        services
        |> Seq.map (fun s ->
            WriteLine [DARKRED; "Queueing "; s.troller.ServiceName]
            s)
    let job = async {
        let! starts =
            initialState
            |> Seq.mapi (fun i s -> stopServiceA s (i + startIndex))
            |> Async.Parallel
        ()
        }
    job |> Async.RunSynchronously


let StopCurrentClient() =
    WriteLine [DARKYELLOW; "Stopping all services for "; Config.CurrentClient]
    stopServices (getCurrentClientEnvironment().services)

let StopInstance (instance:string) =
    WriteLine [DARKYELLOW; "Stopping instances of \""; instance; "\" for "; Config.CurrentClient]

    let instanceName = instance.ToUpperInvariant()
    let instances =
        getCurrentClientEnvironment().services
        |> Seq.where (fun i -> i.instance.ToUpperInvariant().Contains instanceName || i.name.ToUpperInvariant().Contains instanceName)
    stopServices instances

let private THREADHAXX offset index =
    async {
        let origA = GetThreadInfo()
        let origT = System.Threading.Thread.CurrentThread.ManagedThreadId
//        let context = System.Threading.SynchronizationContext()
        let origB = GetThreadInfo()

//        do! Async.SwitchToThreadPool()
        let origC = GetThreadInfo()
        let tid = System.Threading.Thread.CurrentThread.ManagedThreadId

        let mutable count = 0
        while count < 20 do
            count <- count+1
            WriteAt (index + offset - 1) [RED; "Index:"; index; GREEN; " "; count; DARKYELLOW; " - Starting Thread:"; origT; " "; origA; " - "; origB; GREEN; " tid:"; tid ; " "; origC ]
//            do! Async.Sleep(500)
            System.Threading.Thread.Sleep(500)

//        do! Async.SwitchToContext context
    }

let StartServices (misc:bool) (gimps:InstanceSelector) (renderers:InstanceSelector) (arbitrary:InstanceSelector) =
    SetDefaultThreadPool()
    let shouldStartService = isServiceSelected misc gimps renderers arbitrary


    (*
    let indices = [1 .. 10]
    WriteLine  [DARKYELLOW; "Before first Async: "; GetThreadInfo()]

    let job = async {
        WriteLine  [DARKYELLOW; "In first Async: "; GetThreadInfo()]
        let startIndex = UI.NextLineIndex()
        indices
        |> Seq.iter (fun i -> WriteLine [GREEN; "*"; i])
        let! starts =
            indices
            |> Seq.mapi (fun i -> THREADHAXX startIndex)
            |> Async.Parallel
        ()
        }
    job |> Async.RunSynchronously
    *)


    WriteLine [DARKYELLOW; "Starting "; Config.CurrentClient; " services:"]
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
                | ServiceControllerStatus.Stopped -> WriteLine [DARKRED; "Queueing "; cs.troller.ServiceName]
                | _ -> WriteLine [DEFAULT; cs.troller.ServiceName; " - Running"]
            | false ->
                let colour =
                    match cs.troller.Status with
                    | ServiceControllerStatus.Running -> DARKGREEN
                    | _ -> DARKGREY
                WriteLine [colour; "Skipped - "; cs.troller.ServiceName]
            x)
        |> Seq.toArray

//    WriteLine  [DARKYELLOW; "Before first Async: "; GetThreadInfo()]

    let job = async {
//        WriteLine  [DARKYELLOW; "In first Async: "; GetThreadInfo()]
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


