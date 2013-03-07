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
using System.IO;

namespace DotsOnTheDeep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private Skeleton[] _FrameSkeletons;
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private int _RawDepthImageStride;
        #endregion Member Variables

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();


            this.Loaded += (s, e) =>
           {
               KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
               this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
           };


        }


        #endregion Constructor

        #region Methods
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                case KinectStatus.Initializing:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;

                case KinectStatus.Disconnected:
                    MessageBox.Show("Plug in Kinect Device!");
                    this.KinectDevice = null;
                    break;

                default:
                    MessageBox.Show("//TODO Show an error state");
                    break;
            }
        }

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._RawDepthImage.WritePixels(this._RawDepthImageRect,
                                                    pixelData, this._RawDepthImageStride, 0);
                }
            }
        }


        #endregion Methods

        #region Properties
        public KinectSensor KinectDevice
        {
            get { return this._KinectDevice; }
            set
            {
                if (this._KinectDevice != value)
                {
                    //Uninitialize
                    if (this._KinectDevice != null)
                    {
                        this._KinectDevice.Stop();
                        this._KinectDevice.SkeletonStream.Disable();
                        this._KinectDevice.DepthStream.Disable();
                        SkeletonViewerElement.KinectDevice = null;
                        this._FrameSkeletons = null;

                        this._KinectDevice.DepthFrameReady -= Kinect_DepthFrameReady;
                    }

                    this._KinectDevice = value;

                    //Initialize
                    if (this._KinectDevice != null)
                    {
                        if (this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            this._KinectDevice.SkeletonStream.Enable();
                            DepthImageStream depthStream = this._KinectDevice.DepthStream;
                            depthStream.Enable();

                            this._RawDepthImage = new WriteableBitmap(depthStream.FrameWidth,
                                depthStream.FrameHeight, 96, 96,
                                PixelFormats.Gray16, null);
                            this._RawDepthImageRect = new Int32Rect(0, 0, depthStream.FrameWidth,
                                depthStream.FrameHeight);
                            this._RawDepthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                            DepthImage.Source = this._RawDepthImage;
                            this._FrameSkeletons = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this._KinectDevice.Start();

                            this._KinectDevice.DepthFrameReady += Kinect_DepthFrameReady;

                            SkeletonViewerElement.KinectDevice = this.KinectDevice;
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}