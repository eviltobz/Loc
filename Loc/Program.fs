// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open CommandLine
open Commands
open ConfigCommands
open UI
open CurrentConfig

open System.Management.Automation
open System.Management.Automation.Runspaces

// Turn F# function to Func<> for commandline selection
let Select validation command =
    new System.Func<'arg, unit>(fun x ->
    validation()
    command x)

//
// Config Validation Functions
//
let validateServer() =
    match Config.LocSpecified with
    | true -> ()
    | false -> failwith "No LOC server specified"

let validateClient() =
    validateServer()
    match Config.ClientSpecified with
    | true -> ()
    | false -> failwith "No client specified"

let dontValidate() = ()


//let RunProcess name args =
//    LogLine [ "Starting process: "; DARKYELLOW; name; BLUE; " "; args]
//    let p = new System.Diagnostics.Process()
//    p.StartInfo.FileName <- name
//    p.StartInfo.Arguments <- args
//    p.StartInfo.UseShellExecute <- false
//    p.Start() |> ignore
//    p.WaitForExit()

////
//// Config Functions
////
//let SetClient options =
//    let client = options.Client.ToUpper()
//    let isValid = Services.getAllClients() |> Seq.contains client
//    match isValid with
//    | true ->
//        WriteLine [ DARKYELLOW; "Setting current client to "; client ]
//        Configuration.SetClient client
//    | false ->
//        WriteLine [ RED; "Cannot set current client to "; client ]
//        Services.PrintAllClients()

////let SetServer server =
////    WriteLine [ DARKYELLOW; "Setting server to "; server ]
////    Configuration.SetServer server

//let EditConfig () =
//    let configPath = System.AppDomain.CurrentDomain.BaseDirectory + @"loc.exe.config"
//    let editor = Config.Editor
//    WriteLine [DARKYELLOW; "Opening config file "; DEFAULT; configPath; DARKYELLOW; " for editing with "; DEFAULT; editor]
//    RunProcess editor ("\"" + configPath + "\"")

//let UpdateConfig(options : ConfigOptions) =
//    match options with
//    | o when o.Edit -> EditConfig()
//    | o when o.Verbose ->
//        WriteLine  [ DARKYELLOW; "Turning Verbose ";  (if Config.Verbose then "Off" else "On") ]
//        ToggleVerbosity()
//    | o when o.Client <> null -> SetClient options.Client
//    | _ -> WriteLine ["boo"]

//    match options.Edit with
//    | true ->
//        let configPath = System.AppDomain.CurrentDomain.BaseDirectory + @"loc.exe.config"
//        let editor = Config.Editor
//        WriteLine [DARKYELLOW; "Opening config file "; DEFAULT; configPath; DARKYELLOW; " for editing with "; DEFAULT; editor]
//        RunProcess editor ("\"" + configPath + "\"")
//    | false ->
//        //if options.Server <> null then SetServer(options.Server.ToUpper())
//        //if options.Client <> null then SetClient(options.Client.ToUpper())
//        //if options.Verbose then ToggleVerbosity()
//        if options.Client = null then
//            let WriteList  title items =
//                let formattedItems = items |> Seq.map (fun item -> [DARKYELLOW; item; DEFAULT; "; "]:obj list )
//                WriteLine (title :: (formattedItems |> Seq.concat |> Seq.toList))
//            WriteLine [ DARKYELLOW; "Current config:"]
//            WriteLine [ "  Loc address        : "; DARKYELLOW; Config.LocAddress]
//            WriteLine [ "  Config editor      : "; DARKYELLOW; Config.Editor]
//            WriteLine [ "  Source location    : "; DARKYELLOW; Config.SourceFolder]
//            WriteList   "  Octopus cache dirs : "  Config.OctopusPurgeDirs
//            WriteLine [ "  Octopus API key    : "; DARKYELLOW; Config.OctopusApiKey]
//            WriteLine [ "  Verbose output     : "; DARKYELLOW; Config.Verbose]
//            WriteLine []
//            WriteLine [ "  Current client     : "; CYAN; Config.CurrentClient ]
//            WriteLine [ "  Octopus project    : "; DARKYELLOW; Config.OctopusProject ]
//            WriteList   "    Packaging nugets : " Config.NugetsToBuild
//            WriteList   "    Skipping steps   : " Config.StepsToSkip

