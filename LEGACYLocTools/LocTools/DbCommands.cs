using System;
using System.Data.SqlClient;

namespace LocTools
{
    public class DbCommands
    {
        private readonly Writer output;
        private readonly Configuration config;

        public DbCommands(Writer output, Configuration config)
        {
            this.output = output;
            this.config = config;
        }

        private SqlCommand GetCommand(string sql)
        {
            var db =
                new System.Data.SqlClient.SqlConnection("Data Source=localhost;Initial catalog=" + config.Client +
                                                        "-LOC-PASNGR;Integrated Security=SSPI");
            db.Open();
            var command = db.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        private void CloseCommand(SqlCommand command)
        {
            var db = command.Connection;
            db.Close();
        }

        // Move SQL to config so it can be easily tweaked without recompilation?
        // And add arbitrary named SQL tasks to run?

        public void Resend(int? notificationId)
        {
            // if no arg, list some, prompt for number, or show next X or abort, rather than just the last 10 and that's it.
            // when listing recent notifications try to include the number of recipients & datasource id/name
            if (notificationId.HasValue)
            {
                output.Line(
                    "NOTE: This command purges related records in batch_notifications & batch_notifications_recipients.", ConsoleColor.Red);

                var sql = "delete r from tbl_batch_notifications_recipients r inner join tbl_batch_notifications b on r.BatchNotificationID = b.ID where b.NotificationID = " + notificationId.Value;
                sql += "delete tbl_batch_notifications where NotificationID = " + notificationId.Value;
                sql += "update tbl_schedule_notifications set schd_sent = 0 where notificationID = " + notificationId.Value;

                var command = GetCommand(sql);
                command.ExecuteNonQuery();
                CloseCommand(command);

                output.Line("Triggered resend of notification " + notificationId.Value + " at " + DateTime.Now.ToShortTimeString());
            }
            else
            {
                const int x = 10;
                output.Line("Last " + x + " notifications:");
                var sql = "select top " + x +
                          " notification_desc, notificationId from tbl_schedule_notifications order by notificationid desc";

//select top 10 n.notification_desc, n.notificationId, COUNT(*) as recipientCount
//from tbl_schedule_notifications n
//    inner
//join tbl_batch_notifications b on n.notificationID = b.NotificationID
//    inner
//join tbl_batch_notifications_recipients r on b.ID = r.BatchNotificationID
//group by n.notificationID, n.notification_desc
//order by notificationid desc

                var command = GetCommand(sql);
                var a = command.ExecuteReader();
                while (a.Read())
                {
                    int id = a.GetInt32(1);
                    string name = a.GetString(0);
                    output.Line(id.ToString().PadLeft(5) + ": " + name);
                }
                a.Close();
                CloseCommand(command);
            }
        }
    }
}