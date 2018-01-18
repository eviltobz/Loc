module Database

open System.Data.SqlClient
open UI
open CurrentConfig

type NotificationStatus = 
        NotSent = 0 | Processing = 1 | Sent = 2 | RecipientsCreated = 3 | Sending = 4 | Rendered = 5 | Pause = 6 | Paused = 7 | Abandon = 8 | Abandoned = 9 | RecipientsCreated2 = -3

type Notification = {
    Id:int
    Name:string
    Template:string
    DataSource:string
    RecipientCount:int
    }

type Import = {
    Instance:string
    Date:System.DateTime
    Name:string
    Type:string
    }

type RendererMonitor = {
    NotificationId:int
    Status:NotificationStatus
    RendererInstance:string
    Total:int
    Started:int
    Finished:int
    }

    

let private getCommand sql =
    let conn = new SqlConnection ("Data Source=" + Config.LocAddress + ";Initial catalog=" + Config.CurrentClient + "-LOC-PASNGR;Integrated Security=SSPI")
    conn.Open()
    let command = conn.CreateCommand()
    command.CommandText <- sql 
    command

let private loadRecentNotifications () = 
    let sql = @"select top 100 sn.notificationId,sn.notification_desc, template_desc, data_source_name, recipientCount
                                from tbl_schedule_notifications sn
                                    inner join tbl_queue_notifications qn on schd_batchID = 'QID' + CAST(qn.notificationID as varchar(10))
                                    inner join notificationSource ns on qn.notificationID = ns.notificationId
                                    inner join tbl_data_source ds on ns.dataSourceId = ds.data_sourceID
                                    inner join tbl_texts_template t on qn.nt1_templateID = t.templateID 
                                    inner join (
										select notificationId, COUNT(*) as recipientCount
										from tbl_batch_notifications bn inner join tbl_batch_notifications_recipients bnr on bn.ID = bnr.BatchNotificationID
										group by notificationId
										) as r on sn.notificationID = r.NotificationID
                                order by sn.notificationid desc"
    use command = getCommand sql
    use reader = command.ExecuteReader()
    WriteLine [DARKGREY; "Received Data..."]
    let loaded = seq {
        while reader.Read() do
            let id = reader.GetInt32 0
            let name = reader.GetString 1
            let template = reader.GetString 2
            let datasource = reader.GetString 3
            let count = reader.GetInt32 4
            yield {Id=id; Name=name; Template=template; DataSource=datasource; RecipientCount=count}
        }
    loaded |> Seq.toList

let private writeNotification n =
    WriteLine 
        [DARKGREEN; n.Id; ":"; DEFAULT; n.Name; DARKGREY; " ("; 
        DEFAULT; "#:"; DARKYELLOW; n.RecipientCount;
        DEFAULT; " / T:"; DARKGREY; n.Template; 
        DEFAULT; " / DS:"; DARKGREY; n.DataSource; ")"]


//
// Public API Bits
//

let ClearScheduler () =
    let sql = "update tbl_schedule_notifications set schd_sent = 2"
    use command = getCommand sql
    command.ExecuteNonQuery() |> ignore
    WriteLine [RED; "Cleared all notifications from scheduler"]

let LoadActivity (command:SqlCommand) =
    use reader = command.ExecuteReader()
    let notifications = 
        seq {
            while reader.Read() do
                let NotificationId = reader.GetInt32 0
                let Status = enum<NotificationStatus>(reader.GetInt32 1)
                let ren = 
                    match reader.IsDBNull 2 with 
                    | true -> ""
                    | false -> reader.GetString 2
                let total, started, finished =
                    match reader.IsDBNull 3 with
                    | true -> (0, 0, 0)
                    | false -> ((reader.GetInt32 3),
                                (reader.GetInt32 4),
                                (reader.GetInt32 5))
                yield {
                    NotificationId = NotificationId;
                    Status = Status;
                    RendererInstance = ren;
                    Total = total;
                    Started = started;
                    Finished = finished
                    }
        } |> Seq.toList

    reader.NextResult() |> ignore
    let imports = 
        seq {
            while reader.Read() do
               let instance = reader.GetString 0
               let date = reader.GetDateTime 1
               let name = reader.GetString 2
               let reqType = reader.GetString 2

               yield {
                    Instance = instance;
                    Date = date;
                    Name = name;
                    Type = reqType;
               }
        } |> Seq.toList
    (notifications, imports)


