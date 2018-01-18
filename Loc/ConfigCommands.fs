module ConfigCommands

open UI
open CurrentConfig
open ConfigManager
open CurrentConfig

open CommandLine

[<Verb("Config", HelpText = "Set the active client and LOC server")>]
type ConfigOptions() = 
    class
        [<Option('c', "Client", HelpText = "Set the active client", SetName="Client")>]
        member val Client : string = null with get, set

        [<Option('e', "Edit", HelpText = "Open config in your configured editor", SetName="Edit")>]
        member val Edit : bool = false with get, set

        [<Option('v', "Verbose", HelpText = "Toggle verbose output", SetName="Verbose")>]
        member val Verbose : bool = false with get, set
    end


let private UpdateClient (options : ConfigOptions) = 
    let client = options.Client.ToUpper()
    let isValid = Services.getAllClients() |> Seq.contains client
    match isValid with
    | true -> 
        WriteLine [ DARKYELLOW; "Setting current client to "; client ]
        SetClient client
    | false -> 
        WriteLine [ RED; "Cannot set current client to "; client ]
        Services.PrintAllClients()

//let SetServer server = 
//    WriteLine [ DARKYELLOW; "Setting server to "; server ]
//    Configuration.SetServer server

let EditConfig () =
    let configPath = System.AppDomain.CurrentDomain.BaseDirectory + @"loc.exe.config"
    let editor = Config.Editor
    WriteLine [DARKYELLOW; "Opening config file "; DEFAULT; configPath; DARKYELLOW; " for editing with "; DEFAULT; editor]
    Run.Process editor ("\"" + configPath + "\"")

let Display () =
    let config = ReadConfig()
    let WriteList  title items =
        let formattedItems = items |> Seq.map (fun item -> [DARKYELLOW; item; DEFAULT; "; "]:obj list )
        WriteLine (title :: (formattedItems |> Seq.concat |> Seq.toList))
    WriteLine [ DARKYELLOW; "Current config:"] 
    WriteLine [ "  Loc address        : "; DARKYELLOW; config.LocAddress]
    WriteLine [ "  config editor      : "; DARKYELLOW; config.Editor]
    WriteLine [ "  Source location    : "; DARKYELLOW; config.SourceFolder]
    WriteList   "  Octopus cache dirs : "  config.OctopusPurgeDirs
    WriteLine [ "  Octopus API key    : "; DARKYELLOW; config.OctopusApiKey]
    WriteLine [ "  Verbose output     : "; DARKYELLOW; config.Verbose]
    WriteLine []
    WriteLine [ "  Current client     : "; CYAN; config.CurrentClient ]
    WriteLine [ "  Octopus project    : "; DARKYELLOW; config.OctopusProject ]
    WriteList   "    Packaging nugets : " config.NugetsToBuild 
    WriteList   "    Skipping steps   : " config.StepsToSkip 

let UpdateConfig(options : ConfigOptions) = 
    match options with
    | o when o.Edit -> EditConfig()
    | o when o.Verbose -> 
        WriteLine  [ DARKYELLOW; "Turning Verbose ";  (if Config.Verbose then "Off" else "On") ]
        ToggleVerbosity()
    | o when o.Client <> null -> UpdateClient options
    | _ -> ()
    Display()
    WriteLine [RED; "hmmm, we're doing a fresh ReadConfig call, but it's not refreshing that altered data. needs investigrating" ]


    //match options.Edit with
    //| true -> 
    //    let configPath = System.AppDomain.CurrentDomain.BaseDirectory + @"loc.exe.config"
    //    let editor = Config.Editor
    //    WriteLine [DARKYELLOW; "Opening config file "; DEFAULT; configPath; DARKYELLOW; " for editing with "; DEFAULT; editor]
    //    Run.Process editor ("\"" + configPath + "\"")
    //| false ->
        //if options.Server <> null then SetServer(options.Server.ToUpper())
        //if options.Client <> null then SetClient(options.Client.ToUpper())
        //if options.Verbose then ToggleVerbosity()
        //if options.Client = null then 
            //let WriteList  title items =
            //    let formattedItems = items |> Seq.map (fun item -> [DARKYELLOW; item; DEFAULT; "; "]:obj list )
            //    WriteLine (title :: (formattedItems |> Seq.concat |> Seq.toList))
            //WriteLine [ DARKYELLOW; "Current config:"] 
            //WriteLine [ "  Loc address        : "; DARKYELLOW; Config.LocAddress]
            //WriteLine [ "  Config editor      : "; DARKYELLOW; Config.Editor]
            //WriteLine [ "  Source location    : "; DARKYELLOW; Config.SourceFolder]
            //WriteList   "  Octopus cache dirs : "  Config.OctopusPurgeDirs
            //WriteLine [ "  Octopus API key    : "; DARKYELLOW; Config.OctopusApiKey]
            //WriteLine [ "  Verbose output     : "; DARKYELLOW; Config.Verbose]
            //WriteLine []
            //WriteLine [ "  Current client     : "; CYAN; Config.CurrentClient ]
            //WriteLine [ "  Octopus project    : "; DARKYELLOW; Config.OctopusProject ]
            //WriteList   "    Packaging nugets : " Config.NugetsToBuild 
            //WriteList   "    Skipping steps   : " Config.StepsToSkip 

