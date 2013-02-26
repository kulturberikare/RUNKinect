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

namespace DepthPlayerIndexingAB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private short[] _RawDepthPixelData;
        private int _RawDepthImageStride;
        private WriteableBitmap _EnhDepthImage;
        private Int32Rect _EnhDepthImageRect;
        private short[] _EnhDepthPixelData;
        private int _EnhDepthImageStride;
        #endregion Member Variables

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
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }


        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
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
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        if (this.Kinect == null)
                        {
                            MessageBox.Show("No connected sensor!");
                        }
                    }
                    break;

                //TODO: Handle all other statuses according to needs
            }
            if (e.Status == KinectStatus.Connected)
            {
                this.Kinect = e.Sensor;
            }
        }

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                this._Kinect.SkeletonStream.Enable();
                this._Kinect.DepthStream.Enable();   

                DepthImageStream depthStream = this._Kinect.DepthStream;
                this._RawDepthImage = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                this._RawDepthImageRect = new Int32Rect(0, 0, (int)Math.Ceiling(this._RawDepthImage.Width), (int)Math.Ceiling(this._RawDepthImage.Height));
                this._RawDepthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                this._RawDepthPixelData = new short[depthStream.FramePixelDataLength];
                this.RawDepthImage.Source = this._RawDepthImage;

                this._EnhDepthImage = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this._EnhDepthImageRect = new Int32Rect(0, 0, (int)Math.Ceiling(this._EnhDepthImage.Width), (int)Math.Ceiling(this._EnhDepthImage.Height));
                this._EnhDepthImageStride = depthStream.FrameWidth * 4;
                this._EnhDepthPixelData = new short[depthStream.FramePixelDataLength];
                this.EnhDepthImage.Source = this._EnhDepthImage;      

                depthStream.Enable();

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

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    frame.CopyPixelDataTo(this._RawDepthPixelData);
                    this._RawDepthImage.WritePixels(this._RawDepthImageRect, this._RawDepthPixelData,
                                                    this._RawDepthImageStride, 0);
                    CreatePlayerDepthImage(frame, this._RawDepthPixelData);
                }
            }
        }

        private void CreatePlayerDepthImage(DepthImageFrame depthFrame, short[] pixelData)
        {
            int playerIndex;
            int depthBytePerPixel = 4;
            byte[] enhPixelData = new byte[depthFrame.Height * this._EnhDepthImageStride];

            for (int i = 0, j = 0; i < pixelData.Length; i++, j += depthBytePerPixel)
            {
                playerIndex = pixelData[i] & DepthImageFrame.PlayerIndexBitmask;

                if (playerIndex == 0)
                {
                    enhPixelData[j] = 0xFF;
                    enhPixelData[j + 1] = 0xFF;
                    enhPixelData[j + 2] = 0xFF;
                }
                else
                {
                    enhPixelData[j] = 0x00;
                    enhPixelData[j + 1] = 0x00;
                    enhPixelData[j + 2] = 0x00;
                }
            }

            this._EnhDepthImage.WritePixels(this._EnhDepthImageRect, enhPixelData,
                                            this._EnhDepthImageStride, 0);
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

        #endregion Properties
    }
}
