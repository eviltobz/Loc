module UI

open System

let RED = ConsoleColor.Red
let DARKRED = ConsoleColor.DarkRed
let GREEN = ConsoleColor.Green
let DARKGREEN = ConsoleColor.DarkGreen
let DEFAULT = ConsoleColor.Gray

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
    Console.SetCursorPosition(0, line - StartLine)
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


let HAXX =
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
    let HardcodeyLine = 7
    let flow = async {
        cprintfn DEFAULT "Updatey. WHY ME NO WORKEE? :("
        do! Async.Sleep 500
        cprintfat HardcodeyLine RED "WTF?!?!"
//        cprintfat  HardcodeyLine RED "Updatey 1"
//        do! Async.Sleep 500
//        cprintfat  HardcodeyLine DARKRED "Updatey 2"
//        do! Async.Sleep 500
//        cprintfat  HardcodeyLine GREEN "Updatey 3"
//        do! Async.Sleep 500
//        cprintfat  HardcodeyLine DARKGREEN "Updatey 4"
//        do! Async.Sleep 500
        }

    
    Async.RunSynchronously flow
//    let a = Async.StartAsTask flow

    ()

