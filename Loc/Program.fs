// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CommandLine
open UI

// Turn F# function to Func<> for commandline selection
let Select validation command =
    new System.Func<'arg, unit>(fun x -> 
        validation()
        command x)



//
// https://github.com/gsscoder/commandline/wiki/Display-A-Help-Screen
// commandline parsey library help & docs & whatnot
//


//
// Config Verbs
//
[<Verb("Config", HelpText = "Set the active client and LOC server")>]
type ConfigOptions () = class 
//    [<Value(0, MetaName="Client Code", Required=true, HelpText="Client Code to activate")>]
//    member val Name:string = null with get, set
    [<Option('c', "Client", HelpText="Set the active client")>]
    member val Client:string = null with get, set
        
    [<Option('s', "Server", HelpText="Set the LOC server address")>]
    member val Server:string = null with get, set
    end

//
// All Clients Verbs
//
[<Verb("ListClients", HelpText = "List all installed clients")>]
type ListClientOptions () = class end

[<Verb("ListAllServices", HelpText = "List all running services for all installed clients")>]
type ListAllServicesOptions () = class end

[<Verb("StopAllServices", HelpText = "Stop all running services for all installed clients")>]
type StopAllServicesOptions () = class end

//
// Specific Client Verbs
//
//[<Verb("HAXX", HelpText = "Do hacky testy stuff")>]
//type HAXXOptions () = class end

[<Verb("Stop", HelpText = "Stop services for current client")>]
type StopCurrentClientOptions () = class end

[<Verb("Start", HelpText = "Start services for current client")>]
type StartCurrentClientOptions () = class 
  [<Option('a', "All", HelpText="All client services", SetName="all")>]
  member val All:bool = false with get, set
    
  [<Option('m', "Misc", HelpText="All services except Gimps & Renderers", SetName="partial")>]
  member val Misc:bool = false with get, set

  [<Option("Gimp1", HelpText="Include gimps with index 1 in the instance name", SetName="partial")>]
  member val Gimp1:bool = false with get, set

  [<Option("Ren1", HelpText="Include renderers with index 1 in the instance name", SetName="partial")>]
  member val Renderer1:bool = false with get, set

  [<Option('g', "GimpInstance", HelpText="Include gimps who partially match the provided instance name", SetName="partial")>]
  member val GimpInstance:string = null with get, set

  [<Option('r', "RendererInstance", HelpText="Include renderers who partially match the provided instance name", SetName="partial")>]
  member val RendererInstance:string = null with get, set

  end

[<Verb("Details", HelpText = "Details for current client")>]
type DetailCurrentClientOptions () = class end

[<Verb("Resend", HelpText = "Resend a specific notification")>]
type ResendOptions () = class 
    [<Value(0, MetaName="NotificationId", Required=false, HelpText="NotificationId to send")>]
    member val NotificationId:System.Nullable<int> = System.Nullable<int>() with get, set

    end

// 
// Config Validation Functions
//
let validateServer () =
    match Config.get.LocSpecified with 
    | true -> ()
    | false -> failwith "No LOC server specified"

let validateClient () =
    validateServer()
    match Config.get.ClientSpecified with 
    | true -> ()
    | false -> failwith "No client specified"


let dontValidate () =
    ()

// 
// Config Functions
//
let SetClient client =
    let isValid = Services.getAllClients() |> Seq.contains client
    match isValid with
    | true -> 
        WriteLine [_DARKYELLOW ("Setting current client to %s" + client)]
        Config.SetClient client
    | false -> 
        WriteLine [_RED ("Cannot set current client to %s" + client)]
        Services.PrintAllClients()

let SetServer server =
    WriteLine [_DARKYELLOW ("Setting server to %s" + server)]
    Config.SetServer server

let UpdateConfig (options:ConfigOptions) =
    if options.Server <> null then
        SetServer (options.Server.ToUpper())

    if options.Client <> null then
        SetClient (options.Client.ToUpper())



//
// All Client Functions
//
let ListClients (options:ListClientOptions) = 
    Services.PrintAllClients()

let ListAllServices (options:ListAllServicesOptions) =
    Services.PrintAllRunningServices()

let StopAllServices (options:StopAllServicesOptions) =
    Services.StopAllClients()

//
// Current Client Functions
//
let StopCurrentClient (options:StopCurrentClientOptions) = 
    Services.StopCurrentClient()

