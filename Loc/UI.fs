module UI

open CurrentConfig

open System
open System.Collections.Generic

let DEFAULT = ConsoleColor.Gray
let DARKGREY = ConsoleColor.DarkGray
let RED = ConsoleColor.Red
let DARKRED = ConsoleColor.DarkRed
let GREEN = ConsoleColor.Green
let DARKGREEN = ConsoleColor.DarkGreen
let DARKYELLOW = ConsoleColor.DarkYellow
let YELLOW = ConsoleColor.Yellow
let BLUE = ConsoleColor.Blue
let CYAN = ConsoleColor.Cyan

let private StartLine = Console.CursorTop
let private Lock = new System.Object()

let private writeMessage line (items:obj list) =
    let imp = fun () -> 
        items 
        |> Seq.iter (fun item ->
            match box item with
            | :? ConsoleColor -> Console.ForegroundColor <- (item :?> ConsoleColor)
            | _ -> Console.Write(item.ToString())
            )

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

let WriteAt line (items:obj list) =
    writeMessage (Some line) items

let WriteLine (items:obj list) = 
    let newLined = items @ ["\n"]
    writeMessage None newLined

let LogLine (items:obj list) = 
    if Config.Verbose then
        WriteLine items


let Init() =
    StartLine |> ignore

let NextLineIndex() =
    Console.CursorTop - StartLine

let mutable private CanvasTopLine = 0 
let mutable private CanvasScreenLines = new List<obj list>()
let mutable private CanvasNewLines = new List<obj list>()
let StartCanvas() =
    CanvasTopLine <- 0

let private AddLines number = 
    if number > CanvasTopLine then
        [CanvasTopLine..number-1] |> Seq.iter (fun x -> WriteLine [RED; "AddING: "; x; ", TopLine="; CanvasTopLine; " - "; number])
        System.Threading.Thread.Sleep(777)
        CanvasTopLine <- CanvasTopLine + number

let private RemoveLines newLines oldLines = 
    if oldLines > newLines then
        [newLines..oldLines-1] |> Seq.iter (fun x -> WriteAt x [RED; "KILL ME: "; x; "/"; newLines;"/";CanvasTopLine])
        System.Threading.Thread.Sleep(777)

let CanvasRedraw() =
    // update the screen...
    let lines = CanvasNewLines.Count 
    AddLines lines
    let oldLines = CanvasScreenLines.Count 
    RemoveLines lines oldLines

    // iterate over new lines. compare to old line at index, re-write if different, don't if not.
    for i = 0 to lines - 1 do
           WriteAt i (CanvasNewLines.[i] @ [GREEN; " - i="; i; ", CTL="; CanvasTopLine;])
            

    CanvasScreenLines <- CanvasNewLines
    CanvasNewLines <- new List<obj list>()

let CanvasWriteLine (items:obj list) =
    CanvasNewLines.Add(items)
    

    

let HAXX () =
    StartCanvas()
    CanvasWriteLine ["first line"]
    CanvasWriteLine ["second line"]
    CanvasWriteLine ["third line"]
    CanvasRedraw()
    System.Threading.Thread.Sleep(1000)
    CanvasRedraw()
    System.Threading.Thread.Sleep(1000)
    CanvasWriteLine ["first line"]
    CanvasWriteLine ["second line"]
    CanvasRedraw()
    System.Threading.Thread.Sleep(1000)
    CanvasWriteLine ["first line"]
    CanvasWriteLine ["second line"]
    CanvasWriteLine ["third line"]
    CanvasRedraw()
    System.Threading.Thread.Sleep(1000)
    CanvasWriteLine ["first line"]
    CanvasWriteLine ["second line"]
    CanvasRedraw()
    System.Threading.Thread.Sleep(1000)
    CanvasWriteLine ["first line"]
    CanvasRedraw()

//    WriteLine ["first"]
//    System.Threading.Thread.Sleep(500)
//    WriteLine ["second"]
//    System.Threading.Thread.Sleep(500)
//    WriteLine ["third"]
//    System.Threading.Thread.Sleep(500)
//    WriteLine ["not first"]
//    System.Threading.Thread.Sleep(2000)
