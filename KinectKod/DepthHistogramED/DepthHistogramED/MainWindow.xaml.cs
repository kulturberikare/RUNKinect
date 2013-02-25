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

namespace DepthHistogramED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        //private DepthImageFrame _LastDepthFrame;
        private short[] _DepthPixelData;
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
                DepthImageStream depthStream = sensor.DepthStream;

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
                    frame.CopyPixelDataTo(this._DepthPixelData);
                    CreateBetterShadesOfGray(frame, this._DepthPixelData);
                    CreateDepthHistogram(frame, this._DepthPixelData);
                }
            }
        }

        private void CreateBetterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;
            int gray;
            int loThreshold = 1220;
            int hiThreshold = 3048;
            int bytesPerPixel = 4;
            byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];

            for (int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth < loThreshold || depth > hiThreshold)
                {
                    gray = 0xFF;
                }
                else
                {
                    gray = (255 * depth / 0xFFF);
                }

                enhPixelData[j] = (byte)gray;
                enhPixelData[j + 1] = (byte)gray;
                enhPixelData[j + 2] = (byte)gray;
            }
        }

        private void CreateDepthHistogram(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;
            int[] depths = new int[4096];
            int maxValue = 0;
            double chartBarWidth = DepthHistogram.ActualWidth / depths.Length;

            DepthHistogram.Children.Clear();

            for (int i = 0; i < pixelData.Length; i += depthFrame.BytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth != 0)
                {
                    depths[depth]++;
                }
            }

            for (int i = 0; i < depths.Length; i++)
            {
                maxValue = Math.Max(maxValue, depths[i]);
            }

            for (int i = 0; i < depths.Length; i++)
            {
                Rectangle r = new Rectangle();
                r.Fill = Brushes.Black;
                r.Width = chartBarWidth;
                r.Height = DepthHistogram.ActualHeight *
                           (depths[i] / (double)maxValue);
                r.Margin = new Thickness(1, 0, 1, 0);
                r.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                DepthHistogram.Children.Add(r);
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

        #endregion Properties
    }
}
