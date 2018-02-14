using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KINO_MO_UI
{
    public class PlayList
    {
        public struct ListPlay
        {
            public DateTime Time;
            public string Name;
            public int Duration;
            public string type; 
        }
        public ListPlay[] ListForPlay;
        public static ListPlay[] ListSync;
        public PlayList()
        {

        }
        public DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
        public double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        public ListPlay[] GetPlaylist(string NamePlaylist)
        {

            string[] readText = File.ReadAllLines(NamePlaylist);
            ListPlay[] ListForPlay = new ListPlay[readText.Length];
            int i = 0;
            foreach (string s in readText)
            {
                string[] Element = s.Split(';');
                ListForPlay[i].Time = ConvertFromUnixTimestamp(Convert.ToDouble(Element[0]));
                ListForPlay[i].Name = Element[1];
                ListForPlay[i].Duration = Convert.ToInt32(Element[2]);
                ListForPlay[i].type = Element[4];
                i++;
            }
            return ListForPlay;
        }
        public bool SavePlayList(string NamePlaylist)
        {
            try
            {
                string str = "";
                FileInfo fi = new FileInfo(NamePlaylist);
                if (fi.Exists) fi.Delete();
                //создаем новый playlist
                using (StreamWriter sw = fi.CreateText())
                {
                    for (int i = 0; i < ListForPlay.Length; i++)
                    {
                        str = "";
                        str = "0;";
                        str = str + ListForPlay[i].Name + ";";
                        str = str + ListForPlay[i].Duration.ToString() + ";;";
                        str = str + ListForPlay[i].type + ";;";
                        sw.WriteLine(str);
                    }
                    sw.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
