module Commands

open CommandLine

//
// https://github.com/gsscoder/commandline/wiki/Display-A-Help-Screen
// commandline parsey library help & docs & whatnot
//
//
// Config Verbs
//
//[<Verb("Config", HelpText = "Set the active client and LOC server")>]
//type ConfigOptions() = 
//    class
//        [<Option('c', "Client", HelpText = "Set the active client", SetName="Args")>]
//        member val Client : string = null with get, set

//        [<Option('e', "Edit", HelpText = "Open config in your configured editor", SetName="Edit")>]
//        member val Edit : bool = false with get, set

//        [<Option('v', "Verbose", HelpText = "Toggle verbose output", SetName="Args")>]
//        member val Verbose : bool = false with get, set
//    end

//
// All Clients Verbs
//
[<Verb("All", HelpText = "List or stop services for all clients on the LOC")>]
type AllClientsOptions() = 
    class
        [<Option('c', "Clients", HelpText = "List all the installed clients", SetName="clients", Required = true )>]
        member val Clients : bool = false with get, set
        
        [<Option('r', "Running", HelpText = "List the running services for all installed clients", SetName="running", Required = true)>]
        member val Running : bool = false with get, set

        [<Option('s', "Stop", HelpText = "Stop the running services for all installed clients", SetName="stop", Required = true)>]
        member val Stop : bool = false with get, set
    end

//
// Specific Client Verbs
//
[<Verb("Stop", HelpText = "Stop services for current client")>]
type StopCurrentClientOptions() = 
    class
        [<Option('i', "Instance", HelpText = "Stop services whose instance partially matches the provided name", SetName = "partial")>]
        member val Instance : string = null with get, set
    end

[<Verb("Start", HelpText = "Start services for current client")>]
type StartCurrentClientOptions() = 
    class
        [<Option('a', "All", HelpText = "All client services", SetName = "all")>]
        member val All : bool = false with get, set
        
        [<Option('m', "Misc", HelpText = "All services except Gimps & Renderers", SetName = "partial")>]
        member val Misc : bool = false with get, set
        
        [<Option('g', "GimpInstance", HelpText = "Include gimps who partially match the provided instance name", SetName = "partial")>]
        member val GimpInstance : string = null with get, set
        
        [<Option('r', "RendererInstance", HelpText = "Include renderers who partially match the provided instance name", SetName = "partial")>]
        member val RendererInstance : string = null with get, set
        
        [<Option('i', "Instance", HelpText = "Include any  services who partially match the provided instance name", SetName = "instance")>]
        member val Instance : string = null with get, set
    end

[<Verb("Status", HelpText = "Status for current client")>]
type StatusCurrentClientOptions() = 
    class
    end

[<Verb("Resend", HelpText = "Resend a specific notification")>]
type ResendOptions() = 
    class
        
        [<Value(0, MetaName = "NotificationId", Required = false, HelpText = "NotificationId to send")>]
        member val NotificationId : System.Nullable<int> = System.Nullable<int>() with get, set
        
        [<Option('q', "Quiet", HelpText = "Don't monitor the send after completion")>]
        member val Quiet : bool = false with get, set
    end

[<Verb("ClearScheduler", HelpText = "Clear all notifications from scheduler thingy in the old UI")>]
type ClearSchedulerOptions() = 
    class
    end

[<Verb("Monitor", HelpText = "Monitor current activity")>]
type MonitorOptions() = 
    class
    end



[<Verb("Deploy", HelpText = "Package the configured nugets & run the deployment")>]
type DeployOptions() = 
    class
        [<Option('b', "BuildOnly", HelpText = "Only build the nugets, don't push to Octopus")>]
        member val BuildOnly : bool = false with get, set
    end

