using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace KINO_MO_UI
{
    public class Core
    {
        private ClassSock Socket;
        private int CountDevice = 0;
        private int SelectedDevice = -1;
        private bool FlagSetMode = true;
        private float AvalibleSpase = 0;
        private int THRESHOLD = 100;
        private int SendComand = 0;
        private string PlayListNames = "";
        private PlayList PlayListDev = new PlayList();
        private int ActivePlaylist = -1;
        public Device DeviceList = new Device();
        //private Form2 EditForm = new Form2();
        private string UI_Name = "UI_KINOMO";
        private string Signature = "mfyTnVlp8Gj6kT61+ksdfsfbrgHVu3d640YNetfvo9WkI12WvyjnSviI3jjTnY0LPmC90z";
        private FFMPEGConvert Converter;
        public static string DeviceName = "";
        public bool flagFormClosed = false;
        public static bool FlagErrorConnection = false;
        //public static FormAuthentication AuthenticationForm = new FormAuthentication();
        public bool FlagReciveIP = false;


        public void Connection()
        {
            DeviceList.ReadDeviceName();
            string CheckServerCore = "";
            Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
            //проверяем наличие сервера
            CheckServerCore = Socket.CheckAnotherCore();
            try
            {
                if(IsProcessOpen("KMcontroller"))
                {
                    this.FindAndKillProcess("KMcontroller");
                    Thread.Sleep(1000);
                }
                Process proc = new Process();
                proc.StartInfo.FileName = @"KMcontroller.exe";
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                Thread.Sleep(100);
                Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
            }
            catch
            {
                Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
            }
            if (CheckServerCore == "NO")
            {
                Socket.OnReceiveMessage += new OnReceive(OnReceivedData);
                //Socket.OnConnectErrorMessage += new OnConnectError(OnConnectError);

                Socket.DeviceName = UI_Name;
                Socket.ErrorConnect = 0;
                string myHost = Dns.GetHostName();
                string myIP = GetIPAddress(myHost);
                Socket.Connect(myIP, true);
            }
          
            if (Directory.Exists(FFMPEGConvert.PathConvert))
            {
                //Directory.Delete(FFMPEGConvert.PathConvert, true);
            }
            Thread.Sleep(1000);
            Directory.CreateDirectory(FFMPEGConvert.PathConvert);
        }


        public void FormAuthentication()
        {
            Cryptography Crypto = new Cryptography();
            string Password = "PassF0rAuthentication";
            string NameConfig = "configA";
            string userName = "prabash@seentechnology.com.au";
            string userPass = "abcd1234";

            //string PasswordChar = '*';
            try
            {
                //string[] StrAuthentication = File.ReadAllLines(NameConfig);
                string[] StrAuthentication = new string[2];
                StrAuthentication[0] = Crypto.Encrypt(userName, Password);
                StrAuthentication[1] = Crypto.Encrypt(userPass, Password);

                File.WriteAllLines(NameConfig, StrAuthentication);
                System.Console.ForegroundColor = ConsoleColor.Green;

                System.Console.WriteLine("Login Success");
            }
            catch(Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(ex.Message);
            }
        }

        public void getDeviceList()
        {
            Thread.Sleep(100);
            string myHost = Dns.GetHostName();
            string myIP = GetIPAddress(myHost);
            Socket.Connect(myIP, true);
            //Socket.Connect();
            Socket.SendSocketCommand(ComandSend.GET_DEVICE_LIST, null, null);
            Socket.OnReceiveMessage += new OnReceive(OnReceivedData);
           // Socket.OnConnectErrorMessage += new OnConnectError(OnConnectError);
            Thread.Sleep(100);
        }

        public void getRemoteIpAddress()
        {
            string checkAnotherCore = Socket.CheckAnotherCore();
            if (checkAnotherCore.Equals("NO"))
            {
                string myHost = Dns.GetHostName();
                string myIP = GetIPAddress(myHost);
                Socket.Connect(myIP, true);
            }
            ArrayList DevList = new ArrayList();
            Socket.SendSocketCommand(ComandSend.GET_DEVICE_IP, DevList, null);
            Socket.OnReceiveMessage += new OnReceive(OnReceivedData);
            //Socket.OnConnectErrorMessage += new OnConnectError(OnConnectError);
            Thread.Sleep(100);
        }

        public void restConnnection()
        {
            Socket.SendSocketCommand(ComandSend.RESET_CONNECTION, null, null);
        }

        public void receiveMessage()
        {
            Socket.OnReceiveMessage += new OnReceive(OnReceivedData);
            //Socket.OnConnectErrorMessage += new OnConnectError(OnConnectError);
            this.Socket.DeviceName = this.UI_Name;
            this.Socket.ErrorConnect = (byte)0;
        }
        
        public static string GetIPAddress(string hostname)
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }
        static public bool IsProcessOpen(string name)
        {
            return Process.GetProcesses().Any(clsProcess => clsProcess.ProcessName.Contains(name));
        }
        private bool FindAndKillProcess(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName == name)
                {
                    clsProcess.Kill();
                    return true;
                }
            }

            return false;
        }

        private void OnConnectError(string Message)
        {
            //делаем неактивными все устройства
            //for (int i = 0; i < dataGridView1.RowCount; i++)
                //dataGridView1.Rows[i].Cells[0].Style.BackColor = Color.Gray;
           // Devicelabel.ForeColor = Color.Gray;

            //RESET_CONNECTION.ImageIndex = 1;
            //panelCore.BackColor = Color.Red;
            //panelCRM.BackColor = Color.Red;
            if (!FlagReciveIP)
                try
                {

                    if (IsProcessOpen("KMcontroller"))
                    {
                        FindAndKillProcess("KMcontroller");
                        Thread.Sleep(500);
                    }
                    Process proc = new Process();
                    proc.StartInfo.FileName = @"KMcontroller.exe";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    if (!flagFormClosed) proc.Start();
                }
                catch { }
        }


        public void OnReceivedData(RequestAnswer Message)
        {
            string strFile = "";
            string[] FileName;
            string[] strPlaylist;

            switch(Message.code)
            {
                case ComandReceive.CORE_OK:
                    try
                    {
                        string[] StrOK = Encoding.UTF8.GetString(Message.Data).Split(':');
                        if(StrOK[0].Equals("OK"))
                        {
                            switch(StrOK[1])
                            {
                                case "AUTHENTICATION":
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("License OK");
                                    break;
                                case "CONNECTION":
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("License OK");
                                    break;
                            }
                        }

                    }
                    catch(Exception ex)
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case ComandReceive.CORE_ERROR:
                    //FormCollection fc;
                    //bool formFlag;
                    try
                    {
                        string[] StrError = Encoding.UTF8.GetString(Message.Data).Split(':');
                        if (StrError[0] == "ERROR")
                        {
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            switch (StrError[1])
                            {
                                case "AUTHENTICATION":
                                    if (StrError[2] == "1")
                                        Console.WriteLine("Check the login and password please"); 
                                    if (StrError[2] == "2")
                                        Console.WriteLine("Server error. Check the connection\n or contact server administrator");
                                    if (StrError[2] == "3")
                                        Console.WriteLine("Authentication error. Contact server\n administrator please"); 
                                    if (StrError[2] == "4")
                                        Console.WriteLine("License Error");
                                    if (StrError[2] == "5")
                                        Console.WriteLine("License OK");
                                    break;
                                case "CONNECTION":
                                    Console.WriteLine("CONNECTION Passing to FormAuthentication");
                                    FormAuthentication();
                                    getDeviceList();
                                    break;
                                case "LICENSE":
                                    Console.WriteLine(" LICENSE Passing to FormAuthentication");
                                    FormAuthentication();
                                    getDeviceList();
                                   // Console.WriteLine("License Error");
                                    break;
                            }

                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case ComandReceive.PLAYLIST_SEND:
                    string[] StrPlay = Encoding.UTF8.GetString(Message.Data).Split('\r');
                    StrPlay = StrPlay[0].Split('\n').ToArray();
                    string NamePlaylist = StrPlay[0];
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    foreach (string a in StrPlay)
                        Console.WriteLine(a.ToString());

                    /* dataGridView3.Invoke((MethodInvoker)(delegate ()
                     {
                         dataGridView3.Rows.Clear();
                         for (int i = 0; i < StrPlay.Length - 1; i++)
                         {
                             string[] StrForSave = StrPlay[i + 1].Split(';').ToArray();
                             dataGridView3.Rows.Add();
                             dataGridView3.Rows[i].Cells[0].ReadOnly = true;
                             dataGridView3.Rows[i].Cells[0].Value = StrForSave[0];
                             dataGridView3.Rows[i].Cells[1].Value = Convert.ToInt32(StrForSave[1]);
                             if (checkBox5.Checked) dataGridView3.Rows[i].Cells[2].Value = true;

                         }
                         dataGridView3.ClearSelection();
                     }));*/
                    break;
                case ComandReceive.CLIENT_CONNECTED:
                    Thread.Sleep(100);
                    string myHost = Dns.GetHostName();
                    string myIP = GetIPAddress(myHost);
                    Socket.Connect(myIP, true);
                    Socket.SendSocketCommand(ComandSend.CONNECTION_CRM, null, "");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("CLIENT_CONNECTED");

                    break;
                case ComandReceive.DEVICE_IP:
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("IP = " + Encoding.UTF8.GetString(Message.Data));
                    break;
                case ComandReceive.DEVICE_LIST_SEND:
                    if (SendComand == ComandSend.FILE_SEND_CANCEL)
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("DEVICE_LIST_SEND");
                        SendComand = 0;
                    }

                    int DevConnect = 0;
                    int j = 0;
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Gray;

                    foreach (string device in Message.List)
                    {
                        string NameDev = "";
                        bool flagAddToFile = true;
                        int countDev = 0;
                        //Проверяем есть ли такое в списке
                        foreach (string DevID in DeviceList.DevID)
                        {
                            if (DevID == device)
                            {
                                flagAddToFile = false;
                                NameDev = DeviceList.DevName[countDev].ToString();
                                break;
                            }
                            countDev++;
                        }
                        if (flagAddToFile)
                        {
                            //добавляем в файл
                            NameDev = Message.List[j].ToString();
                            StreamWriter myfile = new StreamWriter(DeviceList.FileDev, true);
                            myfile.WriteLine(NameDev + ";" + NameDev);
                            myfile.Close();
                            //добавляем список
                            DeviceList.DevID.Add(NameDev);
                            DeviceList.DevName.Add(NameDev);
                        }


                        bool flagAdd = true;
                        /*  for (int i = 0; i < dataGridView1.RowCount; i++)
                          {
                              //string Name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                              if (NameDev == Name) { flagAdd = false; DevConnect = i; break; }
                          }*/

                        if ((flagAdd) && (NameDev != ""))
                        {
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("need more work");

                            /*  dataGridView1.Rows.Add();
                              dataGridView1.Rows[CountDevice].Cells[0].ReadOnly = true;
                              dataGridView1.Rows[CountDevice].Cells[0].Value = NameDev;
                              dataGridView1.Rows[CountDevice].Cells[0].Style.BackColor = Color.LimeGreen;
                              if (checkBox1.Checked) dataGridView1.Rows[CountDevice].Cells[1].Value = true;
                              dataGridView1.Rows[CountDevice].Cells[2].Value = 0;
                              CountDevice++;
                              dataGridView1.ClearSelection();*/
                        }
                        else
                        {
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("need more work");
                            /*  dataGridView1.Rows[DevConnect].Visible = true;
                              dataGridView1.Rows[DevConnect].Cells[0].Style.BackColor = Color.LimeGreen;
                              dataGridView1.Rows[DevConnect].Cells[1].ReadOnly = false;
                              if (checkBox1.Checked) dataGridView1.Rows[DevConnect].Cells[1].Value = true;
                              if (DevConnect == SelectedDevice) Devicelabel.ForeColor = Color.LimeGreen;
                              */
                        }
                        j++;
                    }

                    break;
            }
        }



    }
}
