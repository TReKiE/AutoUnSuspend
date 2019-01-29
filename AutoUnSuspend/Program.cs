using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Sparrow;
using System.Runtime.InteropServices;

namespace AutoUnSuspend
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                try
                {
                    int nProcessID = Process.GetCurrentProcess().Id;
                    Process[] processes = Process.GetProcesses();
                    foreach (var process in processes)
                    {
                        Debug.WriteLine("Processing.. " + process.ProcessName);
                        if (process.ProcessName != "Secure System" && process.ProcessName != "Idle" && process.Id != nProcessID && process.Threads[0].WaitReason == ThreadWaitReason.Suspended && (DateTime.Now-process.StartTime).TotalSeconds < 60 )
                        {
                            Debug.WriteLine("Resuming process");
                            process.Resume();
                        }
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(" *** EXCEPTION *** " + ex.ToString());
                }
            }
        }
    }

    public static class ProcessExtension
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        public static void Suspend(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SuspendResume, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                SuspendThread(pOpenThread);
            }
        }
        public static void Resume(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SuspendResume, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                ResumeThread(pOpenThread);
            }
        }
    }
}
