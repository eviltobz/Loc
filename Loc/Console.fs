module UI

open System

let RED = System.ConsoleColor.Red
let DARKRED = System.ConsoleColor.DarkRed
let GREEN = System.ConsoleColor.Green
let DARKGREEN = System.ConsoleColor.DarkGreen

let private StartLine = Console.CursorTop

let private printColouredString c (s:string) =
    let old = System.Console.ForegroundColor
    try
      System.Console.ForegroundColor <- c;
      System.Console.Write s
    finally
      System.Console.ForegroundColor <- old

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

let printfat line fmt = 
    let printFunc = fun (s:string) -> Console.Write s
    Printf.kprintf
        (fun s -> 
            printStringAt printFunc line s)
        fmt

let cprintfat line c fmt = 
    let printFunc = printColouredString c
    Printf.kprintf
        (fun s -> 
            printStringAt printFunc line s)
        fmt

let HAXX =
    
    for i in 0 .. 100 do
        let colour = match i%2 with
                        | 0 -> RED
                        | _ -> GREEN
        cprintfn colour "line %d - Cleft:%d Ctop:%d Wheight:%d WTop:%d" i System.Console.CursorLeft  System.Console.CursorTop  System.Console.WindowHeight System.Console.WindowTop  

    printfn "The starting CursorTop is still %d and the current one is %d" StartLine Console.CursorTop

    System.Threading.Thread.Sleep(1000)

    for i in 0 .. 10 do
        let line = i*i
        match i%2 with
        | 0 -> cprintfat line DARKRED "TOBZHAXX!!!!!!!!!---------******** %d %d" i line
        | _ -> printfat line "TOBZHAXX!!!!!!!!!---------******** %d %d" i line

//    let realTop = Console.CursorTop
//    let realLeft = Console.CursorLeft
//    System.Console.SetCursorPosition(0, (StartLine + 10))
//    cprintfn DARKRED "******** HAXXXX *********** - Cleft:%d Ctop:%d Wheight:%d WTop:%d" System.Console.CursorLeft  System.Console.CursorTop  System.Console.WindowHeight System.Console.WindowTop  
//    
//    System.Console.SetCursorPosition(realLeft, realTop)
