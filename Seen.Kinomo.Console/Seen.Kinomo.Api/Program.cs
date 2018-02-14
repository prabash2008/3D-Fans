using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace KINO_MO_UI
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("Started...");
            Core co = new Core();
            co.Connection();
            co.Connection();
            Console.Read();


        }
    }
}
