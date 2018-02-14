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
    public partial class Form1 : Form
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
        public Form1()
        {
            InitializeComponent();
            DeviceList.ReadDeviceName();

            DataGridViewProgressColumn column = new DataGridViewProgressColumn();
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns.Add(column);
            dataGridView1.Columns[2].MinimumWidth = 220;
            dataGridView1.Columns[2].Width = 220;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.HeaderText = "        Progress       ";

            dataGridView1.Columns.Add("Column1", "    File    ");
            dataGridView1.Columns[3].MinimumWidth = 200;
            dataGridView1.Columns[3].Width = 200;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            string CheckServerCore = "";
            try
            {
                Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
                //проверяем наличие сервера
                CheckServerCore = Socket.CheckAnotherCore();

                if (CheckServerCore == "NO")
                {
                    if (IsProcessOpen("KMcontroller"))
                    {
                        FindAndKillProcess("KMcontroller");
                        Thread.Sleep(1000);
                    }
                    Process proc = new Process();
                    proc.StartInfo.FileName = @"KMcontroller.exe";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    Thread.Sleep(100);
                }
                else
                {
                    MessageBox.Show("Close Server = " + CheckServerCore + " please!");
                }

                // Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
            }
            catch
            {
                // Socket = new ClassSock(UI_Name, Signature, FlagReciveIP);
            }
            if (CheckServerCore == "NO")
            {
                Socket.OnReceiveMessage += new OnReceive(OnReceivedData);
                Socket.OnConnectErrorMessage += new OnConnectError(OnConnectError);

                Socket.DeviceName = UI_Name;
                Socket.ErrorConnect = 0;
                string myHost = Dns.GetHostName();
                string myIP = GetIPAddress(myHost);
                Socket.Connect(myIP, true);
            }

            //checkBox1.Left = dataGridView1.Columns[0].Width + checkBox1.Width / 3 + dataGridView1.Location.X;
            //checkBox3.Left = dataGridView2.Columns[0].Width + dataGridView2.Columns[1].Width + dataGridView2.Columns[2].Width + dataGridView2.Columns[3].Width + dataGridView2.Columns[4].Width + checkBox3.Width / 3 + dataGridView2.Location.X;
            //checkBox4.Left = dataGridView4.Columns[0].Width + checkBox4.Width / 3 + dataGridView4.Location.X;
            //checkBox5.Left = dataGridView3.Columns[0].Width + dataGridView3.Columns[1].Width / 2 + checkBox5.Width + dataGridView3.Location.X;
            //////////////////////////////////////////////////////            
            if (Directory.Exists(FFMPEGConvert.PathConvert))
            {
                Directory.Delete(FFMPEGConvert.PathConvert, true);
            }
            Thread.Sleep(1000);
            Directory.CreateDirectory(FFMPEGConvert.PathConvert);

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

        private ArrayList GetDevNames(bool AllDevice)
        {
            int CountDev = 0;
            string SearchName = "";
            ArrayList DevList = new ArrayList();
            if (AllDevice)
            {

                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Style.BackColor == Color.LimeGreen)
                    {
                        if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[1].Value))
                        {
                            SearchName = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            CountDev = 0;
                            foreach (string Name in DeviceList.DevName)
                            {
                                if (Name == SearchName)
                                {
                                    DevList.Add(DeviceList.DevID[CountDev]);
                                    break;
                                }
                                CountDev++;
                            }
                        }
                    }
                }

            }
            else if (SelectedDevice != -1)
                if (dataGridView1.Rows[SelectedDevice].Cells[0].Style.BackColor == Color.LimeGreen)
                {
                    SearchName = dataGridView1.Rows[SelectedDevice].Cells[0].Value.ToString();
                    CountDev = 0;
                    foreach (string Name in DeviceList.DevName)
                    {
                        if (Name == SearchName)
                        {
                            DevList.Add(DeviceList.DevID[CountDev]);
                            break;
                        }
                        CountDev++;
                    }
                }

            return DevList;
        }
        private string SearchID(string SearchName)
        {
            int CountDev = 0;
            foreach (string Name in DeviceList.DevName)
            {
                if (Name == SearchName)
                {
                    return DeviceList.DevID[CountDev].ToString();
                }
                CountDev++;
            }
            return "";
        }
        private string GetFileNames()
        {
            string FileName = "";
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[5].Value))
                    FileName = FileName + dataGridView1.Rows[i].Cells[0].Value.ToString() + "\n";
            }
            return FileName;
        }

        private string GetPlayListNames()
        {
            string FileName = "";
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[1].Value))
                    FileName = FileName + dataGridView1.Rows[i].Cells[0].Value.ToString() + "\n";
            }
            return FileName;
        }

        public bool AuthenticationCRM(string login, string password)
        {
            Socket.SendSocketCommand(ComandSend.AUTHENTICATION, null, login + ":" + password);
            return true;
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
                if (AuthenticationCRM(userName, userPass))
                {
                    string[] StrAuthentication = new string[2];
                    StrAuthentication[0] = Crypto.Encrypt(userName, Password);
                    StrAuthentication[1] = Crypto.Encrypt(userPass, Password);

                    File.WriteAllLines(NameConfig, StrAuthentication);
                }
                else
                    MessageBox.Show("Wrong UserName and Password");
                //System.Console.ForegroundColor = ConsoleColor.Green;

                //System.Console.WriteLine("Login Success");
            }
            catch (Exception ex)
            {
                //System.Console.ForegroundColor = ConsoleColor.Red;
                //System.Console.WriteLine(ex.Message);
            }
        }

        private void OnConnectError(string Message)
        {
            //делаем неактивными все устройства
            for (int i = 0; i < dataGridView1.RowCount; i++)
                dataGridView1.Rows[i].Cells[0].Style.BackColor = Color.Gray;
            textBox1.ForeColor = Color.Gray;

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
        private void OnReceivedData(RequestAnswer Message)
        {
            string strFile = "";
            string[] FileName;
            string[] strPlaylist;
            switch (Message.code)
            {
                case ComandReceive.CORE_OK:
                    try
                    {
                        string[] StrOK = Encoding.UTF8.GetString(Message.Data).Split(':');
                        if (StrOK[0] == "OK")
                        {
                            textBox1.Invoke((MethodInvoker)(delegate ()
                            {
                                switch (StrOK[1])
                                {
                                    case "AUTHENTICATION":
                                        //panelCRM.BackColor = Color.Green;
                                        //AuthenticationForm.Close();
                                        textBox1.ForeColor = Color.Green;
                                        textBox1.Text = "License OK";
                                        //включаем таймер регистрации
                                        //button1.PerformClick();
                                        //button1_Click(null, null);
                                        break;
                                    case "CONNECTION":
                                        textBox1.ForeColor = Color.Green;
                                        textBox1.Text = "License OK";
                                        textBox1.BackColor = Color.Green;
                                        //включаем таймер регистрации
                                        //button1.PerformClick();
                                        //button1_Click(null, null);
                                        break;
                                }
                            }));
                        }
                    }
                    catch { }
                    break;
                case ComandReceive.CORE_ERROR:
                    FormCollection fc;
                    bool formFlag;
                    try
                    {
                        string[] StrError = Encoding.UTF8.GetString(Message.Data).Split(':');
                        if (StrError[0] == "ERROR")
                        {
                            textBox1.Invoke((MethodInvoker)(delegate ()
                            {
                                switch (StrError[1])
                                {
                                    case "AUTHENTICATION":
                                        textBox1.BackColor = Color.Red;
                                        //выключаем таймер регистрации
                                        //button2.PerformClick();
                                        if (StrError[2] == "1")
                                        {
                                            textBox1.Text = "Check the login and password please";
                                        }
                                        if (StrError[2] == "2")
                                        {
                                            textBox1.Text = "Server error. Check the connection\n or contact server administrator";
                                        }
                                        if (StrError[2] == "3")
                                        {
                                            textBox1.Text = "Authentication error. Contact server\n administrator please";
                                        }
                                        if (StrError[2] == "4")
                                        {
                                            textBox1.ForeColor = Color.Red;
                                            textBox1.Text = "License Error";
                                            //AuthenticationForm.labelError.Text = "License error. Contact server\n administrator please";
                                        }
                                        if (StrError[2] == "5")
                                        {
                                            //AuthenticationForm.Close();
                                            textBox1.ForeColor = Color.Green;
                                            textBox1.Text = "License OK";
                                        }
                                        break;
                                    case "CONNECTION":
                                        textBox1.BackColor = Color.Red;
                                            FlagErrorConnection = true;
                                        break;
                                    case "LICENSE":
                                        textBox1.BackColor = Color.Red;
                                        FlagErrorConnection = true;
                                        textBox1.ForeColor = Color.Red;
                                        textBox1.Text = "License Error";
                                        break;
                                }
                            }));
                        }
                    }
                    catch { }
                    break;
                case ComandReceive.PLAYLIST_SEND:
                    string[] StrPlay = Encoding.UTF8.GetString(Message.Data).Split('\r');
                    StrPlay = StrPlay[0].Split('\n').ToArray();
                    string NamePlaylist = StrPlay[0];
                    dataGridView1.Invoke((MethodInvoker)(delegate ()
                    {
                        dataGridView1.Rows.Clear();
                        for (int i = 0; i < StrPlay.Length - 1; i++)
                        {
                            string[] StrForSave = StrPlay[i + 1].Split(';').ToArray();
                            dataGridView1.Rows.Add();
                            dataGridView1.Rows[i].Cells[0].ReadOnly = true;
                            dataGridView1.Rows[i].Cells[0].Value = StrForSave[0];
                            dataGridView1.Rows[i].Cells[1].Value = Convert.ToInt32(StrForSave[1]);
                            //if (checkBox5.Checked) dataGridView1.Rows[i].Cells[2].Value = true;

                        }
                        dataGridView1.ClearSelection();
                    }));
                    break;
                case ComandReceive.CLIENT_CONNECTED:
                    //RESET_CONNECTION.ImageIndex = 0;
                    textBox1.BackColor = Color.Green;
                    Thread.Sleep(100);
                    Socket.SendSocketCommand(ComandSend.CONNECTION_CRM, null, "");

                    break;
                case ComandReceive.DEVICE_IP:

                    textBox1.Invoke((MethodInvoker)(delegate ()
                    {
                        textBox1.Text = "IP = " + Encoding.UTF8.GetString(Message.Data);
                    }));
                    break;
                case ComandReceive.DEVICE_LIST_SEND:
                    if (SendComand == ComandSend.FILE_SEND_CANCEL)
                    {
                        textBox1.Invoke((MethodInvoker)(delegate ()
                        {
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                dataGridView1.Rows[i].Cells[2].Value = 0;
                                dataGridView1.Rows[i].Cells[3].Value = "";
                            }
                            textBox1.Text = "0 %";
                            //progressBar1.Value = 0;
                            textBox1.Text = "File = ";
                            SendComand = 0;
                        }));
                    }

                    int DevConnect = 0;
                    int j = 0;

                    dataGridView1.Invoke((MethodInvoker)(delegate ()
                    {
                        //сделать неактивными все девайсы;                     
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                            dataGridView1.Rows[i].Cells[0].Style.BackColor = Color.Gray;
                        textBox1.ForeColor = Color.Gray;


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
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                string Name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                                if (NameDev == Name) { flagAdd = false; DevConnect = i; break; }
                            }

                            if ((flagAdd) && (NameDev != ""))
                            {

                                dataGridView1.Rows.Add();
                                dataGridView1.Rows[CountDevice].Cells[0].ReadOnly = true;
                                dataGridView1.Rows[CountDevice].Cells[0].Value = NameDev;
                                dataGridView1.Rows[CountDevice].Cells[0].Style.BackColor = Color.LimeGreen;
                                //if (checkBox1.Checked) dataGridView1.Rows[CountDevice].Cells[1].Value = true;
                                dataGridView1.Rows[CountDevice].Cells[2].Value = 0;
                                CountDevice++;
                                dataGridView1.ClearSelection();
                            }
                            else
                            {
                                dataGridView1.Rows[DevConnect].Visible = true;
                                dataGridView1.Rows[DevConnect].Cells[0].Style.BackColor = Color.LimeGreen;
                                dataGridView1.Rows[DevConnect].Cells[1].ReadOnly = false;
                                //if (checkBox1.Checked) dataGridView1.Rows[DevConnect].Cells[1].Value = true;
                                if (DevConnect == SelectedDevice) textBox1.ForeColor = Color.LimeGreen;

                            }
                            j++;
                        }
                    }));

                    break;
                case ComandReceive.DEVICE_ERROR:
                    dataGridView1.Invoke((MethodInvoker)(delegate ()
                    {
                        string strMess = Encoding.UTF8.GetString(Message.Data);
                        string[] strMessage = new string[2];
                        try
                        {
                            strMessage = strMess.Split('\n').ToArray();
                        }
                        catch
                        {

                        }
                        if (strMessage[0] == "DEVICE_NAME_ERROR")
                        {

                            MessageBox.Show(this, "The device is disconnected or DEVICE_NAME_ERROR");

                        }
                        else if (strMessage[0] == "ERROR_DELETE_ACTIVE_FILE")
                        {
                            MessageBox.Show(this, "Error Delete Active File: " + strMessage[1]);
                        }
                        else if (strMessage[0] == "ERROR_SET_PLAYLIST")
                        {
                            if (strMessage.Length > 1)
                            {
                                if (strMessage[1] == "CRM_MODE") MessageBox.Show(this, "Error Set Playlist: CRM device mode! ");
                                else MessageBox.Show(this, "Error Set Playlist: " + strMessage[1]);
                            }

                        }
                        else if (strMessage[0] == "ERROR_MODE")
                        {
                            MessageBox.Show(this, "Error Mode: " + strMessage[1]);
                        }
                    }));
                    break;
                case ComandReceive.DEVICE_OK:
                    strFile = Encoding.UTF8.GetString(Message.Data);
                    string[] cmdI2C = strFile.Split(' ').ToArray();

                    switch (cmdI2C[0])
                    {
                        case "PLAYLIST_SEND":
                           /* if (cmdI2C[1] == "OK")
                                //обновляем
                                PLAYLIST_GET_NAMES.Invoke((MethodInvoker)(delegate ()
                                {
                                    PLAYLIST_GET_NAMES_Click(null, null);
                                }));*/
                            break;
                        case "I2C":
                            switch (cmdI2C[1])
                            {
                                case "rBrightness":
                                   /* textBoxR.Invoke((MethodInvoker)(delegate ()
                                    {
                                        textBoxR.Text = cmdI2C[2];
                                        textBoxG.Text = cmdI2C[3];
                                        textBoxB.Text = cmdI2C[4];
                                    }));*/
                                    break;

                                case "rConfiguration":
                                   /* labelSector.Invoke((MethodInvoker)(delegate ()
                                    {
                                        labelSector.Text = "Sector = " + cmdI2C[2] + ";";
                                        labelMotor.Text = "Motor = " + cmdI2C[3] + " rpm;";
                                        textBoxRotation.Text = cmdI2C[4];
                                        if (cmdI2C.Length > 5)
                                            labelTemp.Text = "Temp = " + cmdI2C[5] + ";";

                                    }));*/
                                    break;

                            }
                            break;
                        case "SET_PLAYLIST_SYNC":
                            try
                            {
                                string[] StrCmd = strFile.Split(':').ToArray();
                                PlayList.ListSync = new PlayList.ListPlay[StrCmd.Length - 1];
                                for (int i = 1; i < StrCmd.Length; i++)
                                {
                                    PlayList.ListSync[i - 1].Duration = Convert.ToInt32(StrCmd[i]);
                                }
                            }
                            catch { }

                            break;
                        case "SETTINGS":
                            //button32.ImageIndex = 3;
                            break;
                        case "VER":
                            textBox1.Invoke((MethodInvoker)(delegate ()
                            {
                                textBox1.Text = "Ver = " + cmdI2C[1] + ";";
                            }));
                            break;
                        default:
                            switch (SendComand)
                            {
                                case ComandSend.FILE_DELETE:
                                    //читаем список файлов и размер  
                                   /* FILE_GET_NAMES.Invoke((MethodInvoker)(delegate ()
                                    {
                                        FILE_GET_NAMES_Click(null, null);
                                        label15.Text = "File = ";
                                    }));*/
                                    break;
                                case ComandSend.PLAYLIST_DELETE:
                                    //читаем список playlist 
                                   /* FILE_GET_NAMES.Invoke((MethodInvoker)(delegate ()
                                    {
                                        PLAYLIST_GET_NAMES_Click(null, null);
                                        label15.Text = "File = ";

                                    }));*/
                                    break;
                            }
                            SendComand = 0;
                            break;
                    }


                    string[] StrDevOk = Encoding.UTF8.GetString(Message.Data).Split(':');
                    textBox1.Invoke((MethodInvoker)(delegate ()
                    {
                        switch (StrDevOk[0])
                        {
                            case "DEVICE_ERROR":
                                string StrError = "Error: \n";
                                for (int i = 1; i < StrDevOk.Length; i++)
                                    StrError = StrError + StrDevOk[i] + "\n";
                                //MessageBox.Show(StrError);
                                textBox1.AppendText("******************** ID **********************\n");
                                textBox1.AppendText(Message.List[0].ToString() + "\n");
                                textBox1.AppendText(StrError);
                                textBox1.AppendText("**********************************************\n");

                                break;
                        }
                    }));
                    break;
                case ComandReceive.SEND_MODE_DEVICE:

                    int Mode = Convert.ToInt32(Encoding.UTF8.GetString(Message.Data));

                 /*   radioButton1.Invoke((MethodInvoker)(delegate ()
                    {
                        switch (Mode)
                        {
                            case 0:
                                FlagSetMode = false;
                                radioButton1.Checked = true;
                                FlagSetMode = true;
                                break;
                            case 1:
                                FlagSetMode = false;
                                radioButton2.Checked = true;
                                FlagSetMode = true;
                                break;
                        }
                    }));*/

                    break;
                case ComandReceive.FILE_NAMES_SEND:
                   /* strFile = Encoding.UTF8.GetString(Message.Data);
                    if (strFile != "")
                        dataGridView2.Invoke((MethodInvoker)(delegate ()
                        {
                            dataGridView2.Rows.Clear();
                            FileName = strFile.Split('\n').ToArray();


                            for (int i = 0; i < FileName.Length; i++)
                            {
                                dataGridView2.Rows.Add();
                                dataGridView2.Rows[i].Cells[0].ReadOnly = true;
                                string[] FileNameParametr = FileName[i].Split(';').ToArray();
                                dataGridView2.Rows[i].Cells[0].Value = FileNameParametr[0];
                                if (FileNameParametr.Length > 1)
                                {
                                    try
                                    {

                                        dataGridView2.Rows[i].Cells[1].Value = FileNameParametr[1];
                                        if (FileNameParametr[2] != "-")
                                            dataGridView2.Rows[i].Cells[2].Value = (Convert.ToInt32(FileNameParametr[2]) / 1024).ToString();
                                        else dataGridView2.Rows[i].Cells[2].Value = "-";
                                        dataGridView2.Rows[i].Cells[3].Value = FileNameParametr[4];
                                        dataGridView2.Rows[i].Cells[4].Value = FileNameParametr[5];
                                    }
                                    catch { }
                                }
                                else
                                {
                                    dataGridView2.Rows[i].Cells[1].Value = "-";
                                    dataGridView2.Rows[i].Cells[2].Value = "-";
                                    dataGridView2.Rows[i].Cells[3].Value = "-";
                                    dataGridView2.Rows[i].Cells[4].Value = "-";
                                }
                                if (checkBox3.Checked) dataGridView2.Rows[i].Cells[5].Value = true;



                            }
                            dataGridView2.ClearSelection();
                        }));*/

                    break;
                case ComandReceive.DISK_FREE_SPACE:
                    AvalibleSpase = Convert.ToSingle(Encoding.UTF8.GetString(Message.Data)) / 1048576.0f;
                    textBox1.Invoke((MethodInvoker)(delegate () { textBox1.Text = "Available disk space = " + AvalibleSpase.ToString("0.0") + "Mb"; }));
                    break;
                case ComandReceive.FILE_SEND_PERCENT:
                   /* labelPersent.Invoke((MethodInvoker)(delegate ()
                    {
                        string Persent = Encoding.UTF8.GetString(Message.Data);
                        float Progress;

                        if (!Single.TryParse(Persent, out Progress))
                        {
                            Progress = Convert.ToSingle(Persent, new CultureInfo("en-US"));
                        }
                        string Name;
                        // проверяем имя устройства
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            Name = SearchID(Convert.ToString(dataGridView1.Rows[i].Cells[0].Value));
                            if (Name != "")
                            {
                                if (Message.List[0].ToString() == Name)
                                {
                                    dataGridView1.Rows[i].Cells[2].Value = (int)Progress;
                                    break;
                                }
                            }
                        }
                        Name = SearchID(Devicelabel.Text);
                        if (Name != "")
                        {
                            if (Name == Message.List[0].ToString())
                            {
                                labelPersent.Text = Persent + " %";
                                progressBar1.Value = (int)Progress;
                            }
                        }


                    }));*/
                    break;
                case ComandReceive.FILE_SEND:
                  /*  labelPersent.Invoke((MethodInvoker)(delegate ()
                    {
                        string Name;
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            // Name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                            Name = SearchID(Convert.ToString(dataGridView1.Rows[i].Cells[0].Value));
                            if (Name != "")
                            {
                                if (Message.List[0].ToString() == Name)
                                {
                                    dataGridView1.Rows[i].Cells[2].Value = 0;
                                    dataGridView1.Rows[i].Cells[3].Value = dataGridView1.Rows[i].Cells[3].Value + " - OK";
                                    break;
                                }
                            }
                        }
                        Name = SearchID(Devicelabel.Text);
                        if (Name != "")
                        {
                            if (Name == Message.List[0].ToString())
                            {
                                labelPersent.Text = "0 %";
                                progressBar1.Value = 0;
                                label15.Text = label15.Text + " - OK";
                            }
                        }
                        //запрос список файлов и размер
                        FILE_GET_NAMES_Click(null, null);
                    }));*/
                    break;
                case ComandReceive.PLAYLIST_NAMES_SEND:
                    ActivePlaylist = -1;
                    strFile = Encoding.UTF8.GetString(Message.Data);
                   /* if (strFile != "")
                        dataGridView1.Invoke((MethodInvoker)(delegate ()
                        {
                            dataGridView1.Rows.Clear();
                            dataGridView1.Rows.Clear();
                            strPlaylist = strFile.Split('\n').ToArray();

                            for (int i = 0; i < strPlaylist.Length; i++)
                            {
                                try
                                {
                                    FileName = strPlaylist[i].Split(':').ToArray();
                                }
                                catch
                                {
                                    FileName = new string[2];
                                    FileName[0] = strPlaylist[i];
                                }
                                dataGridView1.Rows.Add();
                                dataGridView1.Rows[i].Cells[0].ReadOnly = true;
                                dataGridView1.Rows[i].Cells[0].Value = FileName[0];

                                dataGridView5.Rows.Add();
                                dataGridView5.Rows[i].Cells[0].ReadOnly = true;
                                dataGridView5.Rows[i].Cells[0].Value = FileName[0];

                                if (FileName.Length > 1 && FileName[1] == "1")
                                {
                                    ActivePlaylist = i;
                                    dataGridView4.Rows[i].Cells[0].Style.BackColor = Color.CornflowerBlue;
                                    dataGridView5.Rows[i].Cells[0].Style.BackColor = Color.CornflowerBlue;
                                }
                                if (checkBox4.Checked) dataGridView4.Rows[i].Cells[1].Value = true;
                            }
                            dataGridView4.ClearSelection();
                            dataGridView5.ClearSelection();
                        }));*/
                    break;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FlagErrorConnection)
            {
                FlagErrorConnection = false;
                try
                {
                    FormAuthentication();
                }
                catch
                {
                    FormAuthentication();
                }

            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            ArrayList DevList = new ArrayList();

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Style.BackColor == Color.LimeGreen)
                {
                    string SearchName = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    int CountDev = 0;
                    foreach (string Name in DeviceList.DevName)
                    {
                        if (Name == SearchName)
                        {
                            DevList.Add(DeviceList.DevID[CountDev]);
                            break;
                        }
                        CountDev++;
                    }
                }
            }

            //  DevList = GetDevNames(true);
            Socket.SendSocketCommand(ComandSend.SYNC_TIME, DevList, "");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            while (DateTime.Now.Millisecond > 100)
            {
                Thread.Sleep(10);
            }
            //Получаем часы
            DateTime localDate = DateTime.Now.ToUniversalTime();
            int Second = 0;
            int Minute = 0;
            Second = localDate.Second + 1;
            Minute = localDate.Minute;
            if (Second >= 60)
            {
                Minute++;
                Second = Second - 60;
            }
            if (Minute >= 60) Minute = Minute - 60;

            int TimeSync = localDate.Hour * 3600 + localDate.Minute * 60 + localDate.Second + 1;

            ArrayList DevList = new ArrayList();
            DevList.Add("H-R1700802");
            //DevList = GetDevNames(false);
            //int SelectedPlaylist = dataGridView4.CurrentRow.Index;
            string FileName = "Stella_new.txt";
            Socket.SendSocketCommand(ComandSend.PLAYLIST_SET, DevList, FileName);
            //Socket.SendSocketCommand(ComandSend.PLAYLIST_SET, DevList, FileName+ ";"+TimeSync);
            Thread.Sleep(100);
            //PLAYLIST_GET_NAMES_Click(null, null);
        }
    }
}
