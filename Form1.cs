using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace AtomOneBit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string UserOutput = "";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string imgPath = openFileDialog1.FileName;

            if(!File.Exists(imgPath))
            {
                MessageBox.Show("File does not exist!", "File Error");
                return;
            }

            UserOutput = "// Original File Path: " + imgPath + Environment.NewLine;

            BinaryReader imgBin = new BinaryReader(File.Open(imgPath, FileMode.Open));

            byte[] bmpHeader = imgBin.ReadBytes(2);

            if((bmpHeader[0] != 0x42) || (bmpHeader[1] != 0x4d))
            {
                MessageBox.Show("Not a BMP at all!");
                return;
            }

            txtSource.Text = UserOutput;
            imgBin.Close();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }
    }
}
