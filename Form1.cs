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

        private enum MouseMessages
        {
            MOUSEEVENTF_LEFTDOWN = 0x02,
            MOUSEEVENTF_LEFTUP = 0x04,
            MOUSEEVENTF_WHEEL = 0x0800,
        }

        bool progressCountedOnceAlready = false;

        Stopwatch timer = new Stopwatch();
        Stopwatch keyStrokeTimer = new Stopwatch();

        private BackgroundWorker bw = new BackgroundWorker();
        private static ManualResetEvent mre = new ManualResetEvent(false);
        bool pause = false;
        bool click = false;

        int repetitionsActuelles = 0;
        int nombreRepetitions = 1;

        public void doMouseClick(int X, int Y)
        {
            Point retPoint = new Point();
            GetCursorPos(ref retPoint);
            SetCursorPos(X, Y);
            mouse_event((uint)MouseMessages.MOUSEEVENTF_LEFTDOWN, (uint)X, (uint)Y, 0, 0);
            mouse_event((uint)MouseMessages.MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
            SetCursorPos(retPoint.X, retPoint.Y);
        }


        public void doKeyInput(string key, string duration)
        {
            KeyPressHelper.HoldKey(Convert.ToByte(key), Convert.ToInt32(duration));
        }

        public void doMouseScroll(int X, int Y, int delta)
        {
            Point retPoint = new Point();
            GetCursorPos(ref retPoint);
            SetCursorPos(X, Y);
            mouse_event((uint)MouseMessages.MOUSEEVENTF_WHEEL, (uint)X, (uint)Y, (uint)delta, 0);
            SetCursorPos(retPoint.X, retPoint.Y);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            List<string> sequence = (List<string>)e.Argument;

            int progress;
            int chiffreRepet;
            initWorker(sequence, out progress, out chiffreRepet);

            while (!worker.CancellationPending)
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
                            else if (instruction.StartsWith("in"))
                            {
                                doInput(instruction);
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
            mre.WaitOne();
        }

        private int getRepeatInstruction(int progress, int chiffreRepet)
        {
            repetitionsActuelles++;
            nombreRepetitions = chiffreRepet;
            if (!progressCountedOnceAlready)
            {
                progress -= 1;
            }
            progressCountedOnceAlready = true;
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

        private void doInput(string instruction)
        {
            string[] coords = instruction.Trim('i', 'n', '(', ')').Split(',');
            doKeyInput(coords[0], coords[1]); //coords[0]= key; coords[1] = duration en ms 
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
                else
                    repet = "rp(1);";
            }

            string rt = repet.Trim(new char[] { ' ', 'r', 'p', '(', ')', ';' });

            chiffreRepet = Convert.ToInt32(rt);

            repetitionsActuelles = 0;
            nombreRepetitions = 1;

        }

        public Form1()
        {
            InitializeComponent();
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
            mre.Reset();
            pause = false;
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
                                if (!string.IsNullOrEmpty(s))
                                    sequence.Add(s.Trim());
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
                bw.CancelAsync();
                bw.RunWorkerAsync(sequence);
            }

        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.Dispose();

            if ((e.Cancelled == true))
            {
                this.tbProgress.Text = "Cancelled!";
            }

            else if (!(e.Error == null))
            {
                this.tbProgress.Text = ("Error: " + e.Error.Message);
            }
            else if (e.Result.ToString() == "finished")
            {
                this.tbProgress.Text = "victoly";
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
