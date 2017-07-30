using System;
using System.Management;
using System.Runtime.InteropServices;

namespace MediaCenter.Hosting.Services
{
    public interface IDVDTrayService
    {
        void OpenDiscTray(string driveLetter);
        void CloseDiscTray(string driveLetter);

        event DiskStateDelegate DiskState;
    }

    public delegate void DiskStateDelegate(bool isEjected);
    public class DVDTrayService : IDVDTrayService
    {
        public event DiskStateDelegate DiskState;

        public DVDTrayService()
        {
            try
            {
                WqlEventQuery q = new WqlEventQuery();
                q.EventClassName = "__InstanceModificationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 1);
                q.Condition = @"TargetInstance ISA 'Win32_LogicalDisk' and TargetInstance.DriveType = 5";

                ConnectionOptions opt = new ConnectionOptions();
                opt.EnablePrivileges = true;
                opt.Authority = null;
                opt.Authentication = AuthenticationLevel.Default;
                //opt.Username = "Administrator";
                //opt.Password = "";
                ManagementScope scope = new ManagementScope("\\root\\CIMV2", opt);

                ManagementEventWatcher watcher = new ManagementEventWatcher(scope, q);
                watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
                watcher.Start();
            }
            catch (ManagementException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject wmiDevice = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string driveName = (string)wmiDevice["DeviceID"];
            Console.WriteLine(driveName);
            Console.WriteLine(wmiDevice.Properties["VolumeName"].Value);
            Console.WriteLine((string)wmiDevice["Name"]);
            if (wmiDevice.Properties["VolumeName"].Value != null)
                DiskState(false);
            else
                DiskState(true);
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString,
                            int uReturnLength, int hwndCallback);

        public void OpenDiscTray(string driveLetter)
        {
            string returnString = "";
            mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter,
                 returnString, 0, 0);
            mciSendStringA("set drive" + driveLetter + " door open", returnString, 0, 0);
        }

        public void CloseDiscTray(string driveLetter)
        {
            string returnString = "";
            mciSendStringA("open " + driveLetter + ": type CDaudio alias drive" + driveLetter,
                 returnString, 0, 0);
            mciSendStringA("set drive" + driveLetter + " door closed", returnString, 0, 0);
        }
    }
}
