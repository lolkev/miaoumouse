using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        Stopwatch timer = new Stopwatch();
        Stopwatch keyStrokeTimer = new Stopwatch();

        private BackgroundWorker bw = new BackgroundWorker();
        private static ManualResetEvent mre = new ManualResetEvent(false);
        bool pause = false;
        bool click = false;

        int repetitionsActuelles = 0;
        int nombreRepetitions = 1;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public void doMouseClick(int X, int Y)
        {
            Point retPoint = new Point();
            GetCursorPos(ref retPoint);
            SetCursorPos(X, Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)X, (uint)Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
            SetCursorPos(retPoint.X, retPoint.Y);
        }


        public void doMouseScroll(int X, int Y, int delta)
        {
            Point retPoint = new Point();
            GetCursorPos(ref retPoint);
            SetCursorPos(X, Y);
            mouse_event(MOUSEEVENTF_WHEEL, (uint)X, (uint)Y, (uint)delta, 0);
            SetCursorPos(retPoint.X, retPoint.Y);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            List<string> sequence = (List<string>)e.Argument;

            int progress;
            int chiffreRepet;
            initWorker(sequence, out progress, out chiffreRepet);

            while (!worker.CancellationPending == true)
            {
                while (repetitionsActuelles < nombreRepetitions)
                {

                    foreach (string seq in sequence)
                    {
                        if (pause)
                        {
                            mre.WaitOne();
                        }
                        else
                        {
                            progress += 1;
                            string instruction = seq.TrimStart();
                            if (instruction.StartsWith("cl"))
                            {
                                doClick(instruction);
                            }
                            else if (instruction.StartsWith("sc"))
                            {
                                doScroll(instruction);
                            }
                            else if (instruction.StartsWith("+in"))
                            {
                                //doInput(instruction);
                            }
                            else if (instruction.StartsWith("-in"))
                            {
                                //stopInput(instruction);
                            }
                            else if (instruction.StartsWith("wt"))
                            {
                                getWaitInstruction(instruction);
                            }
                            else if (instruction.StartsWith("rp"))
                            {
                                progress = getRepeatInstruction(progress, chiffreRepet);
                            }
                            
                            worker.ReportProgress(((progress) * 100) / (sequence.Count() * chiffreRepet));
                        }
                    }

                }

            }
            e.Cancel = true;

        }

        private int getRepeatInstruction(int progress, int chiffreRepet)
        {
            repetitionsActuelles++;
            nombreRepetitions = chiffreRepet;
            progress -= 1;
            return progress;
        }

        private void getWaitInstruction(string instruction)
        {
            int waitTime = Convert.ToInt32(instruction.Trim('w', 't', '(', ')'));
            timer.Start();
            do
            {
                int i;
            } while (timer.ElapsedMilliseconds < waitTime);
            timer.Reset();
        }

        private void doClick(string instruction)
        {
            string[] coords = instruction.Trim('c', 'l', '(', ')').Split(',');
            doMouseClick(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]));
        }

        private void doScroll(string instruction)
        {
            string[] coords = instruction.Trim('s', 'c', '(', ')').Split(',');
            doMouseScroll(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]), Convert.ToInt32(coords[2]));
        }

        private void initWorker(List<string> sequence, out int progress, out int chiffreRepet)
        {
            progress = 0;
            chiffreRepet = 1;
            string repet = string.Empty;
            foreach (string s in sequence)
            {
                if (s.Contains("rp"))
                    repet = s;
            }

            string rt = repet.Trim(new char[] { ' ', 'r', 'p', '(', ')' });

            chiffreRepet = Convert.ToInt32(rt);


            repetitionsActuelles = 0;
            nombreRepetitions = 1;

        }


        public Form1()
        {
            InitializeComponent();
            //_hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            Stopwatch sw = new Stopwatch();
            Debug.WriteLine("time : " + KeyPressHelperExtensions.Time(
                sw, () => KeyPressHelperExtensions.GetKeyFromHookCallback(nCode, wParam, lParam)));

            #region probation
            //if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            //{
            //    kph1.previousPressedKeyCode = 0;
            //    kph1.currentReleasedKeyCode = 0;
            //    int vkCode = Marshal.ReadInt32(lParam);   //timemr 
                
            //    kph1.currentPressedKeyCode = (Keys)vkCode;
            //    kphList.Add(kph1);
            //}
            //if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) // lparam manage l'état précédent de la touche. (toujours pas compris à voir)
            //{
            //    kph2.previousPressedKeyCode = kphList.Find(k => k.Equals(kph1)).currentPressedKeyCode;
            //    kph2.currentPressedKeyCode = 0;
            //    int vkCode = Marshal.ReadInt32(lParam);
            //    kph2.currentReleasedKeyCode = (Keys)vkCode;
            //    if (kph2.previousPressedKeyCode == kph2.currentReleasedKeyCode)
            //    {
            //        //keyStrokeTimer.Stop();
            //        //MessageBox.Show("" + keyStrokeTimer.ElapsedMilliseconds);
            //        //outputKeyStrokeToDebug(keyStrokeTimer, kph2);
            //    }
            //    else
            //    {
            //        //keyStrokeTimer.Stop();
            //    }
            //}
            #endregion


            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initBackgroundWorker();
        }

        private void initBackgroundWorker()
        {
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Formcap cap = new Formcap();
            cap.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            UnhookWindowsHookEx(_hookID);
            mre.Reset();
            pause = false;
            BackgroundWorker bw = new BackgroundWorker();
            initBackgroundWorker();
            List<string> sequence = new List<string>();
            OpenFileDialog fd = new OpenFileDialog();
            Stream myStream;

            fd.InitialDirectory = Environment.CurrentDirectory;
            fd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    fd.OpenFile();

                    if ((myStream = fd.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            StreamReader sr = new StreamReader(myStream);

                            foreach (string s in sr.ReadToEnd().Split(';'))
                            {
                                sequence.Add(s);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            if (sequence.Count() != 0)
            {
                bw.RunWorkerAsync(sequence);
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                this.tbProgress.Text = "Cancelled!";
            }

            else if (!(e.Error == null))
            {
                this.tbProgress.Text = ("Error: " + e.Error.Message);
            }

            else
            {
                this.tbProgress.Text = "Done!";
            }
        }

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!click)
            {
                pause = true;
                mre.Reset();
                bw.CancelAsync();
            }
        }
    }
}
