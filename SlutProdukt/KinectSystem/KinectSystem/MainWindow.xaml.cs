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
        private WriteableBitmap _ImageOne;
        private WriteableBitmap _ImageTwo;
        private Int32Rect _ImageRectOne;
        private Int32Rect _ImageRectTwo;
        private int _ImageStrideOne;
        private int _ImageStrideTwo;

        private MainWindowViewModel viewModel;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensors(); };
            this.Unloaded += (s, e) => { KinectSensorOne = null; KinectSensorTwo = null; };
            this.Closed += MainWindow_Closed;
            this.viewModel = new MainWindowViewModel();
            this.viewModel.CanStartOne = false;
            this.viewModel.CanStopOne = false;
            this.viewModel.CanStartTwo = false;
            this.viewModel.CanStopTwo = false;
            this.DataContext = this.viewModel;


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

            if (FirstKinect != null)
            {
                KinectSensorOne = FirstKinect;
            }

            if (FirstKinect != null && LastKinect != null && FirstKinect != LastKinect)
            {
                KinectSensorTwo = LastKinect;
            }

            SetKinectData();
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
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    if (this.KinectSensorTwo == null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorTwo = LastKinect;
                        this.viewModel.SensorstatusTwo = e.Status.ToString();
                    }
                    if (this.KinectSensorTwo != null && this.KinectSensorOne == null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorOne = LastKinect;
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (KinectSensorOne == e.Sensor)
                    {
                        KinectSensorOne = null;
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    if (KinectSensorTwo == e.Sensor)
                    {
                        KinectSensorTwo = null;
                        this.viewModel.SensorstatusTwo = e.Status.ToString();
                    }
                    break;
            }
        }

        private void SetKinectData()
        {
            if (KinectSensorOne != null)
            {
                this.viewModel.ConectionIDOne = this.KinectSensorOne.DeviceConnectionId;
                this.viewModel.DeviceIDOne = this.KinectSensorOne.UniqueKinectId;
                this.viewModel.SensorstatusOne = this.KinectSensorOne.Status.ToString();
                this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
                this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
                this.viewModel.IsSkeletonStreamEnabledOne = this.KinectSensorOne.SkeletonStream.IsEnabled;
                this.viewModel.SensorAngleOne = this.KinectSensorOne.ElevationAngle;
            }

            if (KinectSensorTwo != null)
            {
                this.viewModel.ConectionIDTwo = this.KinectSensorTwo.DeviceConnectionId;
                this.viewModel.DeviceIDTwo = this.KinectSensorTwo.UniqueKinectId;
                this.viewModel.SensorstatusTwo = this.KinectSensorTwo.Status.ToString();
                this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorTwo.ColorStream.IsEnabled;
                this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorTwo.DepthStream.IsEnabled;
                this.viewModel.IsSkeletonStreamEnabledTwo = this.KinectSensorTwo.SkeletonStream.IsEnabled;
                this.viewModel.SensorAngleTwo = this.KinectSensorTwo.ElevationAngle;
            }
        }

        private void StartColorImageOne(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsColorStreamEnabledOne)
            {
                KinectSensor sensor = KinectSensorOne;

                StopDepthImageOne(sensor);

                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();

                this._ImageOne = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ImageRectOne = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ImageStrideOne = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageOne.Source = this._ImageOne;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyOne;
                this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
            }
        }

        private void StartColorImageTwo(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsColorStreamEnabledTwo)
            {
                KinectSensor sensor = KinectSensorTwo;

                StopDepthImageTwo(sensor);

                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();

                this._ImageTwo = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ImageRectTwo = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ImageStrideTwo = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageTwo.Source = this._ImageTwo;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyTwo;
                this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorOne.ColorStream.IsEnabled;
            }
        }

        private void StopColorImageOne(KinectSensor sensor)
        {
            sensor.ColorFrameReady -= Kinect_ColorFrameReadyOne;
            sensor.ColorStream.Disable();
            this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
        }

        private void StopColorImageTwo(KinectSensor sensor)
        {
            sensor.ColorFrameReady -= Kinect_ColorFrameReadyTwo;
            sensor.ColorStream.Disable();
            this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorOne.ColorStream.IsEnabled;
        }

        private void StartDepthImageOne(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsDepthStreamEnabledOne)
            {
                KinectSensor sensor = KinectSensorOne;

                StopColorImageOne(sensor);
                
                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._ImageOne = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._ImageRectOne = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._ImageStrideOne = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                SightImageOne.Source = this._ImageOne;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyOne;
                this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
            }
        }

        private void StartDepthImageTwo(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsDepthStreamEnabledTwo)
            {
                KinectSensor sensor = KinectSensorTwo;

                StopColorImageTwo(sensor);

                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._ImageTwo = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._ImageRectTwo = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._ImageStrideTwo = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                SightImageTwo.Source = this._ImageTwo;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyTwo;
                this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorOne.DepthStream.IsEnabled;
            }
        }

        private void StopDepthImageOne(KinectSensor sensor)
        {
            sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
            sensor.DepthStream.Disable();
            this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
        }

        private void StopDepthImageTwo(KinectSensor sensor)
        {
            sensor.DepthFrameReady -= Kinect_DepthFrameReadyTwo;
            sensor.DepthStream.Disable();
            this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorOne.DepthStream.IsEnabled;
        }

        private void InitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                SightImageOne.Source = this._ImageOne;
                sensor.Start();
            }
        }

        private void InitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                SightImageTwo.Source = this._ImageTwo;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                if (this.viewModel.IsColorStreamEnabledOne)
                {
                    StopColorImageOne(sensor);
                }

                if (this.viewModel.IsDepthStreamEnabledOne)
                {
                    sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
                }
            }
        }

        private void UninitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                if (this.viewModel.IsColorStreamEnabledOne)
                {
                    sensor.ColorFrameReady -= Kinect_ColorFrameReadyOne;
                }

                if (this.viewModel.IsDepthStreamEnabledOne)
                {
                    sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
                }
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
                    this._ImageOne.WritePixels(this._ImageRectOne,
                                                    pixelData, this._ImageStrideOne, 0);
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
                    this._ImageTwo.WritePixels(this._ImageRectTwo,
                                                    pixelData, this._ImageStrideTwo, 0);
                }
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
                    this._ImageOne.WritePixels(this._ImageRectOne,
                                                   pixelData, this._ImageStrideOne, 0);
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
                    this._ImageTwo.WritePixels(this._ImageRectTwo,
                                                   pixelData, this._ImageStrideTwo, 0);
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