//
// All Client Functions
//
let AllClients(options : AllClientsOptions) =
    match options.Clients, options.Running, options.Stop with
    | true, false, false -> Services.PrintAllClients()
    | false, true, false -> Services.PrintAllRunningServices()
    | false, false, true -> Services.StopAllClients()
    | _,_,_ -> WriteLine [RED; "Please specify either Clients, Running, or Stop"]

//
// Current Client Functions
//
let StopCurrentClient(options : StopCurrentClientOptions) =
    if System.String.IsNullOrWhiteSpace options.Instance then Services.StopCurrentClient()
    else Services.StopInstance options.Instance

let StartCurrentClient(options : StartCurrentClientOptions) =
    printfn
        "StartCurrentClient called with All:%b Misc:%b GimpInstance:%s RendererInstance:%s (Instance:%s)"
        options.All options.Misc options.GimpInstance options.RendererInstance
        options.Instance
    let allFilterFunc name instance = true

    let allFilterFunc name instance =
        match name with
        | "specific instance" -> false
        | "like gimp" -> false // do stuff
        | "like renderer" -> false // do stuff
        | "misc" -> false
    if options.All then
        Services.StartServices true Services.InstanceSelector.All Services.InstanceSelector.All
            Services.InstanceSelector.None
    if not <| System.String.IsNullOrWhiteSpace options.Instance then
        printfn "instancetastic"
        Services.StartServices false Services.InstanceSelector.None Services.InstanceSelector.None
            (Services.InstanceSelector.Instance options.Instance)
    else
        let gimpOptions =
            (match options with
             | x when x.GimpInstance <> null -> Services.InstanceSelector.Instance options.GimpInstance
             | _ -> Services.InstanceSelector.None)

        let renOptions =
            (match options with
             | x when x.RendererInstance <> null -> Services.InstanceSelector.Instance options.RendererInstance
             | _ -> Services.InstanceSelector.None)

        Services.StartServices options.Misc gimpOptions renOptions Services.InstanceSelector.None

let StatusCurrentClient(options : StatusCurrentClientOptions) = Services.StatusCurrentClient()

let ResendNotification(options : ResendOptions) =
    printfn "resend %A..." options.NotificationId
    match options.NotificationId with
    | x when x.HasValue -> Database.ResendNotification x.Value
    | _ -> Database.RecentNotifications()
    if not options.Quiet then Database.MonitorActivity()

let ClearScheduler(options : ClearSchedulerOptions) = Database.ClearScheduler()
let Monitor(options : MonitorOptions) = Database.MonitorActivity()


let PurgeOctopusCache(paths) =
    match Seq.length paths with
    | 0 -> WriteLine [RED; "No directories configured under OctopusPurgeDirs"]
    | _ ->
        let connectionInfo = new WSManConnectionInfo()
        connectionInfo.ComputerName <- Config.LocAddress
        let runspace = RunspaceFactory.CreateRunspace(connectionInfo)

        try
            runspace.Open()
        with
        | ex ->
            WriteLine [ RED; "  Error connecting to server "; Config.LocAddress; " to purge Octopus cache folders. ";
                        DARKYELLOW; "  Try running this command on the target server: Enable-PSRemoting -force"]
            reraise()

        let ps = PowerShell.Create()
        ps.Runspace <- runspace
        WriteLine [ YELLOW; "Clearing Octopus cache locations:"]

        let y = paths |> Seq.length
        paths |> Seq.iteri (fun x path ->
                     WriteLine [ YELLOW; " "; (x+1); "/"; y; " "; DEFAULT; path]
                     let cmd =
                        @"if (Test-Path """ + path + @""")" +
                        // Yep, calling cmd from powershell. It's the easiest way to bin a whole folder
                        @" { cmd /C ""rmdir /s /q """"" + path + @""""""" } "
                     ps.Commands.Clear()
                     ps.AddScript(cmd).Invoke() |> ignore )

//let localPowershell = PowerShell.Create()
//let exec cmd =
//    localPowershell.Commands.Clear()
//    //WriteLine [GREEN; "execing: "; DEFAULT; cmd ]
//    let results = localPowershell.AddScript(cmd).Invoke()
//    results |> Seq.iter (fun r -> WriteLine[ "r:"; r.ToString() ])
//    localPowershell.Streams.Debug |> Seq.iter (fun x -> WriteLine["debug:"; x.Message])
//    localPowershell.Streams.Error |> Seq.iter (fun x -> WriteLine["error:"; RED; x.Exception])
//    localPowershell.Streams.Progress |> Seq.iter (fun x -> WriteLine["progress:"; x.StatusDescription])
//    localPowershell.Streams.Verbose |> Seq.iter (fun x -> WriteLine["verbose:"; x.Message])
//    localPowershell.Streams.Warning |> Seq.iter (fun x -> WriteLine["warning:"; x.Message])

