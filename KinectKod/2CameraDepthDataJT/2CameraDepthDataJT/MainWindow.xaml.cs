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

namespace _2CameraDepthDataJT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectOne;
        private KinectSensor _KinectTwo;
        private WriteableBitmap _RawDepthImageOne;
        private WriteableBitmap _RawDepthImageTwo;
        private Int32Rect _RawDepthImageRectOne;
        private Int32Rect _RawDepthImageRectTwo;
        private int _RawDepthImageStrideOne;
        private int _RawDepthImageStrideTwo;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this._KinectOne = null; this._KinectTwo = null; };
        }
        #endregion Constructor

        #region Methods
        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            KinectOne = KinectSensor.KinectSensors
                                    .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            if (KinectOne == null)
            {
                MessageBox.Show("No Kinect One Conected!");
            }
            else
            {
                MessageBox.Show("Kinect one conected!");
            }
            if (KinectSensor.KinectSensors.Count >= 2
                && KinectSensor.KinectSensors
                               .FirstOrDefault(x => x.Status == KinectStatus.Connected)
                != KinectSensor.KinectSensors
                               .LastOrDefault(x => x.Status == KinectStatus.Connected))
            {
                KinectTwo = KinectSensor.KinectSensors
                                        .LastOrDefault(x => x.Status == KinectStatus.Connected);
                MessageBox.Show("Kinect two Conected!");
            }
            else
            {
                MessageBox.Show("No Kinect two conected!");
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.KinectOne == null
                        && KinectSensor.KinectSensors
                                       .FirstOrDefault(x => x.Status == KinectStatus.Connected)
                        != KinectSensor.KinectSensors
                                       .LastOrDefault(x => x.Status == KinectStatus.Connected)
                        && e.Sensor == KinectSensor.KinectSensors
                                                   .FirstOrDefault(x => x.Status == KinectStatus.Connected))
                    {
                        this.KinectOne = e.Sensor;
                        MessageBox.Show("Kinect one connected!");
                    }
                    if (this.KinectTwo == null
                        && KinectSensor.KinectSensors.Count >= 2
                        && KinectSensor.KinectSensors
                                       .FirstOrDefault(x => x.Status == KinectStatus.Connected)
                        != KinectSensor.KinectSensors
                                       .LastOrDefault(x => x.Status == KinectStatus.Connected)
                        && e.Sensor == KinectSensor.KinectSensors
                                                   .LastOrDefault(x => x.Status == KinectStatus.Connected)
                        )
                    {
                        this.KinectTwo = e.Sensor;
                        MessageBox.Show("Kinect two connected");
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.KinectOne == e.Sensor)
                    {
                        this.KinectOne = null;
                        if (KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected)
                            != KinectSensor.KinectSensors.LastOrDefault(x => x.Status == KinectStatus.Connected))
                        {
                            this.KinectOne = KinectSensor.KinectSensors
                                                         .FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        } 
                        if (this.KinectOne == null)
                        {
                            MessageBox.Show("Camera 1 dissconnected");
                        }
                    }
                    if (this.KinectTwo == e.Sensor)
                    {
                        this.KinectTwo = null;

                        if (KinectSensor.KinectSensors.Count >= 2 
                            && KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected)
                            != KinectSensor.KinectSensors.LastOrDefault(x => x.Status == KinectStatus.Connected))
                        {
                            this.KinectTwo = KinectSensor.KinectSensors
                                                         .LastOrDefault(x => x.Status == KinectStatus.Connected);
                        }
                        if (this.KinectTwo == null)
                        {
                            MessageBox.Show("Camera 2 dissconnected");
                        }
                    }
                    break;
            }
        }

        private void InitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._RawDepthImageOne = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._RawDepthImageRectOne = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._RawDepthImageStrideOne = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthImageOne.Source = this._RawDepthImageOne;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyOne;
                sensor.Start();
            }
        }

        private void InitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._RawDepthImageTwo = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._RawDepthImageRectTwo = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._RawDepthImageStrideTwo = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthImageTwo.Source = this._RawDepthImageTwo;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyTwo;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
            }
        }

        private void UninitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.DepthFrameReady -= Kinect_DepthFrameReadyTwo;
            }
        }

        private void Kinect_DepthFrameReadyOne(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._RawDepthImageOne.WritePixels(this._RawDepthImageRectOne,
                                                       pixelData, this._RawDepthImageStrideOne, 0);
                }
            }
        }

        private void Kinect_DepthFrameReadyTwo(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._RawDepthImageTwo.WritePixels(this._RawDepthImageRectTwo,
                                                       pixelData, this._RawDepthImageStrideTwo, 0);
                }
            }
        }

        #endregion Methods

        #region Properties
        public KinectSensor KinectOne
        {
            get
            {
                return _KinectOne;
            }

            set
            {
                if (this._KinectOne != value)
                {
                    if (this._KinectOne != null)
                    {
                        UninitializeKinectSensorOne(this._KinectOne);
                        this._KinectOne = null;
                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._KinectOne = value;
                        InitializeKinectSensorOne(this._KinectOne);
                    }
                }
            }
        }

        public KinectSensor KinectTwo
        {
            get
            {
                return _KinectTwo;
            }

            set
            {
                if (this._KinectTwo != value)
                {
                    if (this._KinectTwo != null)
                    {
                        UninitializeKinectSensorTwo(this._KinectTwo);
                        this._KinectTwo = null;
                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._KinectTwo = value;
                        InitializeKinectSensorTwo(this._KinectTwo);
                    }
                }
            }
        }

        #endregion Properties
    }
}
