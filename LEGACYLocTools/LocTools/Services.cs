using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace LocTools
{
    public class Services
    {
        public IReadOnlyCollection<string> GetAllClients()
        {
            var troller = new System.ServiceProcess.ServiceController();
            troller.MachineName = "loc";

            ServiceController[] a = System.ServiceProcess.ServiceController.GetServices();
            var b = a.Where(s => s.ServiceName.IndexOf('$') > 0 && s.ServiceName.IndexOf("-LOC", StringComparison.InvariantCultureIgnoreCase) > s.ServiceName.IndexOf('$'));
            var clients = b.Select(s =>
            {
                var name = s.ServiceName;
                int instanceStart = name.IndexOf('$');
                int end = name.IndexOf("-LOC", instanceStart, StringComparison.InvariantCultureIgnoreCase);
                int start = end;
                while (name[start - 1] != '.' && name[start - 1] != '$')
                    start--;
                var retval = name.Substring(start, end - start);
                return retval.ToUpper();

            }).Distinct();
            return clients.ToArray();
        }

    // This code seems incapable of differentiating 2 & 3 letter codes properly
    // Set client to LX, and it also shows LXA
        public IEnumerable<ServiceController> GetClientServices(string client)
        {
            var a = System.ServiceProcess.ServiceController.GetServices();
            return a.Where(s => s.ServiceName.Contains(client));
        }

        public IEnumerable<ServiceController> GetAll15bServices()
        {
            var a = System.ServiceProcess.ServiceController.GetServices();
            var b = a.Where(s => s.ServiceName.IndexOf('$') > 0 && s.ServiceName.IndexOf("-LOC", StringComparison.InvariantCultureIgnoreCase) > s.ServiceName.IndexOf('$'));
            return b;
        }
    }
}