// ICK - for something that's meant to be in a database module this is doing a whole lot of 
// manipulation of the texty output - methinks that i need to persue the canvas idea.
let PrintActivity startIndex (notifications:RendererMonitor list) (imports:Import list) = 
    let f x =
        match x with
        | 0 -> 1
        | y -> y

    let expected =  startIndex + 2 + (f notifications.Length) + (f imports.Length)
    let top = UI.NextLineIndex()
    if top < expected then
        let diff = expected - top - 1
        [0..diff] |> Seq.iter (fun x -> printfn "")

    WriteAt (startIndex) [YELLOW; "Notifications:"]

    let mutable offset = startIndex 

    if notifications.IsEmpty then
        offset <- offset + 1
        WriteAt (offset) [DARKRED; " No active notifications"]

    notifications 
    |> Seq.iteri 
        (fun i ns -> 
            offset <- offset + 1
            WriteAt (offset) [DEFAULT; " ID:"; DARKYELLOW; ns.NotificationId; 
                                        DEFAULT; " Status:"; DARKYELLOW; ns.Status; 
                                        DEFAULT; " Instance:"; DARKYELLOW; ns.RendererInstance; 
                                        DEFAULT; " Total/Started/Rendered:"; DARKYELLOW; ns.Total; DEFAULT; "/"; DARKYELLOW; ns.Started; DEFAULT; "/"; DARKYELLOW; ns.Finished])

    offset <- offset + 1
    WriteAt (offset)[YELLOW; "Imports:"]

    if imports.IsEmpty then
        offset <- offset + 1
        WriteAt (offset) [DARKRED; " No active imports"]

    imports
    |> Seq.iteri
        (fun i imp ->
            offset <- offset + 1
            WriteAt (offset) [DEFAULT; " Gimp:"; DARKYELLOW; imp.Instance;
                                    DEFAULT; " Name:"; DARKYELLOW; imp.Name;
                                    DEFAULT; " Type:"; DARKYELLOW; imp.Type;
                                    DEFAULT; " Date:"; DARKYELLOW; imp.Date])


                                        
    let top = UI.NextLineIndex()
    if top > expected then
        [expected..top] |> Seq.iter (fun x -> WriteAt x [""]; System.Console.CursorTop <- System.Console.CursorTop - 1)



let MonitorActivity () =
    WriteLine [DARKYELLOW; "Monitoring database activity. Ctrl-C to exit."]
    let debugIndex = UI.NextLineIndex()
    printfn "debugout"
    let startIndex = UI.NextLineIndex() 
    printfn ""

//    let offset = 2 // title & debug line

//    printfn ""
//    printfn ""
//    printfn ""

    let printer = PrintActivity startIndex

    let sql = @"select sn.NotificationId, sn.schd_sent, bn.RenderBotID, r.Total, r.Started, r.Rendered
                from tbl_schedule_notifications sn
                    left join tbl_batch_notifications bn on sn.notificationID = bn.NotificationID
                    left join 
                        (select bnr.BatchNotificationId, count(*) as Total, 
                            count(r.PasngrStartDate) as Started, count(r.PasngrRenderDate) as Rendered
                        from tbl_batch_notifications_recipients bnr  
                            left join tbl_recipient r on bnr.RecipientID = r.recipientID
                            group by bnr.BatchNotificationID) as r on bn.ID = r.BatchNotificationID                
                where sn.schd_sent <> 5 and sn.schd_sent <> 2
                order by sn.notificationId ; "
                +
                @"select GDSCode, CreationDate, RequestingAppName, RequestType 
                  from gdsInteractionView 
                  where status <> 'complete' 
                  order by CreationDate"

    use command = getCommand sql
//    while true do
//        let loaded = LoadActivity command
//        printer loaded
//        System.Threading.Thread.Sleep(500)

    let rec loop maxLines count =
        let notifications, imports = LoadActivity command
        WriteAt debugIndex ["#"; count; " MaxLines:"; maxLines; " Loaded:"; notifications.Length; " Diff:"; (notifications.Length - maxLines); " StartIndex:"; startIndex; " Next:"; UI.NextLineIndex()]
//        match notifications.Length - maxLines with
//        | needed when needed > offset -> [offset..needed] |> Seq.iter (fun x -> WriteLine ["need "; x])
//        | excess when excess < 0 && notifications.Length > 0 ->  [0..(0-excess)] |> Seq.iter (fun i -> WriteAt ((maxLines - i) + offset) [DARKRED; " -- killing line "; i]) //WriteAt maxLines ["excess is "; excess] //
//        | _ -> ()
        printer notifications imports
        System.Threading.Thread.Sleep(500)
        let newMax = 
            match notifications.Length - maxLines with
            | bigger when bigger > 0 -> notifications.Length
            | less -> maxLines

        if count < 1000 then
            loop newMax (count + 1)

    loop 0 0


    

let ResendNotification id = 
    WriteLine [RED; "NOTE: This command purges related records in batch_notifications & batch_notifications_recipients."]

    let sql = "delete r from tbl_batch_notifications_recipients r inner join tbl_batch_notifications b on r.BatchNotificationID = b.ID where b.NotificationID = " + id.ToString()
                + "\ndelete tbl_batch_notifications where NotificationID = " + id.ToString()
                + "\nupdate tbl_schedule_notifications set schd_sent = 0 where notificationID = " + id.ToString()

    use command = getCommand sql
    command.ExecuteNonQuery() |> ignore
    WriteLine [DEFAULT; "Triggered resend of notification "; id; " at "; System.DateTime.Now.ToShortTimeString()]


let RecentNotifications() =
    let data = loadRecentNotifications()
    let mutable exit = false
    let mutable startIndex = 0

    WriteLine [DARKYELLOW; "Last 10 notifications:"]
    while exit = false do
        data |> Seq.skip startIndex |> Seq.truncate 10 |> Seq.iter writeNotification

        startIndex <- startIndex + 10
        printf "enter an id, q to exit, enter to list more: "
        let id = System.Console.ReadLine()
        let x,y = System.Int32.TryParse id

        match id, x with
        | _, true -> 
            if x && data |> Seq.exists (fun d -> d.Id = y ) then
                exit <- true
                ResendNotification y
            else
                startIndex <- startIndex - 10
                WriteLine [RED; "Invalid Notification Id:"; id]
        | "", _ -> 
            if startIndex >= data.Length then
                exit <- true
            ()
        | _, _ -> exit <- true


