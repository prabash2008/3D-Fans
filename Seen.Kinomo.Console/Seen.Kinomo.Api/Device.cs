using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace KINO_MO_UI
{
    public class Device
    {
        public ArrayList DevID;
        public ArrayList DevName;
        public string FileDev = "Device.txt";
        public void ReadDeviceName()
        {
            DevID = new ArrayList();
            DevName = new ArrayList();
            if (File.Exists(FileDev))
            {

                string[] AllLines = File.ReadAllLines(FileDev);
                foreach (string Dev in AllLines)
                {
                    try
                    {
                        string[] DevLine = Dev.Split(';').ToArray();
                        DevID.Add(DevLine[0]);
                        DevName.Add(DevLine[1]);
                    }
                    catch { }
                }
            }
        }
    }
}
