using System.Windows.Input;
namespace WindowsFormsApplication1
{
    partial class Formcap
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Formcap));
            this.SuspendLayout();
            // 
            // Formcap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(600, 300);
            this.ControlBox = false;
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "Formcap";
            this.Opacity = 0.2D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form2";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Formcap_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Formcap_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Formcap_MouseDown);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Formcap_MouseWheel);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Formcap_MouseMove);
            this.ResumeLayout(false);

        }

        #endregion
    }
}