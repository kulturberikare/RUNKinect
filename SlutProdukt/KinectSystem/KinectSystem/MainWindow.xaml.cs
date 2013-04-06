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
using System.Windows.Forms;

using Microsoft.Kinect;

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectSensorOne;
        private KinectSensor _KinectSensorTwo;
        private WriteableBitmap _ColorImageOne;
        private WriteableBitmap _ColorImageTwo;
        private Int32Rect _ColorImageRectOne;
        private Int32Rect _ColorImageRectTwo;
        private int _ColorImageStrideOne;
        private int _ColorImageStrideTwo;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensors(); };
            this.Unloaded += (s, e) => { KinectSensorOne = null; KinectSensorTwo = null; };
            this.Closed += MainWindow_Closed;
        }
        #endregion Constructor

        #region Methods
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (KinectSensorOne != null && KinectSensorOne.IsRunning)
            {
                KinectSensorOne.Stop();
            }

            if (KinectSensorTwo != null && KinectSensorTwo.IsRunning)
            {
                KinectSensorTwo.Stop();
            }
        }

        public void DiscoverKinectSensors()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            KinectSensor FirstKinect = KinectSensor.KinectSensors
                                          .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            KinectSensor LastKinect = KinectSensor.KinectSensors
                               .LastOrDefault(x => x.Status == KinectStatus.Connected);

            KinectSensorOne = FirstKinect;
            if (FirstKinect != null && LastKinect != null && FirstKinect != LastKinect)
            {
                KinectSensorTwo = LastKinect;
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            KinectSensor FirstKinect = KinectSensor.KinectSensors
                                                   .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            KinectSensor LastKinect = KinectSensor.KinectSensors
                                                   .LastOrDefault(x => x.Status == KinectStatus.Connected);
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.KinectSensorOne == null && FirstKinect != LastKinect && e.Sensor == FirstKinect)
                    {
                        KinectSensorOne = FirstKinect;
                    }
                    if (this.KinectSensorTwo == null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorTwo = LastKinect;
                    }
                    if (this.KinectSensorTwo != null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorOne = LastKinect;
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (KinectSensorOne == e.Sensor)
                    {
                        KinectSensorOne = null;
                    }
                    if (KinectSensorTwo == e.Sensor)
                    {
                        KinectSensorTwo = null;
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

                this._ColorImageOne = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ColorImageRectOne = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ColorImageStrideOne = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageOne.Source = this._ColorImageOne;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyOne;
                sensor.Start();
            }
        }

        private void InitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();

                this._ColorImageTwo = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ColorImageRectTwo = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ColorImageStrideTwo = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageTwo.Source = this._ColorImageTwo;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyTwo;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady -= Kinect_ColorFrameReadyOne;
            }
        }

        private void UninitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady -= Kinect_ColorFrameReadyTwo;
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
                    this._ColorImageOne.WritePixels(this._ColorImageRectOne,
                                                    pixelData, this._ColorImageStrideOne, 0);
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
                    this._ColorImageTwo.WritePixels(this._ColorImageRectTwo,
                                                    pixelData, this._ColorImageStrideTwo, 0);
                }
            }
        }

        private void BrowseButton1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Info1 = new OpenFileDialog();
            Info1.InitialDirectory = "c:\\";
            Info1.Filter = "(*.txt)|*.txt|(*.png)|*.png|(*.jpg)|*.jpg";
            Info1.RestoreDirectory = true;

            if (Info1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info1.FileName;
                InfoLabel1.Content = file;
            }
        }

        private void BrowseButton2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Info2 = new OpenFileDialog();
            Info2.InitialDirectory = "c:\\";
            Info2.Filter = "(*.txt)|*.txt|(*.png)|*.png|(*.jpg)|*.jpg";
            Info2.RestoreDirectory = true;

            if (Info2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info2.FileName;
                InfoLabel2.Content = file;
            }
        }
        private void Skeleton1_Checked(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void Skeleton2_Checked(object sender, RoutedEventArgs e)
        {
            return;
        }

        #endregion Methods

        #region Properties
        public KinectSensor KinectSensorOne
        {
            get
            {
                return this._KinectSensorOne;
            }
            set
            {
                if (this._KinectSensorOne != value)
                {
                    UninitializeKinectSensorOne(this._KinectSensorOne);
                    this._KinectSensorOne = null;
                }
                if (value != null && value.Status == KinectStatus.Connected)
                {
                    this._KinectSensorOne = value;
                    InitializeKinectSensorOne(this._KinectSensorOne);
                }
            }
        }

        public KinectSensor KinectSensorTwo
        {
            get
            {
                return this._KinectSensorTwo;
            }
            set
            {
                if (this._KinectSensorTwo != value)
                {
                    if (this._KinectSensorTwo != null)
                    {
                        UninitializeKinectSensorTwo(this._KinectSensorTwo);
                        this._KinectSensorTwo = null;
                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._KinectSensorTwo = value;
                        InitializeKinectSensorTwo(this._KinectSensorTwo);
                    }
                }
            }
        }
        #endregion Properties
    }
}
