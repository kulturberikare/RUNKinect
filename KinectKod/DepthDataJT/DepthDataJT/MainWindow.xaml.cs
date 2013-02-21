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

namespace DepthDataJT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        #region Members Variables
        private KinectSensor _Kinect;
        private DepthImageFrame _LastDepthFrame;
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private int _RawDepthImageStride;
        private short[] _DepthImagePixelData;
        #endregion Members Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this.Kinect = null; };
        }
        #endregion Constructor

        #region Methods
        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensor_StstusChanged;
            this.Kinect = KinectSensor.KinectSensors
                                      .FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void KinectSensor_StstusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                    }
                    break;

                case KinectStatus.Disconnected:
                    if (this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors
                                                  .FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        if (this.Kinect == null)
                        {
                            MessageBox.Show("No connected sensor!");
                        }
                    }
                    break;

                // In case of more statuschanges, add here:
            }
        }

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (this._LastDepthFrame != null)
            {
                this._LastDepthFrame.Dispose();
                this._LastDepthFrame = null;
            }

            this._LastDepthFrame = e.OpenDepthImageFrame();

            if (this._LastDepthFrame != null)
            {
                this._LastDepthFrame.CopyPixelDataTo(this._DepthImagePixelData);
                this._RawDepthImage.WritePixels(this._RawDepthImageRect, this._DepthImagePixelData,
                                                               this._RawDepthImageStride, 0);
            }
        }

        private void DepthImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DepthImage);

            if (this._DepthImagePixelData != null && this._DepthImagePixelData.Length > 0)
            {
                int pixelIndex  = (int)(p.X + ((int)p.Y * this._LastDepthFrame.Width));
                int depth       = this._DepthImagePixelData[pixelIndex] >>
                                 DepthImageFrame.PlayerIndexBitmaskWidth;
                int depthInches = (int)(depth * 0.0393700787);
                int depthFt     = depthInches / 12;
                depthInches     = depthInches % 12;

                PixelDepth.Text = string.Format("{0}mm ~ {1]'{2}\"", depth, depthFt, depthInches);
            }
        }


        #endregion Methods

        #region Properties
        public KinectSensor Kinect
        {
            get
            {
                return this._Kinect;
            }

            set
            {
                if (this._Kinect != value)
                {
                    if (this._Kinect != null)
                    {
                        UninitializeKinectSensor(this.Kinect);
                        this._Kinect = null;
                    }

                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._Kinect = value;
                        InitializeKinectSensor(this.Kinect);
                    }
                }
            }
        }

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();
                this._RawDepthImage = new WriteableBitmap(depthStream.FrameWidth,
                                                          depthStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._RawDepthImageRect = new Int32Rect(0, 0, depthStream.FrameWidth,
                                                        depthStream.FrameHeight);
                this._RawDepthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthImage.Source = this._RawDepthImage;
                this._DepthImagePixelData = new short[depthStream.FramePixelDataLength];
                sensor.DepthFrameReady += Kinect_DepthFrameReady;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.DepthFrameReady -= Kinect_DepthFrameReady;
            }
        }

        #endregion Properties
    }
}
