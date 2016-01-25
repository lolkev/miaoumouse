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

namespace WindowsFormsApplication1
{
    public partial class Formcap : Form
    {
        int leftClicks;
        int rightClick;
        int nombreDuFormulaire;
        StringBuilder msgBoxStr = new StringBuilder();
        StringBuilder seqStr = new StringBuilder();
        Stopwatch timer = new Stopwatch();
        Form3 dialog = new Form3();
        Form3 dialog2 = new Form3();
        string dir = Environment.CurrentDirectory + @"\Sauvegardes\";
        string fileName = @"Miaou - " + DateTime.Now.ToString("MMdd - HHmmss") + ".txt";

        Dictionary<Keys, DateTime> keyDownTimes = new Dictionary<Keys, DateTime>();
        Dictionary<Keys, TimeSpan> keyDurations = new Dictionary<Keys, TimeSpan>();

        public Formcap()
        {
            InitializeComponent();
            leftClicks = 0;
        }


        private void Formcap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
                Console.Out.WriteLine("Delta : "+ e.Delta + "\n coordinates (" + e.X+";"+e.Y+")");
        }

        private void Formcap_MouseWheel(object sender, MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            if(e.Delta != 0)
            {

                seqStr.Append("sc(" + e.X + "," + e.Y +","+e.Delta + ");");
                Debug.WriteLine("Mouse scrolled : " + e.Delta + " \n numTextLines : " + numberOfTextLinesToMove);
            }
        }

        private void Formcap_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!keyDownTimes.ContainsKey(e.KeyCode))
            {
                keyDownTimes[e.KeyCode] = DateTime.Now;

                seqStr.Append("+in(" + e.KeyCode + ");");
            }
        }

        private void Formcap_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (keyDownTimes.ContainsKey(e.KeyCode))
            {
                keyDurations[e.KeyCode] = keyDownTimes[e.KeyCode] - DateTime.Now;

                keyDownTimes.Remove(e.KeyCode);

                seqStr.Append("-in(" + e.KeyCode + ");");

                Debug.WriteLine("key : "+ e.KeyCode + "\ntime : " +keyDurations[e.KeyCode].Duration().ToString());
            }
        }


        private void Formcap_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case (MouseButtons.Right):
                    if (leftClicks > 0)
                    {
                        rightClick = 0;
                        Dispose();
                        //save();
                        rightClick = 1;
                        break;
                    }
                    else
                    {
                        break;
                    }
                case (MouseButtons.Left):
                    if (rightClick == 0)
                    {
                        long tmrTmp = timer.ElapsedMilliseconds;
                        timer.Reset();
                        timer.Start();
                        leftClicks++;
                        if (msgBoxStr.Length == 0)
                        {
                            msgBoxStr.Append("Click " + leftClicks + "                                                    X = " + e.X + "; Y = " + e.Y + "\n");
                        }
                        else
                        {
                            msgBoxStr.Append("Click " + leftClicks + "  temps entre clicks: " + tmrTmp + " ms,    X = " + e.X + "; Y = " + e.Y + "\n");
                        }

                        if (seqStr.Length == 0)
                        {
                            seqStr.Append("cl(" + e.X + "," + e.Y + ");");
                        }
                        else
                        {
                            seqStr.Append(" wt(" + tmrTmp + "); cl(" + e.X + "," + e.Y + ");");
                        }
                        break;
                    }
                    else
                    {
                        break;
                    }
                default:
                    break;
            }
        }

        public void repetAppend()
        {
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
            dialog.changeLabelAttendre();
            dialog.ShowDialog();
            nombreDuFormulaire = dialog.nombreTxtBox;
            seqStr.Append(" wt(" + nombreDuFormulaire * 1000 + ");");
            dialog.Dispose();
        }

    }
}
