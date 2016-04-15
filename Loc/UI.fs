module UI

open System

let RED = ConsoleColor.Red
let DARKRED = ConsoleColor.DarkRed
let GREEN = ConsoleColor.Green
let DARKGREEN = ConsoleColor.DarkGreen
let DEFAULT = ConsoleColor.Gray
let DARKGREY = ConsoleColor.DarkGray
let DARKYELLOW = ConsoleColor.DarkYellow

let _RED s = (ConsoleColor.Red, s)
let _DARKRED s = (ConsoleColor.DarkRed, s)
let _GREEN s = (ConsoleColor.Green, s)
let _DARKGREEN s = (ConsoleColor.DarkGreen, s)
let _DEFAULT s = (ConsoleColor.Gray, s)
let _DARKGREY s = (ConsoleColor.DarkGray, s)
let _DARKYELLOW s = (ConsoleColor.DarkYellow, s)

let private StartLine = Console.CursorTop
let private Lock = new System.Object()
//
//let private writeMessage (message:(ConsoleColor*Object list) list) = 
//    message |> Seq.iter (fun (colour, texts) ->
//        Console.ForegroundColor <- colour
//        texts |> Seq.iter Console.Write
//    )
//
//let WriteAt line (message:(ConsoleColor*Object list) list) = 
//    let old = Console.ForegroundColor
//    writeMessage message
//    Console.ForegroundColor <- old
//
//let Write (message:(ConsoleColor*Object list) list) = 
//    let old = Console.ForegroundColor
//    writeMessage message
//    Console.ForegroundColor <- old
//
//let WriteLine (message:(ConsoleColor*Object list) list) = 
//    let newLined = message @ [(RED, ["\n"])]
//    Write newLined
//


let private writeMessage line (message:(ConsoleColor*string) list) = 
    let imp = fun () -> message |> Seq.iter (fun (colour, text) ->
        Console.ForegroundColor <- colour
        Console.Write text)

    lock Lock (fun () ->
        let oldColour = Console.ForegroundColor

        match line with
        | None -> imp()
        | Some x ->
            let oldTop = Console.CursorTop
            Console.SetCursorPosition(0, StartLine + x)
            Console.Write(new string(' ', Console.WindowWidth))
            Console.SetCursorPosition(0, StartLine + x)
            imp()
            Console.SetCursorPosition(0, oldTop)

        Console.ForegroundColor <- oldColour
        )

let WriteAt line (message:(ConsoleColor*string) list) = 
//    let oldTop = Console.CursorTop
//    let oldColour = Console.ForegroundColor
//
//    Console.SetCursorPosition(0, StartLine + line)
    writeMessage (Some line) message

//    Console.SetCursorPosition(0, oldTop)
//    Console.ForegroundColor <- oldColour

let Write (message:(ConsoleColor*string) list) = 
//    let old = Console.ForegroundColor
    writeMessage None message
//    Console.ForegroundColor <- old

let WriteLine (message:(ConsoleColor*string) list) = 
    let newLined = message @ [(DEFAULT, "\n")]
    Write newLined




//
//let HAXX () =
//    Write [(RED, "in red"); (GREEN, "in green")]
//    WriteLine [(DEFAULT, "things and stuff")]
//
//let private printColouredString c (s:string) =
//    lock Lock (fun () ->
//        let old = Console.ForegroundColor
//        try
//          Console.ForegroundColor <- c;
//          Console.Write s
//        finally
//          Console.ForegroundColor <- old
//    )
//
//let private printStringAt printFunc line s = 
//    lock Lock (fun () ->
//        let top = Console.CursorTop
//        let left = Console.CursorLeft
//        Console.SetCursorPosition(0, StartLine + line)
//        Console.Write(new string(' ', Console.WindowWidth))
//        Console.SetCursorPosition(0, StartLine + line)
//    //    printf "(l:%d sl:%d t:%d)" line StartLine top
//        printFunc s
//        Console.SetCursorPosition(left, top) 
//    )
//
//let cprintf c fmt = 
//    lock Lock (fun () ->
//        Printf.kprintf
//            (fun s -> printColouredString c s)
//            fmt
//            )
//
//let cprintfn c fmt =
//    lock Lock (fun () ->
//        Printf.kprintf
//            (fun s -> 
//                printColouredString c s
//                printfn "")
//            fmt
//            )
//
//let cprintfat line c fmt = 
//    lock Lock (fun () ->
//        let printFunc = printColouredString c
//        Printf.kprintf
//            (fun s -> 
//                printStringAt printFunc line s)
//            fmt
//        )
//
//let cprintflast c fmt =
//    lock Lock (fun () ->
//        let line = Console.CursorTop - StartLine - 1
//        cprintfat line c fmt
//        )
//
//let private pHack c fmt =
//    let currentLine = Console.CursorTop - StartLine
//    let printFunc = printColouredString c
//    Printf.kprintf
//        (fun s -> 
//            printFunc s
//            printfn ""
//            currentLine)
//        fmt

let Init() =
    StartLine |> ignore

let NextLineIndex() =
    Console.CursorTop - StartLine


