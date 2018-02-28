using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace undeaf
{
    public class undeafLogger
    {
        public class MessageType
        {
            public string Name;
            public int Level;
            public string Code;
            public ConsoleColor TagColor;
            public ConsoleColor MessageColor;

            public MessageType (string name, int level, string code, ConsoleColor tagColor, ConsoleColor messageColor)
            {
                Name = name;
                Level = level;
                Code = code;
                TagColor = tagColor;
                MessageColor = messageColor;
            }
        }

        public undeafStatus Status = new undeafStatus();
        public bool PrintToScreen = true;
        public int DisplayMessageLevel = 10;
        public MessageType DefaultMessageType = null;
        public List<MessageType> MessageTypes = new List<MessageType>
        {
            new MessageType("DEBUG", 0, "DBG", ConsoleColor.Magenta, ConsoleColor.Gray),
            new MessageType("INFO", 10, "INF", ConsoleColor.Cyan, ConsoleColor.Gray),
            new MessageType("HELP", 11, "HLP", ConsoleColor.Green, ConsoleColor.Gray),
            new MessageType("OKAY", 40, " OK", ConsoleColor.Green, ConsoleColor.Gray),
            new MessageType("WARN", 70, "WRN", ConsoleColor.Yellow, ConsoleColor.Gray),
            new MessageType("ERROR", 99, "ERR", ConsoleColor.Red, ConsoleColor.Red)
        };
        public MessageType GetByName(string name)
        {
            foreach (MessageType mt in MessageTypes)
            {
                if (mt.Name.ToLower() == name.ToLower())
                {
                    return mt;
                }
            }
            return DefaultMessageType;
        }

        private DateTime appStart = DateTime.MinValue;
        private StreamWriter outFile = null;

        public undeafLogger()
        {
            appStart = DateTime.Now;
            DefaultMessageType = GetByName("INFO");
        }

        public undeafLogger(string logFileSpec)
        {
            appStart = DateTime.Now;
            outFile = new StreamWriter(logFileSpec, true, System.Text.Encoding.UTF8);
        }

        ~undeafLogger()
        {
            Log(GetByName("INFO"), "Program execution took: " + (DateTime.Now - appStart).TotalMilliseconds + "ms");
            if (Status.HasError)
            {
                Log(GetByName("ERROR"), "EXIT: Failure");
            }
            else
            {
                if (Status.HasWarning)
                {
                    Log(GetByName("WARN"), "EXIT: With Warnings");
                } else
                {
                    Log(GetByName("OKAY"), "EXIT: Success");
                }
            }

            if (outFile != null)
            {
                outFile.Close();
            }
        }

        public void Log(MessageType messageType, string message)
        {
            string dur = string.Format("{0:0000}", (DateTime.Now - appStart).TotalSeconds);
            string typeOut = string.Empty;

            //'MessageType cmt = messageTypes.Find(x => x.MessageLevel.Equals(messageType.MessageLevel));

            string chunk = string.Empty;
            string lineOut = string.Empty;

            if ((messageType.Level >= DisplayMessageLevel) && PrintToScreen)
            {
                chunk = "[" + dur + "][";
                lineOut += chunk;
                Console.ResetColor();
                Console.Write(chunk);

                chunk = messageType.Code;
                lineOut += chunk;
                Console.ForegroundColor = messageType.TagColor;
                Console.Write(chunk);

                chunk = "] ";
                lineOut += chunk;
                Console.ResetColor();
                Console.Write(chunk);

                chunk = message;
                lineOut += chunk;
                Console.ForegroundColor = messageType.MessageColor;
                Console.WriteLine(chunk);

                Console.ResetColor();
            }

            if (outFile != null)
            {
                outFile.WriteLineAsync(typeOut + lineOut);
            }
        }
    }
}
