using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BT_Switch
{
    public partial class Form1 : Form
    {
        public static BluetoothClient bc = new BluetoothClient();
        public static Guid myGuid = BluetoothService.SerialPort;
        public static BluetoothAddress targetaddress = BluetoothAddress.Parse("000BCE00D473");
        public static BluetoothEndPoint ep = new BluetoothEndPoint(targetaddress, myGuid);

        public Form1()
        {
            InitializeComponent();
        }

        public void Connect()
        {
            if (bc.Connected)
            {
                MessageBox.Show("Device already connected");
            }
            else
            {
                bc.DiscoverDevices();
                bc.Connect(ep);
            }
        }

        public void Disconnect() {
            bc.Close();
            bc.DiscoverDevices();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Connect();
            }
            catch
            {
                MessageBox.Show("Device not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (bc.Connected)
            {
                textBox1.Text = "Device connected";
            }

            Stream stream = bc.GetStream();
            byte[] buffer = new byte[1000];
            int readLen = stream.Read(buffer, 0, buffer.Length);
            if (readLen == 0)
            {
                MessageBox.Show("Connection is closed");
            }
            else
            {
                MessageBox.Show(string.Format("Recevied {0} bytes", readLen));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!bc.Connected)
            {
                MessageBox.Show("No device available to disconnect from", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Disconnect();
                if (!bc.Connected)
                {
                    MessageBox.Show("Device disconnected successfully", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Text = "Device disconnected";
                }
                else
                {
                    MessageBox.Show("An unknown error occured");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
