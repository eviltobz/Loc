using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace LocTools
{
    public class Writer
    {
        private int indentLevel = 0;
        public void Indent()
        {
            indentLevel++;
        }

        public void Outdent()
        {
            indentLevel--;
            if (indentLevel < 0) indentLevel = 0;
        }
        private void DoWrite(string message, ConsoleColor colour)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = old;
        }

        public void Line(string message, ConsoleColor? colour = null)
        {
            if (colour == null)
                colour = Console.ForegroundColor;

            var spacer = "";
            for (int i = 0; i < indentLevel; i++)
                spacer += "  ";
            DoWrite(spacer + message, colour.Value);
        }

        public void WriteAllClients(IEnumerable<string> clients, string currentClient)
        {
            Line("Current active client is \"" + currentClient + "\"");
            Indent();
            foreach (var client in clients)
            {
                Line(client);
            }
            Outdent();
        }


        public void Service(ServiceController service)
        {
            ConsoleColor? lineColour = null;
            if (service.Status == ServiceControllerStatus.Running)
                lineColour = ConsoleColor.Green;
            if (service.Status == ServiceControllerStatus.Stopped)
                lineColour = ConsoleColor.DarkGray;

            Line(service.Status + " - " + service.ServiceName, lineColour);
        }
    }
}