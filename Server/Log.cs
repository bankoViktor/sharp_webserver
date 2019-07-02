using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{

    class Log
    {
        public static void WriteLine(string msg)
        {
            string time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            Console.WriteLine(time + "   " + msg);
        }

        public static void Write(string msg)
        {
            string time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            Console.Write(time + "   " + msg);
        }
        public static void WriteLineAdd(string msg)
        {
            Console.WriteLine(msg);
        }
    }

}
