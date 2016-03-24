// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CommandLine
open UI

// Turn F# function to Func<> for commandline selection
let Select func =
    new System.Func<'arg, unit>(fun x -> func x)

//[<AllowNullLiteral>]
[<Verb("SetClient", HelpText = "Set the active client")>]
type SetClientOptions () = class //end
    [<Value(0, MetaName="Client Code", Required=true, HelpText="Client Code to activate")>]
    member val Name:string = null with get, set
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
[<Verb("Stop", HelpText = "Stop services for current client")>]
type StopCurrentClientOptions () = class end


//let UnexpectedArguments argv =
//    printfn "Unexpected arguments: %A" argv
//    printfn "...display help..."

//
// All Client Functions
//
let ListClients (options:ListClientOptions) = 
    Services.PrintAllClients()

let SetClient (options:SetClientOptions) = 
    cprintfn RED "Todo - Set current client to %s" (options.Name.ToUpper())

let ListAllServices (options:ListAllServicesOptions) =
    Services.PrintAllRunningServices()

let StopAllServices (options:StopAllServicesOptions) =
    Services.StopAllClients()
//
// Current Client Functions
//
let StopCurrentClient (options:StopCurrentClientOptions) = 
    Services.StopCurrentClient()


[<EntryPoint>]
let main argv = 
    UI.Init()
    cprintfn DARKGREY " args: %A" argv
    let parser = new CommandLine.Parser(fun s -> 
        s.CaseSensitive <- false
        s.HelpWriter <- System.Console.Error)
    let pa = parser.ParseArguments< // All Clients
                                    SetClientOptions, ListClientOptions, ListAllServicesOptions, StopAllServicesOptions,
                                    // Current Client
                                    StopCurrentClientOptions
                                    >(argv)


    let retcode = 
        pa.MapResult(
           Select SetClient,
           Select ListClients,
           Select ListAllServices,
           Select StopAllServices,
           Select StopCurrentClient,
           fun err -> cprintfn DARKYELLOW "Currently selected client is %s" Config.get.CurrentClient)





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
//
// Shortcutty stuff? 
// * quick links to open specific urls - like octopush page for the project
// * open specific files in specific app - like a common set of log files in trailblazer or notepad++ etc.