let StartCurrentClient (options:StartCurrentClientOptions) = 
    printfn "StartCurrentClient called with All:%b Misc:%b (Gimp1:%b GimpInstance:%s) (Renderer1:%b RendererInstance:%s)" 
            options.All options.Misc options.Gimp1 options.GimpInstance options.Renderer1 options.RendererInstance
    if options.All then
        Services.StartServices true Services.InstanceSelector.All Services.InstanceSelector.All
    else
        Services.StartServices options.Misc
            (match options with
            | x when x.Gimp1 = true -> Services.InstanceSelector.Instance "1"
            | x when x.GimpInstance <> null -> Services.InstanceSelector.Instance options.GimpInstance
            | _ -> Services.InstanceSelector.None)
            (match options with
            | x when x.Renderer1 = true -> Services.InstanceSelector.Instance "1"
            | x when x.RendererInstance <> null -> Services.InstanceSelector.Instance options.RendererInstance
            | _ -> Services.InstanceSelector.None)

let DetailCurrentClient (options:DetailCurrentClientOptions) = 
    Services.DetailCurrentClient()

let ResendNotification (options:ResendOptions) = 
    printfn "resend %A..." options.NotificationId
    match options.NotificationId with
    | x when x.HasValue -> Database.ResendNotification x.Value
    | x -> Database.RecentNotifications() 



[<EntryPoint>]
let main argv = 
    try
        UI.Init()
//
//        UI.HAXX()
//
//        raise ( new System.Exception("bom?"))

           
        WriteLine [_GREEN "can i write an operator to work like a C# + when building strings? take 2 objs, call tostring on em, have a really high precidence so it doesn't need brackets?"]

        WriteLine [_DARKGREY (sprintf " args: %A" argv)]
        let parser = new CommandLine.Parser(fun s -> 
            s.CaseSensitive <- false
            s.HelpWriter <- System.Console.Error)
        let pa = parser.ParseArguments< // Config
                                        ConfigOptions, 
                                        // All Clients
                                        ListClientOptions, ListAllServicesOptions, StopAllServicesOptions,
                                        // Current Client
                                        DetailCurrentClientOptions, StopCurrentClientOptions, StartCurrentClientOptions, ResendOptions
//                                        ,HAXXOptions
                                        >(argv)


//        let retcode = 
        pa.MapResult(
           Select dontValidate UpdateConfig,
           Select validateServer ListClients,
           Select validateServer ListAllServices,
           Select validateServer StopAllServices,
           Select validateClient DetailCurrentClient,
           Select validateClient StopCurrentClient,
           Select validateClient StartCurrentClient,
           Select validateClient ResendNotification,
//           Select dontValidate (fun x->Services.StartServicesSERIAL true Services.InstanceSelector.All Services.InstanceSelector.All),
           fun err -> WriteLine [_DARKYELLOW ("LOC server is " + Config.get.LocAddress + ". Currently selected client is " + Config.get.CurrentClient)])
        //()

    with
    | ex -> WriteLine [_RED ("An exception was thrown. %s" + ex.Message)]




//    Services.Haxx

    printfn "fin."
//    System.Console.ReadKey() |> ignore

    0 // return an integer exit code


// What are the command line patterns that I want?
// General client stuff
// * list clients - list all clients & mention what is currently set if appropriate.
// * set active client
// Services
// * display current running services
// * stop all services for client
// * stop all services for all clients
// * stop specific service? - mebe less as a specific command & more for purge, but could help for stuff like replacing renex code etc.
// * * fuzzyish name matching? "loc restart notification" looks for services called "*notification*" with a case insensitive search.
// * restart specific service? - see above
// * restart all running services? nice & simple to use when hacking at things to ensure all configs are refreshed etc.
// * start services...
// * * all but gimps/renderers
// * * all gimps/renderers
// * * specified gimps/renderers RM1, RB1, other renderers? <gimp>X - eg AW5 needed for bus calls, AW1 for manual retrieves
// * * * common set of gimps/renderers that are configured per-client? eg both AW1 & AW5 from somat like "loc start gimps"
// * change all services to manual start (auto/delayed as options?)
// * * startup types aren't on the troller check this blogpost for a pinvoke way: 
// * * http://peterkellyonline.blogspot.co.uk/2011/04/configuring-windows-service.html
// * * this may be all local machine stuff though - need to dig in about remote access.
// * log purge specific service
// * uninstall - churn through & call uninstall on each service
// * * remove IIS bits too? database?
// * update config - find *.config files for services, check the modified/created date. can we get a service's runtime? 
// * -- if not, restart services where config changed in the last... hour?
//
// DB stuff
// * Resend - no args, list recents (notification & data source?) & prompt for more/index/or exit. with arg, run that'n
// * Arbitrary commands? is this much use outside of management studio? not very interactive...
// Bussy Stuff
// * call to CN.P to send a notification for a datasource? - This will have implications with public use of the hubbery :(
//
// Shortcutty stuff? 
// * quick links to open specific urls - like octopush page for the project
// * open specific files in specific app - like a common set of log files in trailblazer or notepad++ etc.


