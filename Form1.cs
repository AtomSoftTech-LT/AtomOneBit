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
                imgBin.Close();
                return;
            }

            //BPP Offset
            imgBin.BaseStream.Seek(0x001C, 0);
            byte[] imgBPP = imgBin.ReadBytes(2);
            uint myBPP = BitConverter.ToUInt16(imgBPP, 0);

            if (myBPP > 1)
            {
                MessageBox.Show("Not a 1 Bit per pixel image.");
                imgBin.Close();
                return;
            }

            //Width Location 4 bytes
            imgBin.BaseStream.Seek(0x0012,0);

            byte[] imgWidth = imgBin.ReadBytes(4);
            uint myWidth = BitConverter.ToUInt32(imgWidth, 0);

            UserOutput += "// Width: " + myWidth + Environment.NewLine;

            //Height is next 4 Bytes
            byte[] imgHeight = imgBin.ReadBytes(4);
            uint myHeight = BitConverter.ToUInt32(imgHeight, 0);

            UserOutput += "// Height: " + myHeight + Environment.NewLine;

            //Data Offset
            imgBin.BaseStream.Seek(0x000A, 0);
            byte[] imgOffset = imgBin.ReadBytes(4);
            long myOffset = BitConverter.ToUInt32(imgOffset, 0);

            txtSource.Text = UserOutput;
            imgBin.Close();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }
    }
}
