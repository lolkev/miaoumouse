using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace WindowsFormsApplication1
{
    public partial class Formcap : Form
    {

        static int leftClicks;
        static int rightClick;
        static int scrolls;
        static int keyboardInputs;
        int nombreDuFormulaire;

        StringBuilder msgBoxStr = new StringBuilder();
        static StringBuilder seqStr = new StringBuilder();
        static Stopwatch timer = new Stopwatch();
        Form3 dialog2 = new Form3();
        string dir = Environment.CurrentDirectory + @"\Sauvegardes\";
        string fileName = @"Miaou - " + DateTime.Now.ToString("MMdd - HHmmss") + ".txt";
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        KeyboardHelper kHelper;
        MouseHelper mHelper;

        static Dictionary<Keys, DateTime> keyDownTimes = new Dictionary<Keys, DateTime>();
        static Dictionary<Keys, TimeSpan> keyDurations = new Dictionary<Keys, TimeSpan>();

        public Formcap()
        {
            kHelper= new KeyboardHelper();
            mHelper = new MouseHelper();
            InitializeComponent();
            leftClicks = 0;
            scrolls = 0;
            keyboardInputs = 0;
        }

        private void Formcap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (leftClicks > 0 || scrolls > 0 || keyboardInputs > 0)
                {
                    kHelper.stopListening();
                    mHelper.stopListening();
                    rightClick = 0;
                    Dispose();
                    save();
                    rightClick = 1;
                }
            }
        }

        private void Formcap_MouseWheel(object sender, MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            if (e.Delta != 0)
            {
                seqStr.Append("sc(" + e.X + "," + e.Y + "," + e.Delta + ");");
                Debug.WriteLine("Mouse scrolled : " + e.Delta + " \n numTextLines : " + numberOfTextLinesToMove);
            }
        }

        //private void Formcap_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (!keyDownTimes.ContainsKey(e.KeyCode))
        //    {
        //        keyDownTimes[e.KeyCode] = DateTime.Now;

        //        //seqStr.Append("+in(" + e.KeyValue + "," );");
        //    }
        //}

        //private void Formcap_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (keyDownTimes.ContainsKey(e.KeyCode))
        //    {
        //        keyDurations[e.KeyCode] = keyDownTimes[e.KeyCode] - DateTime.Now;

        //        keyDownTimes.Remove(e.KeyCode);

        //        seqStr.Append("in(" + e.KeyValue + "," + Math.Round(keyDurations[e.KeyCode].Duration().TotalMilliseconds) + ");");

        //        Debug.WriteLine("key : " + e.KeyCode + "  --   keyValue : " + e.KeyValue + "\ntime : " + Math.Round(keyDurations[e.KeyCode].Duration().TotalMilliseconds));
        //    }
        //}

        public void repetAppend()
        {
            Form3 dialog2 = new Form3();
            dialog2.changeLabelRepets();
            dialog2.ShowDialog();
            seqStr.Append(" rp(" + dialog2.nombreTxtBox + ");");
            dialog2.Dispose();
        }

        public void save()
        {
            attendreAppend();
            repetAppend();

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(Path.Combine(dir, fileName), seqStr.ToString());
        }

        public void attendreAppend()
        {
            Form3 dialog = new Form3();
            dialog.changeLabelAttendre();
            dialog.ShowDialog();
            nombreDuFormulaire = dialog.nombreTxtBox;
            seqStr.Append(" wt(" + nombreDuFormulaire * 1000 + ");");
            dialog.Dispose();
        }

       
        public static void handleKeyboardInput(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (!keyDownTimes.ContainsKey((Keys)vkCode))
                {
                    keyDownTimes[(Keys)vkCode] = DateTime.Now;

                    Debug.WriteLine("keyboard proc read keydown");
                }
            }
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                keyboardInputs = 1;
                int vkCode = Marshal.ReadInt32(lParam);
                if (keyDownTimes.ContainsKey((Keys)vkCode))
                {
                    keyDurations[(Keys)vkCode] = keyDownTimes[(Keys)vkCode] - DateTime.Now;

                    keyDownTimes.Remove((Keys)vkCode);

                    seqStr.Append("in(" + vkCode + "," + Math.Round(keyDurations[(Keys)vkCode].Duration().TotalMilliseconds) + ");");

                    Debug.WriteLine("key : " + vkCode + "\ntime : " + Math.Round(keyDurations[(Keys)vkCode].Duration().TotalMilliseconds));
                }

            }
        }

        internal static void handleScroll(IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            int mouseDeltaUp = 120;
            int mouseDeltaDown = -120;
            scrolls = 1;

            if (hookStruct.mouseData == 7864320)
            {
                int mouseDelta = mouseDeltaUp;
                Debug.WriteLine("ze scroll iz upwards: " + hookStruct.mouseData);
                seqStr.Append("sc(" + hookStruct.pt.x + "," + hookStruct.pt.y + "," + mouseDelta +");");
                seqStr.Append("wt(100);");
            }
            if (hookStruct.mouseData == 4287102976)
            {
                int mouseDelta = mouseDeltaDown;
                Debug.WriteLine("ze scroll iz downwards: " + hookStruct.flags);
                seqStr.Append("sc(" + hookStruct.pt.x + "," + hookStruct.pt.y + "," + mouseDelta + ");");
                seqStr.Append("wt(100);");
            }
            //seqStr.Append("sc(" + hookStruct.pt.x + "," + hookStruct.pt.y + ","+  +");");
        }

        public static void handleLeftClick(IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            if (rightClick == 0)
            {
                long tmrTmp = timer.ElapsedMilliseconds;
                timer.Reset();
                timer.Start();
                leftClicks++;
                if (seqStr.Length == 0)
                {
                    seqStr.Append("cl(" + hookStruct.pt.x + "," + hookStruct.pt.y + ");");
                }
                else
                {
                    seqStr.Append(" wt(" + tmrTmp + "); cl(" + hookStruct.pt.x + "," + hookStruct.pt.y + ");");
                }

                Debug.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
    }
}
