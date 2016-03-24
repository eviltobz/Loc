module UI

open System

let RED = ConsoleColor.Red
let DARKRED = ConsoleColor.DarkRed
let GREEN = ConsoleColor.Green
let DARKGREEN = ConsoleColor.DarkGreen
let DEFAULT = ConsoleColor.Gray
let DARKGREY = ConsoleColor.DarkGray
let DARKYELLOW = ConsoleColor.DarkYellow

let private StartLine = Console.CursorTop

let private printColouredString c (s:string) =
    let old = Console.ForegroundColor
    try
      Console.ForegroundColor <- c;
      Console.Write s
    finally
      Console.ForegroundColor <- old

let private printStringAt printFunc line s = 
    let top = Console.CursorTop
    let left = Console.CursorLeft
    Console.SetCursorPosition(0, StartLine + line)
    Console.Write(new string(' ', Console.WindowWidth))
    Console.SetCursorPosition(0, StartLine + line)
//    printf "(l:%d sl:%d t:%d)" line StartLine top
    printFunc s
    Console.SetCursorPosition(left, top) 

let cprintf c fmt = 
    Printf.kprintf
        (fun s -> printColouredString c s)
        fmt

let cprintfn c fmt =
    Printf.kprintf
        (fun s -> 
            printColouredString c s
            printfn "")
        fmt

let cprintfat line c fmt = 
    let printFunc = printColouredString c
    Printf.kprintf
        (fun s -> 
            printStringAt printFunc line s)
        fmt

let cprintflast c fmt =
    let line = Console.CursorTop - StartLine - 1
    cprintfat line c fmt

let pHack c fmt =
    let currentLine = Console.CursorTop - StartLine
    let printFunc = printColouredString c
    Printf.kprintf
        (fun s -> 
            printFunc s
            printfn ""
            currentLine)
        fmt

let Init() =
    StartLine |> ignore


let HAXX() =
    for i in 0 .. 5 do
        let colour = match i%2 with
                        | 0 -> RED
                        | _ -> GREEN
        cprintfn colour "line %d - Cleft:%d Ctop:%d Wheight:%d WTop:%d" i System.Console.CursorLeft  System.Console.CursorTop  System.Console.WindowHeight System.Console.WindowTop  

    printfn "The starting CursorTop is still %d and the current one is %d" StartLine Console.CursorTop

    System.Threading.Thread.Sleep(1000)

//    for i in 0 .. 5 do
//        match i%2 with
//        | 0 -> cprintfat i DARKRED "TOBZHAXX!!!!!!!!!---------******** %d" i 
//        | _ -> ()
//

    let flow = async {
//        cprintfat 2 DEFAULT "first"
//        do! Async.Sleep 500
//        cprintfat 3 DARKRED "second"
//        do! Async.Sleep 500
//        cprintfat 4 DARKGREEN "third"
//        do! Async.Sleep 500
//        cprintfat 5 RED "forth"

            for i in 0 .. 5 do
                match i%2 with
                | 0 -> cprintfat i DARKRED "TOBZHAXX!!!!!!!!!---------******** %d" i 
                | _ -> ()
                do! Async.Sleep 500

        }

//    Async.StartImmediate flow
    Async.RunSynchronously flow
    flow


