module Database

open System.Data.SqlClient
open UI


let private getCommand sql =
    let conn = new SqlConnection ("Data Source=" + Config.get.LocAddress + ";Initial catalog=" + Config.get.CurrentClient + "-LOC-PASNGR;Integrated Security=SSPI")
    conn.Open()
    let command = conn.CreateCommand()
    command.CommandText <- sql 
    command

//
// Public API Bits
//
let RecentNotifications() =
    let max = 10
    WriteLine [_DARKYELLOW ("Last " + max.ToString() + " notifications:")]

    let sql = "select top " + max.ToString() + 
                          @"    sn.notificationId,sn.notification_desc, template_desc, data_source_name, recipientCount
                                from tbl_schedule_notifications sn
                                    inner join tbl_queue_notifications qn on schd_batchID = 'QID' + CAST(qn.notificationID as varchar(10))
                                    inner join notificationSource ns on qn.notificationID = ns.notificationId
                                    inner join tbl_data_source ds on ns.dataSourceId = ds.data_sourceID
                                    inner join tbl_texts_template t on qn.nt1_templateID = t.templateID 
                                    inner join (
										select notificationId, COUNT(*) as recipientCount
										from tbl_batch_notifications bn inner join tbl_batch_notifications_recipients bnr on bn.ID = bnr.BatchNotificationID
										group by notificationId
										) as r on qn.notificationID = r.NotificationID
                                order by sn.notificationid desc"
    use command = getCommand sql
    use reader = command.ExecuteReader()
    while reader.Read() do
        let id = reader.GetInt32 0
        let name = reader.GetString 1
        let template = reader.GetString 2
        let datasource = reader.GetString 3
        let count = reader.GetInt32 4

        WriteLine [_DARKGREEN (id.ToString() + ":"); _DEFAULT name; _DARKGREY " ("; 
            _DEFAULT "#:"; _DARKYELLOW (count.ToString()); 
            _DEFAULT " / T:"; _DARKGREY template; 
            _DEFAULT " / DS:"; _DARKGREY (datasource + ")")]


let ResendNotification id = 
    WriteLine [_RED "NOTE: This command purges related records in batch_notifications & batch_notifications_recipients."]

    let sql = "delete r from tbl_batch_notifications_recipients r inner join tbl_batch_notifications b on r.BatchNotificationID = b.ID where b.NotificationID = " + id.ToString()
                + "\ndelete tbl_batch_notifications where NotificationID = " + id.ToString()
                + "\nupdate tbl_schedule_notifications set schd_sent = 0 where notificationID = " + id.ToString()

    use command = getCommand sql
    command.ExecuteNonQuery() |> ignore

    WriteLine [_DEFAULT ("Triggered resend of notification " + id.ToString() + " at " + System.DateTime.Now.ToShortTimeString())]


//                output.Line(
//                    "NOTE: This command purges related records in batch_notifications & batch_notifications_recipients.", ConsoleColor.Red);
//
//                var sql = "delete r from tbl_batch_notifications_recipients r inner join tbl_batch_notifications b on r.BatchNotificationID = b.ID where b.NotificationID = " + notificationId.Value;
//                sql += "delete tbl_batch_notifications where NotificationID = " + notificationId.Value;
//                sql += "update tbl_schedule_notifications set schd_sent = 0 where notificationID = " + notificationId.Value;
//
//                var command = GetCommand(sql);
//                command.ExecuteNonQuery();
//                CloseCommand(command);
//
//                output.Line("Triggered resend of notification " + notificationId.Value + " at " + DateTime.Now.ToShortTimeString());
//        

//
// const int x = 10;
//                output.Line("Last " + x + " notifications:");
//                var sql = "select top " + x +
//                          " notification_desc, notificationId from tbl_schedule_notifications order by notificationid desc";
//
////select top 10 n.notification_desc, n.notificationId, COUNT(*) as recipientCount
////from tbl_schedule_notifications n
////    inner
////join tbl_batch_notifications b on n.notificationID = b.NotificationID
////    inner
////join tbl_batch_notifications_recipients r on b.ID = r.BatchNotificationID
////group by n.notificationID, n.notification_desc
////order by notificationid desc
//
//                var command = GetCommand(sql);
//                var a = command.ExecuteReader();
//                while (a.Read())
//                {
//                    int id = a.GetInt32(1);
//                    string name = a.GetString(0);
//                    output.Line(id.ToString().PadLeft(5) + ": " + name);
//                }
//                a.Close();
//                CloseCommand(command);