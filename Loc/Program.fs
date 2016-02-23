// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CommandLine

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
    printfn "args: %A" argv
    
    let parser = new CommandLine.Parser() //.Default
    parser.Settings.CaseSensitive <- false

//    let pa = CommandLine.Parser.Default.ParseArguments<SetClientOptions, ListClientOptions>(argv)
    let pa = parser.ParseArguments<SetClientOptions, ListClientOptions>(argv)

    let retcode = pa.MapResult((fun (x:SetClientOptions) -> printfn "Execute SetClient"), 
                               (fun (x:ListClientOptions) -> ListClients() ), 
                               fun err -> printfn "err: %O" argv)


    0 // return an integer exit code
