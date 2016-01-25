using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public class KeyPressHelper
    {
        public Keys currentPressedKeyCode;
        public Keys currentReleasedKeyCode;
        public Keys previousPressedKeyCode;

        public KeyPressHelper()
        {
            currentPressedKeyCode = new Keys();
            currentReleasedKeyCode = new Keys();
            previousPressedKeyCode = new Keys();
        }
    }

    public static class KeyPressHelperExtensions
    {

        public static KeyPressHelper kph1 = new KeyPressHelper();
        public static KeyPressHelper kph2 = new KeyPressHelper();
        public static List<KeyPressHelper> kphList = new List<KeyPressHelper>();

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        public static long Time(this Stopwatch sw, Action action)
        {

            if (sw.IsRunning)
            {
                action();
                sw.Stop();
            }

            sw.Restart();
            action();           


            return sw.ElapsedMilliseconds;
        }

        //should Reset timer?
        public static void GetKeyFromHookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            //si différents, est-ce que c'est un keydown?
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                kph1.previousPressedKeyCode = 0;
                kph1.currentReleasedKeyCode = 0;
                int vkCode = Marshal.ReadInt32(lParam);

                kph1.currentPressedKeyCode = (Keys)vkCode;
                kphList.Add(kph1);

                Debug.WriteLine("kph1.currentPressedKeyCode  : " + kph1.currentPressedKeyCode);
                kph1.previousPressedKeyCode = kph1.currentPressedKeyCode;

                //timer est reset parce que la touche est différente.
                //                    return true;
            }
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) // lparam manage l'état précédent de la touche. (toujours pas compris à voir)
            {
                kph2.previousPressedKeyCode = kphList.Find(k => k.Equals(kph1)).currentPressedKeyCode;
                kph2.currentPressedKeyCode = 0;
                int vkCode = Marshal.ReadInt32(lParam);
                kph2.currentReleasedKeyCode = (Keys)vkCode;

                //est-ce que la touche relachée est la même que la touche précédente?
                if (kph2.previousPressedKeyCode == kph2.currentReleasedKeyCode)
                {
                    Debug.WriteLine("kph2.previousPressedKeyCode : " + kph2.previousPressedKeyCode);
                    Debug.WriteLine("kph2.currentReleasedKeyCode : " + kph2.currentReleasedKeyCode);
                }
            }
        }
    }
}
