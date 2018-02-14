using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using KINO_MO_UI;

namespace Seen.Kinomo.Console
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //System.Console.BackgroundColor = ConsoleColor.Blue;
            System.Console.ForegroundColor = ConsoleColor.Red;

            System.Console.WriteLine("CLIENT_CONNECTED");
            Core co = new Core();
            co.Connection();
            co.FormAuthentication();
            //co.Connection();
            //co.getDeviceList();
            co.getRemoteIpAddress();
            co.receiveMessage();
            //co.getDeviceList();
            System.Console.Read();


        }
    }
}
