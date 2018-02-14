
using System.Linq;
using NReco.VideoConverter;
using System.IO;
using System.Threading;

namespace KINO_MO_UI
{
    public class FFMPEGConvert
    {
        public FFMpegConverter ffMpeg;
        private ConvertSettings ffMpegSettings;
        public Thread workerThread;
        public static string PathConvert = System.Environment.CurrentDirectory + "\\convert\\";
        public FFMPEGConvert()
        {
            ffMpeg = new FFMpegConverter();
            ffMpegSettings = new ConvertSettings();
            ffMpegSettings.SetVideoFrameSize(880, 880);
            ffMpegSettings.VideoCodec = "h264";
            ffMpegSettings.CustomOutputArgs = "-map 0:v -color_range 1 -preset fast -tune animation -profile:v high -vf \"format=yuv420p, setdar=4:3\"";
            ffMpegSettings.VideoFrameRate = 30;

            //ffMpegSettings.CustomOutputArgs = "-c:v libx264rgb -preset veryslow -tune ssim -s 680x680 -vf setdar=1:1 -pix_fmt bgr0 -r 25";
        }
        private void ConvertVideo(object NameVideo)
        {

            string Obj = NameVideo.ToString();
            string[] Str = Obj.Split('\n').ToArray();
            try
            {
                ffMpeg.ConvertMedia(Str[0] + "\\" + Str[1], Format.mp4, PathConvert + Str[1], NReco.VideoConverter.Format.mp4, ffMpegSettings);
            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show("Wrong video format - " + Str[1] + ". Use .mp4 videos please");
                //if (File.Exists(PathConvert + Str[1])) File.Delete(PathConvert + Str[1]);
                //Form1.DeviceName = "";
            }
        }

        public void VideoConvert(string Path, string VideoName)
        {
            workerThread = new Thread(new ParameterizedThreadStart(ConvertVideo));
            workerThread.Start(Path + "\n" + VideoName);
        }
    }
}
