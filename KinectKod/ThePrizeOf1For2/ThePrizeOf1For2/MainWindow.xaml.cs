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

namespace ThePrizeOf1For2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectOne;
        private KinectSensor _KinectTwo;
        private WriteableBitmap _ColorImageBitmapOne;
        private Int32Rect _ColorImageBitmapRectOne;
        private int _ColorImageStrideOne;
        private WriteableBitmap _ColorImageBitmapTwo;
        private Int32Rect _ColorImageBitmapRectTwo;
        private int _ColorImageStrideTwo;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensors(); };
            this.Unloaded += (s, e) => { this.KinectOne = null; this.KinectTwo = null; };
        }
        #endregion Constructor

        # region Methods
        private void DiscoverKinectSensors()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            if (KinectSensor.KinectSensors[0].Status == KinectStatus.Connected)
            {
                KinectOne = KinectSensor.KinectSensors[0];
                MessageBox.Show("Kinect1");
            }
            if (KinectSensor.KinectSensors[1].Status == KinectStatus.Connected)
            {
                KinectTwo = KinectSensor.KinectSensors[1];
                MessageBox.Show("Kinect2");
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.KinectOne == null && e.Sensor == KinectSensor.KinectSensors[0])
                    {
                        this.KinectOne = e.Sensor;
                    }
                    if (this.KinectTwo == null && e.Sensor == KinectSensor.KinectSensors[1])
                    {
                        this.KinectTwo = e.Sensor;
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.KinectOne == e.Sensor && e.Sensor == KinectSensor.KinectSensors[0])
                    {
                        this.KinectOne = null;
                        if (KinectSensor.KinectSensors[0].Status == KinectStatus.Connected)
                        {
                            this.KinectOne = KinectSensor.KinectSensors[0];
                        }

                        if (this.KinectOne == null)
                        {
                            MessageBox.Show("Kinect1 dissconected!");
                        }
                    }
                    if (this.KinectTwo == null && e.Sensor == KinectSensor.KinectSensors[1])
                    {
                        this.KinectTwo = null;
                        if (KinectSensor.KinectSensors[1].Status == KinectStatus.Connected)
                        {
                            this.KinectTwo = KinectSensor.KinectSensors[1];
                        }

                        if (this.KinectTwo == null)
                        {
                            MessageBox.Show("Kinect2 dissconected!");
                        }
                    }
                    break;
            }
        }

        private void InitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();
                this._ColorImageBitmapOne = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight,
                                                                96, 96, PixelFormats.Bgr32, null);
                this._ColorImageBitmapRectOne = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                this._ColorImageStrideOne = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorImageOne.Source = this._ColorImageBitmapOne;
                sensor.ColorFrameReady += Kinect_ColorFrameReadyOne;
                sensor.Start();
                MessageBox.Show("Started kinect1");
            }
        }

        private void InitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();
                this._ColorImageBitmapTwo = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight,
                                                                96, 96, PixelFormats.Bgr32, null);
                this._ColorImageBitmapRectTwo = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                this._ColorImageStrideTwo = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorImageTwo.Source = this._ColorImageBitmapTwo;
                sensor.ColorFrameReady += Kinect_ColorFrameReadyTwo;
                sensor.Start();
                MessageBox.Show("Started kinect2");
            }
        }

        private void Kinect_ColorFrameReadyOne(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ColorImageBitmapOne.WritePixels(this._ColorImageBitmapRectOne, pixelData,
                                                          this._ColorImageStrideOne, 0);
                }
            }
        }

        private void Kinect_ColorFrameReadyTwo(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ColorImageBitmapTwo.WritePixels(this._ColorImageBitmapRectTwo, pixelData,
                                                          this._ColorImageStrideTwo, 0);
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
                        //uninitialize Kinect sensor
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
                        //uninitialize Kinect sensor
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
