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

namespace TakingMeasureAB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private short[] _DepthPixelData;
        private int _DepthImageStride;
        private WriteableBitmap _DepthImage;
        private Int32Rect _DepthImageRect;

        private const int LoDepthThreshold = 1220;
        private const int HiDepthThreshold = 3048;
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

                this._DepthImage = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this._DepthImageRect = new Int32Rect(0, 0, (int)Math.Ceiling(this._DepthImage.Width), (int)Math.Ceiling(this._DepthImage.Height));
                this._DepthImageStride = depthStream.FrameWidth * 4;
                this._DepthPixelData = new short[depthStream.FramePixelDataLength];
                this.DepthImage.Source = this._DepthImage;                           


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
                    CalculatePlayerSize(frame, this._DepthPixelData);
                }
            }
        }

        private void CreateBetterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;
            int gray;
            int bytesPerPixel = 4;
            byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];

            for (int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth < LoDepthThreshold || depth > HiDepthThreshold)
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

            this._DepthImage.WritePixels(this._DepthImageRect, enhPixelData, this._DepthImageStride, 0);
        }

        private void CalculatePlayerSize(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;
            int playerIndex;
            int pixelIndex;
            int bytesPerPixel = depthFrame.BytesPerPixel;
            PlayerDepthData[] players = new PlayerDepthData[6];

            for (int row = 0; row < depthFrame.Height; row++)
            {
                for (int col = 0; col < depthFrame.Width; col++)
                {
                    pixelIndex = col + (row * depthFrame.Width);
                    depth = pixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if (depth != 0)
                    {
                        playerIndex = (pixelData[pixelIndex] & DepthImageFrame.PlayerIndexBitmask);
                        playerIndex -= 1;

                        if (playerIndex > -1)
                        {
                            if (players[playerIndex] == null)
                            {
                                players[playerIndex] = new PlayerDepthData(playerIndex + 1,
                                                       depthFrame.Width, depthFrame.Height);
                            }

                            players[playerIndex].UpdateData(col, row, depth);
                        }
                    }
                }
            }

            PlayerDepthData.ItemsSource = players;
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

    public class PlayerDepthData
    {
        #region Member Variables
        private const double MillimetersPerInch = 0.0393700787;
        private static readonly double HorizontalTanA = Math.Tan(28.5 * Math.PI / 180);
        private static readonly double VerticalTanA = Math.Abs(Math.Tan(21.5 * Math.PI / 180));

        private int _DepthSum;
        private int _DepthCount;
        private int _LoWidth;
        private int _HiWidth;
        private int _LoHeight;
        private int _HiHeight;
        #endregion Member Variables

        #region Constructor
        public PlayerDepthData(int playerID, double frameWidth, double frameHeight)
        {
            this.PlayerID = playerID;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
            this._LoWidth = int.MaxValue;
            this._HiWidth = int.MinValue;
            this._LoHeight = int.MaxValue;
            this._HiHeight = int.MinValue;
        }
        #endregion Constructor

        #region Methods
        public void UpdateData(int x, int y, int depth)
        {
            this._DepthCount++;
            this._DepthSum += depth;
            this._LoWidth = Math.Min(this._LoWidth, x);
            this._HiWidth = Math.Max(this._HiWidth, x);
            this._LoHeight = Math.Min(this._LoHeight, y);
            this._HiHeight = Math.Max(this._HiHeight, y);
        }
        #endregion Methods

        #region Properties
        public int PlayerID { get; private set; }
        public double FrameWidth { get; private set; }
        public double FrameHeight { get; private set; }

        public double Depth
        {
            get { return this._DepthSum / (double)this._DepthCount; }
        }

        public int PixelWidth
        {
            get { return this._HiWidth - this._LoWidth; }
        }

        public int PixelHeight
        {
            get { return this._HiHeight - this._LoHeight; }
        }

        public double RealWidth
        {
            get
            {
                double opposite = this.Depth * HorizontalTanA;
                return this.PixelWidth * 2 * opposite / this.FrameWidth * MillimetersPerInch;
            }
        }

        public double RealHeight
        {
            get
            {
                double opposite = this.Depth * VerticalTanA;
                return this.PixelHeight * 2 * opposite / this.FrameHeight * MillimetersPerInch;
            }
        }
        #endregion Properties
    }
}
