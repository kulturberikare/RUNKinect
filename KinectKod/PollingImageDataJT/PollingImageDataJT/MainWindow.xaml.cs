#region Comments
// Date of creation: 17-02-13
// Creator: Johan Tidholm
// Source: Begining kinect programing
// Best music to creation: Rush - Time stand still
#endregion Comments

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace PollingImageDataJT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        private byte[] _ColorImagePixelData;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        #endregion Constructor

        #region Methods
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DiscoverKinectSensor();
            PollColorImageStream();
        }


        private void DiscoverKinectSensor()
        {
            if (this._Kinect != null && this._Kinect.Status != KinectStatus.Connected)
            {
                // If the sensor is no longer connected, we need to discover a new one.
                this._Kinect = null;
            }

            if (this._Kinect == null)
            {
                //Find the first connected sensor
                this._Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                if (this._Kinect != null)
                {
                    //Initialize the found sensor
                    this._Kinect.ColorStream.Enable();
                    this._Kinect.Start();

                    ColorImageStream colorSteam = this._Kinect.ColorStream;
                    this._ColorImageBitmap = new WriteableBitmap(colorSteam.FrameWidth,
                                                                        colorSteam.FrameHeight,
                                                                        96, 96, PixelFormats.Bgr32,
                                                                        null);
                    this._ColorImageBitmapRect = new Int32Rect(0, 0, colorSteam.FrameWidth,
                                                                  colorSteam.FrameHeight);
                    this._ColorImageStride = colorSteam.FrameWidth *
                                                    colorSteam.FrameBytesPerPixel;
                    this.ColorImageElement.Source = this._ColorImageBitmap;
                    this._ColorImagePixelData = new byte[colorSteam.FramePixelDataLength];
                }
            }
        }

        private void PollColorImageStream()
        {
            if (this._Kinect == null)
            {
                MessageBox.Show("Plug me in!");
            }
            else
                try
                {
                    using (ColorImageFrame frame = this._Kinect.ColorStream.OpenNextFrame(100))
                    {
                        if (frame != null)
                        {
                            frame.CopyPixelDataTo(this._ColorImagePixelData);
                            this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect,
                                                               this._ColorImagePixelData,
                                                               this._ColorImageStride, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
        }
        #endregion Methods
    }
}
