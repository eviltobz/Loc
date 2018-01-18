module Run

open UI

let Process name args =
    LogLine [ "Starting process: "; DARKYELLOW; name; BLUE; " "; args]
    let p = new System.Diagnostics.Process()
    p.StartInfo.FileName <- name
    p.StartInfo.Arguments <- args
    p.StartInfo.UseShellExecute <- false
    p.Start() |> ignore
    p.WaitForExit()


