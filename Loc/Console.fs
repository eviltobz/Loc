module Console

let printColouredString c (s:string) =
    let old = System.Console.ForegroundColor
    try
      System.Console.ForegroundColor <- c;
      System.Console.Write s
    finally
      System.Console.ForegroundColor <- old

let cprintfOLD c fmt = 
    Printf.kprintf
        (fun s ->
            let old = System.Console.ForegroundColor
            try
              System.Console.ForegroundColor <- c;
              System.Console.Write s
            finally
              System.Console.ForegroundColor <- old)
        fmt


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

