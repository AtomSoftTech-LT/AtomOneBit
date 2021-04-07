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
            // I made this sooooo... please make sure if you remake to include something about me
            MessageBox.Show("Created by Jason Lopez\n\rAtomSoftTech.com\n\rAtomSoft@gmail.com", "Some info", MessageBoxButtons.OK);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // For exiting.. make sure it wasnt a mistake click.. if your dont care then remove entire "if" line
            if (MessageBox.Show("Are you sure?", "Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                this.Close();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Will hold any text i decide to show user for textbox
            string UserOutput = "";

            // Popup the open file dialog, if they canceled then return
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            // Load our filename into a variable
            string imgPath = openFileDialog1.FileName;

            // Check if file exist.. just incase.. you never know
            // If it doesnt then tell the user and return
            if (!File.Exists(imgPath))
            {
                MessageBox.Show("File does not exist!", "File Error");
                return;
            }

            // File exist so first line of output is the Original Path info
            // I added this because im not that organized lol and sometimes have
            // multiple filenames that are the same.
            UserOutput = "// Original File Path: " + imgPath + Environment.NewLine;

            // Start the Binary Reader which will be used to open image and read bytes
            BinaryReader imgBin = new BinaryReader(File.Open(imgPath, FileMode.Open));

            // Var to hold our header. A var wasnt or isnt really needed but used for 
            // debuggine purposes and can be changed but why if it works fine.
            byte[] bmpHeader = imgBin.ReadBytes(2);

            // Test first 2 bytes for BM which signifies a BMP
            // If it fails, alert user and return (also close the file)
            if ((bmpHeader[0] != 0x42) || (bmpHeader[1] != 0x4d))
            {
                MessageBox.Show("Not a BMP at all!");
                imgBin.Close();
                return;
            }

            // Goto offset 0x001C which holds data on how many bits per pixel the image is
            // Its 2 bytes long
            imgBin.BaseStream.Seek(0x001C, 0);
            byte[] imgBPP = imgBin.ReadBytes(2);
            uint myBPP = BitConverter.ToUInt16(imgBPP, 0);

            // If its greater than 1 Bit Per Pixel then alert user, close file and return
            if (myBPP > 1)
            {
                MessageBox.Show("Not a 1 Bit per pixel image.");
                imgBin.Close();
                return;
            }

            //Get Width data @ offset 0x0012
            // The data is 4 bytes long
            imgBin.BaseStream.Seek(0x0012, 0);

            byte[] imgWidth = imgBin.ReadBytes(4);

            // Convert that data to a unsigned int for ease of use
            uint myWidth = BitConverter.ToUInt32(imgWidth, 0);

            //Height is next 4 Bytes
            byte[] imgHeight = imgBin.ReadBytes(4);

            // Convert it also for ease of use
            uint myHeight = BitConverter.ToUInt32(imgHeight, 0);

            // Convert to single bytes for later Might have to change this
            byte[] userData = { (byte)myWidth, (byte)myHeight };

            // Get that Width and Height info into string for user
            UserOutput += "// Width: " + myWidth + "[0x" + myWidth.ToString("X2") + "]" + Environment.NewLine;
            UserOutput += "// Height: " + myHeight + "[0x" + myHeight.ToString("X2") + "]" + Environment.NewLine;

            //Offset 0x000A contains the start of our BMP pixel data location in 4 bytes
            imgBin.BaseStream.Seek(0x000A, 0);
            byte[] imgOffset = imgBin.ReadBytes(4);
            long myOffset = BitConverter.ToUInt32(imgOffset, 0);

            // Determine how many bytes is our data row by dividing width by 8 bits
            long myBytes = myWidth / 8;

            // The BIT padding is the amount of bits from the last byte we do not use
            long myBitPadding = 8 - (myWidth % 8);

            // Since the MyBytes will only tell use whole bytes used
            // if we have padding then that means we have a partial byte
            // and thus should add it to the count even if not fully used
            // some data we need is still there
            if (myBitPadding > 0)
                myBytes++;

            // BMPs are 4 byte chunks of data. So if we have 16 pixel width then thats only
            // 16 bits or 2 bytes. Meaning there are 2 bytes of padding to make it 4 bytes wide
            // We have to calculate how much byte padding there is to determine the rows or Scan Line 
            long myBytePadding = 0;

            // myBytes hold the amount of bytes we use. For example if 24 pixels then 3 bytes
            // So. myBytePadding = 4 - ((myBytes % 4 ) * 4);
            //     myBytePadding = 4 - ((0.75) * 4)
            //     myBytePadding = 4 - ((3))
            //     myBytePadding = 1  
            // So at 24 pixels aka 3 bytes we need 4 bytes... we have to add our byte padding
            // which is 1... wallla now be have our 4 bytes
            if (myBytes % 4 > 0)
                myBytePadding = 4 - ((myBytes % 4 ) * 4);

            // The scan line is the entire row plus padding. We check padding below
            int scanLine = (int)myBytes;

            // If we do have padding then add it to the scan line
            if (myBytePadding > 0)
                scanLine += (int)myBytePadding;

            // Seek to last scan line
            long lastLine = myOffset + (myHeight * scanLine);

            // just a new var to hold this data in int format
            int bytesWeNeed = (int)myBytes;

            // helper var to make a byte array later with the len of bytes we need
            int arrarySize = (bytesWeNeed * (int)myHeight) + 3;

            // The buffer array to hold line data
            byte[] myLineBuff = new byte[bytesWeNeed];

            // Get the actual name of the file... no path and no extension
            // First splits all paths, last one contains file name with extension
            string[] fileNameSplit = imgPath.Split('\\');

            // Split the last one by . which seperates the name and extension
            string[] imgNameSplit = fileNameSplit[fileNameSplit.Length - 1].Split('.');

            // first one is the NAME... next one would be extension aka bmp
            string myName = imgNameSplit[0];

            // Create code for user, call the var the name we got from file
            UserOutput += Environment.NewLine + "unsigned char " + myName + "[" + arrarySize.ToString() + "] = {";

            //Add Width, Height and our Byte Len in hex format for MCU to work easier
            UserOutput += Environment.NewLine + "    0x" + BitConverter.ToString(userData, 0).Replace("-", ", 0x") + ", 0x" + bytesWeNeed.ToString("X2") +  ", ";

            // The 3 lines below are used to add 1's to unused end bits as 0 on my LCD means BLACK and 1 means NONE
            int newPad = 8 - (int)myBitPadding;
            int addData = 0xFF >> newPad;

            byte OrThis = (byte)addData;

            // Grab Line and loop lines in reverse
            for (int i = 0; i < myHeight; i++)
            {
                // Last line holds the END of data so jump back 1 scanLine at each loop
                lastLine -= scanLine;

                // Goto our data offset
                imgBin.BaseStream.Seek(lastLine, 0);

                // read the bytes we need
                myLineBuff = imgBin.ReadBytes(bytesWeNeed);

                // Or the end of the last byte for unused data
                int orTemp = myLineBuff[myLineBuff.Length - 1] | OrThis;
                myLineBuff[myLineBuff.Length - 1] = (byte)orTemp;

                // Get data ready for user
                UserOutput += Environment.NewLine + "    0x" + BitConverter.ToString(myLineBuff, 0).Replace("-", ", 0x");

                // if its not the last line add a comma to end of row
                if(i != myHeight -1)
                    UserOutput += ", ";

                // Loop again if needed
            }
            
            // Add end of variable stuff
            UserOutput += Environment.NewLine + "};";

            // Close the File
            imgBin.Close();

            // Load image for user
            pictureBox1.ImageLocation = imgPath;
            pictureBox1.Load();

            // Show user their code :)
            txtSource.Text = UserOutput;
        }
    }
}
