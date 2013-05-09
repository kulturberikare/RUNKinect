// Standardbibliotek för bl.a. grafik & trådning
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
// Bluetooth-bibliotek
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace BT_Switch
{
    public partial class Form1 : Form
    {
        public static BluetoothClient bc = new BluetoothClient(); // Låser upp bluetooth-relaterade funktioner
        public static Guid myGuid = BluetoothService.SerialPort; // Service-ID, nödvändigt för att kunna identifiera enheten
        public static double sampleRate = 100; // Samplingsfrekvens
        public static Thread dataThread; // Instantiering av en ny tråd. Se detaljer i Form1_Load()
        public static BluetoothAddress targetaddress = BluetoothAddress.Parse("000BCE00D473"); // Pulsbandets fysiska adress
        public static BluetoothEndPoint ep = new BluetoothEndPoint(targetaddress, myGuid); /* Endpoint, skapas av bandets fysiska adress och Guid,
                                                                                              nödvändigt för att kunna koppla till enheten */

        public Form1() // Konstruktor
        {
            InitializeComponent(); // Initierar all grafik...
            bc.DiscoverDevices();  // ... och söker efter enheter
        }

        /// <summary>
        /// Metod som kopplar klienten till pulsbandet
        /// </summary>
        public void Connect()
        {
            if (bc.Connected) // Är enheten kopplad, notifiera användaren, koppla annars
            {
                MessageBox.Show("Device already connected");
            }
            else
            {
                bc.Connect(ep);
            }
        }

        /// <summary>
        /// Metod som frånkopplar pulsbandet
        /// </summary>
        public void Disconnect() {
            bc.Dispose(); // Stäng ner kontakten mellan pulsbandet och datorn...
            bc.DiscoverDevices(); // .. och sök efter enheter
        }

        /// <summary>
        /// Metod som läser data från pulsbandet
        /// </summary>
        public void ReadData()
        {
            NetworkStream datastream = bc.GetStream(); // Variabel som dataströmmen läses in till
            StreamReader sr = new StreamReader(datastream); // Läser av dataströmmen
            string srst = sr.ReadToEnd(); // Konverterar hela den inlästa dataströmmen till en sträng...
            File.WriteAllText(@"C:\Stream\Stream.txt", srst); // ... och skriver den till en textfil
        }

        /// <summary>
        /// Metod som körs när den grafiska delen laddas
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadStart tstream = new ThreadStart(ReadData); // Startar upp metoden ReadData i en ny tråd
            dataThread = new Thread(tstream); // Instans av en ny tråd
        }

        /// <summary>
        /// Metod som körs när knappen 'Connect' trycks ned
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try // Koppla till enheten och starta tråden där data ska tas emot
            {
                Connect();
                dataThread.Start();
            }
            catch // Hittades inte enheten, kansta undantag och notifiera användaren
            {
                MessageBox.Show("Device not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (bc.Connected) // Är enheten kopplad, ändra texten i programmet
            {
                textBox1.Text = "Device connected";
            }

            // 'Test' utförs nedan, notifierar användaren hur mycket data som tagits emot
            byte[] buffer = new byte[1000];
            Stream stream = bc.GetStream();

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

        /// <summary>
        /// Metod som körs när knappen 'Disonnect' trycks ned
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (!bc.Connected) // Är inte enheten kopplad, notifiera användaren
            {
                MessageBox.Show("No device available to disconnect from", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataThread.Abort(); // Stäng ner tråden med datainsamling
                Disconnect(); // Koppla från
                if (!bc.Connected) // Om lyckat, notifiera användaren och ändra texten i programmet, annars okänt fel
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

        private void textBox2_KeyDown(object sender, KeyEventArgs e) // Ändrar samplingsfrekvens vid användarinteraktion (enter-/returslag)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                sampleRate = double.Parse(textBox2.Text);
            }
        }
    }
}
