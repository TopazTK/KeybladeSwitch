using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeybladeSwitch
{
    public static class Terminal
    {
        static string _logFileName = "";

        public static void Log(string Input, byte Type)
        {
            try
            {
                var _formatStr = "{0}{1}: {2}";

                var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
                var _timeStr = "[" + DateTime.Now.ToString("hh:mm:ss") + "] ";

                var _session = 1;

                var _typeStr = "";

                switch (Type)
                {
                    case 0:
                        _typeStr = "MESSAGE";
                        break;

                    case 1:
                        _typeStr = "WARNING";
                        break;

                    case 2:
                        _typeStr = "ERROR";
                        break;
                }

                Console.Write(_timeStr);
                Console.ForegroundColor = Type == 0x00 ? ConsoleColor.Green : (Type == 0x01 ? ConsoleColor.Yellow : ConsoleColor.Red);
                Console.Write(_typeStr + ": ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Input);
            }

            catch (Exception) { }
        }

        public static void Log(Exception Input)
        {
            try
            {
                var _formatStr = "[{0}] {1}";

                var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
                var _timeStr = DateTime.Now.ToString("hh:mm:ss");


                var _exString = Input.ToString().Replace("   ", "").Replace(System.Environment.NewLine, " ");

                Console.Write("[" + _timeStr + "] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("EXCEPTION: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(_exString);
            }

            catch (Exception) { }
        }
    }
}
