// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CommandLine
open UI

//[<AllowNullLiteral>]
[<Verb("SetClient", HelpText = "Set the active client")>]
type SetClientOptions () = class end
//    [<Value(0)>]
////    [<Option('n', Required = true, HelpText = "Client code to activate")>]
//    member val Name = null with get, set

[<Verb("ListClients", HelpText = "List all installed clients")>]
type ListClientOptions () = class end



let UnexpectedArguments argv =
    printfn "Unexpected arguments: %A" argv
    printfn "...display help..."

let ListClients () = 
    printfn "ListClients"
    printfn "Services: %A" Services.GetAllClients

[<EntryPoint>]
let main argv = 
//    printfn "args: %A" argv
//    let parser = new CommandLine.Parser() //.Default
////    parser.Settings.CaseSensitive <- false
////    let pa = CommandLine.Parser.Default.ParseArguments<SetClientOptions, ListClientOptions>(argv)
//    let pa = parser.ParseArguments<SetClientOptions, ListClientOptions>(argv)
//    let retcode = pa.MapResult((fun (x:SetClientOptions) -> printfn "Execute SetClient"), 
//                               (fun (x:ListClientOptions) -> ListClients() ), 
//                               fun err -> printfn "err: %O" argv)
//

    Services.Haxx

    //UI.HAXX

    printfn "fin."
    System.Console.ReadKey() |> ignore

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


