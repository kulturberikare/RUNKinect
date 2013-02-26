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
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private int _RawDepthImagStride;
        private DepthImageFrame _LastDepthFrame;
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

                // In case of more statuschanges, add here.
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
                this._LastDepthFrame.CopyPixelDataTo(_DepthImagePixelData);
                this._RawDepthImage.WritePixels(this._RawDepthImageRect, this._DepthImagePixelData,
                                                this._RawDepthImagStride, 0);
                
                CreateBetterShadesOfGray(_LastDepthFrame, _DepthImagePixelData);
            }
            
        }

        private void DepthImage_MoueLeftButtomUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DepthImage);
            if (this._DepthImagePixelData != null && this._DepthImagePixelData.Length > 0)
            {
                int pixelIndex = (int)(p.X + ((int)p.Y * this._LastDepthFrame.Width));
                int depth = this._DepthImagePixelData[pixelIndex] >>
                            DepthImageFrame.PlayerIndexBitmaskWidth;
                int depthInches = (int)(depth * 0.0393700787);
                int depthFt = depthInches / 12;
                depthInches = depthInches % 12;

                PixelDepth.Text = string.Format("{0}mm ~ {1}'{2}\"", depth, depthFt, depthInches);
            }
        }

        //private void CreateColorDepthImage(DepthImageFrame depthFrame, short[] pixelData)
        //{
        //    int depth;
        //    double hue;
        //    int loThreshold = 1220;
        //    int hiThreshold = 3048;
        //    int bytesPerPixel = 4;
        //    byte[] rgb = new byte[3];
        //    byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];

        //    for (int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
        //    {
        //        depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

        //        if (depth < loThreshold || depth > hiThreshold)
        //        {
        //            enhPixelData[j] = 0x00;
        //            enhPixelData[j + 1] = 0x00;
        //            enhPixelData[j + 2] = 0x00;
        //        }
        //        else
        //        {
        //            hue= ((360 * depth / 0xFFF) + loThreshold);
        //            ConvertHslToRgb(hue, 100, 100, rgb);

        //            enhPixelData[j] = rgb[2]; //Blue
        //            enhPixelData[j + 1] = rgb[1]; //Green
        //            enhPixelData[j + 2] = rgb[0] //Red
        //        }
        //    }

        //    EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
        //                                                    96, 96, PixelFormats.Bgr32, null,
        //                                                    enhPixelData,
        //                                                    depthFrame.Width *
        //                                                    bytesPerPixel);
        //}

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
                    gray = (255 * depth / 0xFF);
                }

                enhPixelData[j] = (byte)gray;
                enhPixelData[j + 1] = (byte)gray;
                enhPixelData[j + 2] = (byte)gray;
            }

            EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                                                            96, 96, PixelFormats.Bgr32, null,
                                                            enhPixelData,
                                                            depthFrame.Width *
                                                            bytesPerPixel);
        }

        //private void CreateLighterSahdesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        //{
        //    int depth;
        //    int loThreshold = 1220;
        //    int hiThreshold = 3048;
        //    short[] enhPixelData = new short[depthFrame.Width * depthFrame.Height];

        //    for (int i = 0; i < pixelData.Length; i++)
        //    {
        //        depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
        //        if (depth < loThreshold || depth > hiThreshold)
        //        {
        //            enhPixelData[i] = 0xFF;
        //        }
        //        else
        //        {
        //            enhPixelData[i] = (short)~pixelData[i];
        //        }
        //    }

        //    EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
        //                                                    96, 96, PixelFormats.Gray16, null,
        //                                                    enhPixelData,
        //                                                    depthFrame.Width * 
        //                                                    depthFrame.BytesPerPixel);
        //}

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._RawDepthImage = new WriteableBitmap(depthStream.FrameWidth,
                                                          depthStream.FrameHeight, 96, 96,
                                                           PixelFormats.Gray16, null);
                this._RawDepthImageRect = new Int32Rect(0, 0, depthStream.FrameWidth,
                                                        depthStream.FrameHeight);
                this._RawDepthImagStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                DepthImage.Source = this._RawDepthImage;
                this._DepthImagePixelData = new short[depthStream.FramePixelDataLength];

                MessageBox.Show("Writeble bitmap data:\n" + this._RawDepthImage.PixelWidth.ToString()
                                + "x" + this._RawDepthImage.PixelHeight.ToString() + "\nRect data:\n"
                                + _RawDepthImageRect.Width.ToString() + "x" + _RawDepthImageRect.Height.ToString()
                                + "\nArray pixel data:\nLength:" + _DepthImagePixelData.Length.ToString()
                                + "\nStride data: \nStride length: " + this._RawDepthImagStride.ToString());

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
