using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace AtomOneBit
{
    public partial class Form1 : Form
    {

        string currentFile = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void stretchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void autoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void centerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Jason Lopez\n\rAtomSoftTech.com\n\rAtomSoft@gmail.com", "Some info", MessageBoxButtons.OK);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                this.Close();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string UserOutput = "";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string imgPath = openFileDialog1.FileName;

            if (!File.Exists(imgPath))
            {
                MessageBox.Show("File does not exist!", "File Error");
                return;
            }

            UserOutput = "// Original File Path: " + imgPath + Environment.NewLine;

            currentFile = imgPath;

            BinaryReader imgBin = new BinaryReader(File.Open(imgPath, FileMode.Open));

            byte[] bmpHeader = imgBin.ReadBytes(2);

            if ((bmpHeader[0] != 0x42) || (bmpHeader[1] != 0x4d))
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
            imgBin.BaseStream.Seek(0x0012, 0);

            byte[] imgWidth = imgBin.ReadBytes(4);
            uint myWidth = BitConverter.ToUInt32(imgWidth, 0);

            //Height is next 4 Bytes
            byte[] imgHeight = imgBin.ReadBytes(4);
            uint myHeight = BitConverter.ToUInt32(imgHeight, 0);

            byte[] userData = { (byte)myWidth, (byte)myHeight };

            UserOutput += "// Width: " + myWidth + "[0x" + myWidth.ToString("X2") + "]" + Environment.NewLine;
            UserOutput += "// Height: " + myHeight + "[0x" + myHeight.ToString("X2") + "]" + Environment.NewLine;

            //Data Offset
            imgBin.BaseStream.Seek(0x000A, 0);
            byte[] imgOffset = imgBin.ReadBytes(4);
            long myOffset = BitConverter.ToUInt32(imgOffset, 0);

            long myBytes = myWidth / 8;
            long myBitPadding = 8 - (myWidth % 8);

            // Round to end of byte
            if (myBitPadding > 0)
                myBytes++;

            long myBytePadding = 0;

            if (myBytes % 4 > 0)
                myBytePadding = 4 - (myBytes % 4);

            int scanLine = (int)myBytes;

            // Add padding
            if (myBytePadding > 0)
                scanLine += (int)myBytePadding;

            // Seek to last scan line
            long lastLine = myOffset + (myHeight * scanLine);

            int bytesWeNeed = (int)myBytes;
            int arrarySize = (bytesWeNeed * (int)myHeight) + 3;

            byte[] myLineBuff = new byte[bytesWeNeed];

            string[] fileNameSplit = imgPath.Split('\\');
            string[] imgNameSplit = fileNameSplit[fileNameSplit.Length - 1].Split('.');
            string myName = imgNameSplit[imgNameSplit.Length - 1];


            UserOutput += Environment.NewLine + "unsigned char " + myName + "[" + arrarySize.ToString() + "] = {";


            //Add Width and Height
            UserOutput += Environment.NewLine + "    0x" + BitConverter.ToString(userData, 0).Replace("-", ", 0x") + ", 0x" + bytesWeNeed.ToString("X2") +  ", ";

            int newPad = 8 - (int)myBitPadding;
            int addData = 0xFF >> newPad;

            byte OrThis = (byte)addData;

            // Grab Line and loop lines in reverse
            for (int i = 0; i < myHeight; i++)
            {
                lastLine -= scanLine;

                imgBin.BaseStream.Seek(lastLine, 0);

                myLineBuff = imgBin.ReadBytes(bytesWeNeed);
                int orTemp = myLineBuff[myLineBuff.Length - 1] | OrThis;
                myLineBuff[myLineBuff.Length - 1] = (byte)orTemp;

                UserOutput += Environment.NewLine + "    0x" + BitConverter.ToString(myLineBuff, 0).Replace("-", ", 0x");

                if(i != myHeight -1)
                    UserOutput += ", ";


            }
            
            UserOutput += Environment.NewLine + "};";
            imgBin.Close();

            pictureBox1.ImageLocation = currentFile;
            pictureBox1.Load();

            //pictureBox1.Image = Image.FromStream(imgBin.BaseStream);

            txtSource.Text = UserOutput;
        }
    }
}
