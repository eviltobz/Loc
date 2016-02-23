//using System;
//using System.Collections.Generic;
//using System.Management.Automation;

//namespace LocTools
//{
//    public class HelpCommand : Command
//    {
//        private readonly IEnumerable<string> _commandList;

//        public HelpCommand(CommandSelector context, IEnumerable<string> commandList) : base(context)
//        {
//            _commandList = commandList;
//        }

//        public override void Process()
//        {
//            if(!string.IsNullOrEmpty(Context.CommandName))
//                Write("Command not found:" + Context.CommandName);

//            Write("Valid commands are: ");
//            foreach (var command in _commandList)
//            {
//                Write("  " + command);
//            }
//        }
//        [Parameter(Position=1)]
//        public string NullParam1 { get; set; }
//        [Parameter(Position=2)]
//        public string NullParam2 { get; set; }
//        [Parameter(Position=3)]
//        public string NullParam3 { get; set; }
//        [Parameter(Position=4)]
//        public string NullParam4 { get; set; }
//        [Parameter(Position=5)]
//        public string NullParam5 { get; set; }
//        [Parameter(Position=6)]
//        public string NullParam6 { get; set; }
//        [Parameter(Position=7)]
//        public string NullParam7 { get; set; }
//        [Parameter(Position=8)]
//        public string NullParam8 { get; set; }
//        [Parameter(Position=9)]
//        public string NullParam9 { get; set; }
//    }
//}