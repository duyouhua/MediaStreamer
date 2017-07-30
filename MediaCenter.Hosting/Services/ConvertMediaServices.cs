using System.Diagnostics;
using System.IO;

namespace MediaCenter.Hosting.Services
{
    public interface IConvertMediaServices
    {
        void Convert(string source);

        event ConvertMediaProgressDelegate ConvertMediaProgress;
        event ConvertMediaCompletedDelegate ConvertMediaCompleted;
    }

    public delegate void ConvertMediaProgressDelegate(int index, int Length);
    public delegate void ConvertMediaCompletedDelegate();

    public class ConvertMediaServices : IConvertMediaServices
    {
        private string VidCoder = @"C:\MediaStreamer\ThirdParty\ffmpeg\ffmpeg.exe";
        private const string ConvertLocation = "C:\\MediaStreamer\\Content\\DVDs\\";

        public event ConvertMediaProgressDelegate ConvertMediaProgress;
        public event ConvertMediaCompletedDelegate ConvertMediaCompleted;

        public void Convert(string source)
        {
            var movieFiles = Directory.GetFiles(source);
            var index = 1;
            foreach (var movie in movieFiles)
            {
                var fi = new FileInfo(movie);
                var name = fi.Name.Replace(".mkv", "");

                var moviePath = movie.Replace(fi.Name, "");
                
                var dir = new DirectoryInfo(moviePath);
                var dirName = dir.Name;

                if (!Directory.Exists(ConvertLocation + dirName))
                {
                    Directory.CreateDirectory(ConvertLocation + dirName);
                }

                var proc = ExecuteCommandLine("-i " + movie + " -vcodec copy -acodec copy C:\\MediaStreamer\\Content\\DVDs\\" + dirName + "\\" + name + ".mp4");
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                }
                
                //covert completed
                if (ConvertMediaProgress != null)
                {
                    ConvertMediaProgress(index, movieFiles.Length);
                }
            }

            if (ConvertMediaCompleted != null)
            {
                ConvertMediaCompleted();
            }
        }

        private Process proc = null;
        private Process ExecuteCommandLine(string argument)
        {
            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = VidCoder,
                    Arguments = argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            return proc;
        }
    }
}
