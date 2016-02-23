using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Text;
using System.Threading.Tasks;

namespace LocTools
{

    public static class ParamSet
    {
        public const string Help = "Help";
        public const string ListAll = "ListAll";
        public const string StopAll = "StopAll";
        public const string ClientDetails = "ClientDetails";
        public const string ActiveClient = "ActiveClient";
        public const string Start = "Start";
        public const string Stop = "Stop";
        public const string Resend = "Resend";
    }

    [Cmdlet(VerbsOther.Use, "Loc", DefaultParameterSetName=ParamSet.Help)]
    public class Hax : PSCmdlet
    {
        private readonly Writer output = new Writer();
        private readonly ServiceCommands services;
        private readonly Configuration config = new Configuration();
        private DbCommands db;

        public Hax()
        {
            services = new ServiceCommands(output, new Services(), config);
            db = new DbCommands(output, config);
        }

        [Parameter(ParameterSetName=ParamSet.Help), Alias("h")]
        public SwitchParameter Help { get; set; }

        [Parameter(ParameterSetName=ParamSet.ListAll, Position=0), Alias("la")]
        public SwitchParameter ListAll { get; set; }

        [Parameter(ParameterSetName=ParamSet.StopAll, Position=0), Alias("sa")]
        public SwitchParameter StopAll { get; set; }

        [Parameter(ParameterSetName=ParamSet.ActiveClient, Position=0), Alias("c", "ac")]
        public SwitchParameter ActiveClient { get; set; }
        [Parameter(ParameterSetName=ParamSet.ActiveClient, Position= 1, Mandatory=false)]
        public string ClientName { get; set; }

        // Commands below this line require an active client setting
        [Parameter(ParameterSetName=ParamSet.ClientDetails, Position=0), Alias("d", "cd")]
        public SwitchParameter ClientDetails { get; set; }

        [Parameter(ParameterSetName=ParamSet.Stop, Position=0)]
        public SwitchParameter Stop { get; set; }

        // Add restart option (with similar params?) add some intelligence, like restart renderers - restarts thems as is running, leaves thems that aint.
        // and an extra flag to purge logfiles? may be a nuisance to "properly" get the file location
        [Parameter(ParameterSetName=ParamSet.Start, Position=0), Alias("s")]
        public SwitchParameter Start { get; set; }
        [Parameter(ParameterSetName=ParamSet.Start, Mandatory=false), Alias("ar")]
        public SwitchParameter AllRenderers { get; set; }
        [Parameter(ParameterSetName=ParamSet.Start, Mandatory=false), Alias("br", "r")]
        public SwitchParameter BasicRenderer { get; set; }
        [Parameter(ParameterSetName=ParamSet.Start, Mandatory=false), Alias("ag")]
        public SwitchParameter AllGimps { get; set; }
        [Parameter(ParameterSetName=ParamSet.Start, Mandatory=false), Alias("bg", "g")]
        public SwitchParameter BasicGimp { get; set; }
        [Parameter(ParameterSetName=ParamSet.Start, Mandatory=false), Alias("m")]
        public SwitchParameter Misc { get; set; }


        [Parameter(ParameterSetName=ParamSet.Resend, Position=0), Alias("re", "send")]
        public SwitchParameter Resend { get; set; }
        [Parameter(ParameterSetName=ParamSet.Resend, Position= 1, Mandatory=false)]
        public int? NotificationId { get; set; }

        private void PrintParams()
        {
            output.Line("Parameters:");
            output.Indent();
            output.Line("Help:'" + Help + "'");
            output.Line("ListAll:'" + ListAll + "'");
            output.Line("StopAll:'" + StopAll + "'");
            output.Line("ActiveClient:'" + ActiveClient + "', ClientName:'" + ClientName + "'");
            output.Line("ClientDetails:'" + ClientDetails + "'");
            output.Line("Start:'" + Start + "', AllRenderers:" + AllRenderers + ", BasicRenderer:" + BasicRenderer);
            output.Outdent();
            output.Line("---------------", ConsoleColor.Cyan);

        }

        protected override void ProcessRecord()
        {
            //PrintParams();

            switch (ParameterSetName)
            {
                case ParamSet.ListAll:
                    services.ListAll();
                    break;
                case ParamSet.StopAll:
                    services.StopAll();
                    break;
                case ParamSet.ActiveClient:
                    services.SetClient(ClientName);
                    break;
                case ParamSet.Help:
                    services.Help(this);
                    break;

                default:
                    // Handle commands that need an active client
                    if (string.IsNullOrWhiteSpace(config.Client))
                        output.Line("The selected command requires an active client to be assigned.");
                    else
                    {
                        switch (ParameterSetName)
                        {
                            case ParamSet.ClientDetails:
                                services.ClientDetails();
                                break;
                            case ParamSet.Stop:
                                services.Stop();
                                break;
                            case ParamSet.Start:
                                services.Start(Misc, BasicRenderer, AllRenderers, BasicGimp, AllGimps);
                                break;
                            case ParamSet.Resend:
                                db.Resend(NotificationId);
                                break;
                        }
                    }
                    break;
            }
        }
    }


}