let Deploy(options : DeployOptions) =
    if not Config.OctopusProjectSpecified then
        WriteLine [ RED; "No octopus project specified for client "; Config.CurrentClient]
    else
        match options.BuildOnly with
        | true -> WriteLine [ GREEN; "Starting BuildOnly for "; YELLOW; Config.CurrentClient; GREEN; " from "; YELLOW; Config.SourceFolder; ]
        | false -> WriteLine [ GREEN; "Starting Deploy for "; YELLOW; Config.CurrentClient; GREEN; " from "; YELLOW; Config.SourceFolder;
                               GREEN; " to "; YELLOW; Config.OctopusProject; GREEN; " on "; YELLOW; Config.LocAddress]


        let exec cmd = Run.Process "powershell.exe" ("-ExecutionPolicy Unrestricted -NoProfile " + cmd)
        let scriptDir = System.AppDomain.CurrentDomain.BaseDirectory

        let command = Config.SourceFolder + @"\packages\NuGet.CommandLine\tools\nuget pack " + Config.SourceFolder + @"\Deployment\NuSpecs\"
        let args = ".nuspec -nopackageanalysis -outputdirectory " + Config.SourceFolder + @"\NugetFeed -basepath " + Config.SourceFolder
        Config.NugetsToBuild |> Seq.iter (fun name ->
            let fullCommand = command + name + args
            WriteLine []
            WriteLine [YELLOW; "Building Package "; name]
            //exec fullCommand
            exec fullCommand)


        if not options.BuildOnly then

            PurgeOctopusCache Config.OctopusPurgeDirs
            let skips = Config.StepsToSkip |> Seq.fold (fun acc step -> acc + " --skip=" + step) ""
            let deployCommand = sprintf "octo deploy-release --progress --forcepackagedownload --project %s --deployto LOC --version latest --waitfordeployment --server http://%s:8080 --apiKey %s %s"
                                        Config.OctopusProject Config.LocAddress Config.OctopusApiKey skips
            exec deployCommand

        //                              scriptDir Config.CurrentClient Config.LocAddress Config.SourceFolder options.BuildOnly Config.OctopusProject Config.OctopusApiKey Config.NugetsToBuild Config.StepsToSkip)

        //RunProcess "powershell.exe" (sprintf
        //                              @"-ExecutionPolicy Unrestricted -NoProfile -File ""%sScripts\BuildAndDeploy.ps1"" %s %s ""%s"" %b %s %s ""%s"" ""%s"" "
        //                              scriptDir Config.CurrentClient Config.LocAddress Config.SourceFolder options.BuildOnly Config.OctopusProject Config.OctopusApiKey Config.NugetsToBuild Config.StepsToSkip)

[<EntryPoint>]
let main argv =
    try
        //        UI.Init()
        //
        //        UI.HAXX()
        //
        //        raise ( new System.Exception("bom?"))
        //        WriteLine [DARKGREY; (sprintf " args: %A" argv)]
        let parser =
            new CommandLine.Parser(fun s ->
            s.CaseSensitive <- false
            s.HelpWriter <- System.Console.Error)

        let pa =
            parser.ParseArguments< // Config
                                   ConfigOptions,
                                   // All Clients
                                   AllClientsOptions,
                                   // Current Client
                                   StatusCurrentClientOptions, StopCurrentClientOptions, StartCurrentClientOptions,
                                   ResendOptions, ClearSchedulerOptions, MonitorOptions, DeployOptions
                                   >
                (argv)
        //        let retcode =
        pa.MapResult
            (Select dontValidate UpdateConfig,
             Select validateServer AllClients,
             Select validateClient StatusCurrentClient,
             Select validateClient StopCurrentClient, Select validateClient StartCurrentClient,
             Select validateClient ResendNotification, Select validateClient ClearScheduler,
             Select validateClient Monitor, Select validateClient Deploy,
             fun err -> ()
             )
    //()
    with ex -> WriteLine [ RED; "An exception was thrown. "; ex.Message ]
    //    Services.Haxx
    //    printfn "fin."
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
// * Monitory stuff - db polling, queue inspection, your mum. display info about current jobs
// * * gimps
// * * renderer
// * * ???
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
// * * this is likely less useful now that LOC is running on my box & remoting to the VM

