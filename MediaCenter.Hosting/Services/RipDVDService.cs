using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MediaCenter.Hosting.Services
{
    public interface IRipDVDService
    {
        void Abort();
        string GetDiscName();
        void RipDVD(string RipPath, string DiscName);

        event TitleCopiedDelegate TitleCopied;
        event RippingCompletedDelegate RippingCompleted;
        event ProgressDelegate Progress;
        event BackupFailedDelegate BackupFailed;

        string DriveLetter { get; set; }
    }

    public delegate void TitleCopiedDelegate(int TitleIndex);
    public delegate void RippingCompletedDelegate();
    public delegate void ProgressDelegate(string output);
    public delegate void BackupFailedDelegate();

    public class RipDVDService : IRipDVDService
    {
        public event TitleCopiedDelegate TitleCopied;
        public event RippingCompletedDelegate RippingCompleted;
        public event ProgressDelegate Progress;
        public event BackupFailedDelegate BackupFailed;

        public string DriveLetter { get; set; }

        private string MakeMKVPath = @"C:\Program Files (x86)\MakeMKV\makemkvcon.exe";

        private int RipIndex = 0;

        public async void RipDVD(string RipPath, string DiscName)
        {
            await Task.Run(() =>
            {
                RipIndex = 0;

                if (String.IsNullOrWhiteSpace(DiscName))
                {
                    DiscName = GetDiscName();
                }

                var pathToRip = RipPath + "\\" + DiscName;
                if (!Directory.Exists(pathToRip))
                {
                    Directory.CreateDirectory(pathToRip);
                }

                var proc = ExecuteCommandLine("mkv disc:0 all " + pathToRip);
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    if (line.Contains("Copy complete."))
                    {
                        // we are done ripping the disc
                        if (RippingCompleted != null)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate {
                                RippingCompleted();
                            }));
                        }
                    }
                    else if (line.Contains("Operation successfully completed"))
                    {
                        // title completed
                        RipIndex++;
                        if (TitleCopied != null)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                            {
                                TitleCopied(RipIndex);
                            }));
                        }
                    }
                    else if (line.Contains("Backup failed") || line.Contains("http://www.makemkv.com/developers"))
                    {
                        if (BackupFailed != null)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                            {
                                BackupFailed();
                            }));
                        }
                    }
                    else
                    {
                        if (Progress != null)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                            {
                                Progress(line);
                            }));
                        }
                    }
                }
            });
        }

        public void Abort()
        {
            proc.Kill();
        }

        public string GetDiscName()
        {
            string discName = "";
            var proc = ExecuteCommandLine("-r info disc:0");
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("DRV:0"))
                {
                    // try to figure out the name of the disc
                    var parsing = line.Split(',');
                    discName = parsing[5];
                    DriveLetter = parsing[6];
                    break;
                }
            }

            discName = discName.Replace("\\","").Replace("\"","");
            return discName;
        }

        Process proc = null;
        private Process ExecuteCommandLine(string argument)
        {
            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = MakeMKVPath,
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
