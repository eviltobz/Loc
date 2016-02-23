using System;

namespace LocTools
{
    public class Configuration
    {
        public string Client
        {
            get { return Environment.GetEnvironmentVariable("LocToolsActiveClient"); }
            set { System.Environment.SetEnvironmentVariable("LocToolsActiveClient", value); }
        }
    }
}