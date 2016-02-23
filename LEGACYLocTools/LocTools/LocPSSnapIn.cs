using System.ComponentModel;
using System.Management.Automation;

namespace LocTools
{
    [RunInstaller(true)]
    public class LocPSSnapIn : PSSnapIn
    {
        public override string Name
        {
            get { return "LocTools"; }
        }

        public override string Vendor
        {
            get { return "15below"; }
        }

        public override string Description
        {
            get { return "Powershell commands for interacting with LOC services"; }
        }
    }
}