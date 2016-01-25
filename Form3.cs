using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        int attend=0;
        public void changeLabelAttendre()
        {
            label1.Text = "Combien de secondes attendre\n entre les répétitions?";
            attend = 1;
        }

        public void changeLabelRepets()
        {
            label1.Text = "Combien de répétitions?";
            attend = 0;
        }
        public int nombreTxtBox;
        

        public Form3()
        {
            InitializeComponent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            nombreTxtBox = 1;
            if (e.KeyCode == Keys.Enter)
            {
                nombreTxtBox = Convert.ToInt32(textBox1.Text);
                if ((nombreTxtBox != 0 && nombreTxtBox < 50) || nombreTxtBox != 0 && attend == 1)
                {
                    Dispose();
                }
                else
                {
                    MessageBox.Show("1 - 50 pls");
                }
            }
        }

        
    }
}

