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

namespace Imagedata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            //InitializeComponent();

            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this.Kinect = null; };
        }

        #endregion Constructor

        #region Methods
        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();
                this._ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,
                                                             colorStream.FrameHeight, 96, 96,
                                                             PixelFormats.Bgr32, null);
                this._ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                         colorStream.FrameHeight);
                this._ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                VideoStreamElement.Source = this._ColorImageBitmap;
                sensor.ColorFrameReady += Kinect_ColorFrameReady;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady -= Kinect_ColorFrameReady;
            }

        }

        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors
                .FirstOrDefault(x => x.Status == KinectStatus.Connected);
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
                        this.Kinect = KinectSensor.KinectSensors
                            .FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        if (this.Kinect == null)
                        {
                            MessageBox.Show("The sensor is dissconnected!");
                        }
                    }
                    break;

                //Handle all other statuses according to need

            }
        }

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);

                    for (int i = 0; i < pixelData.Length; i += frame.BytesPerPixel)
                    {

                        ////Shades of Red                    
                        //    pixelData[i]        = 0x00;     //Blue
                        //    pixelData[i + 1]    = 0x00;     //Green

                        ////Inverted Colors
                        //pixelData[i]        = (byte)~pixelData[i];
                        //pixelData[i + 1]    = (byte)~pixelData[i + 1];
                        //pixelData[i + 2]    = (byte)~pixelData[i + 2];

                        //Apocalyptic Zombie
                        pixelData[i] = pixelData[i + 1];
                        pixelData[i + 1] = pixelData[i];
                        pixelData[i + 2] = (byte)~pixelData[i + 2];

                        ////Gray Scale
                        //byte gray = Math.Max(pixelData[i], pixelData[i + 1]);
                        //gray = Math.Max(gray, pixelData[i + 2]);
                        //pixelData[i] = gray;
                        //pixelData[i + 2] = gray;
                        //pixelData[i + 2] = gray;

                        ////Black 'n' Withe Movie
                        //byte gray = Math.Min(pixelData[i], pixelData[i + 1]);
                        //gray = Math.Min(gray, pixelData[i + 2]);
                        //pixelData[i] = gray;
                        //pixelData[i + 1] = gray;
                        //pixelData[i + 2] = gray;

                        ////Washed out Colors
                        //double gray         = (pixelData[i] * 0.11) + 
                        //                      (pixelData[i + 1] * 0.59) + 
                        //                      (pixelData[i + 2] * 0.3);
                        //double desaturation = 0.75;
                        //pixelData[i] = (byte)(pixelData[i] + desaturation * (gray - pixelData[i]));
                        //pixelData[i + 1] = (byte)(pixelData[i + 1] + desaturation * (gray - pixelData[i + 1]));
                        //pixelData[i + 2] = (byte)(pixelData[i + 2] + desaturation * (gray - pixelData[i + 2]));

                        ////High saturation
                        //if (pixelData[i] < 0x33 || pixelData[i] > 0xE5)
                        //{
                        //    pixelData[i] = 0x00;
                        //}
                        //else
                        //{
                        //    pixelData[i] = 0xFF;
                        //}

                        //if (pixelData[i + 1] < 0x33 || pixelData[i + 1] > 0xE5)
                        //{
                        //    pixelData[i + 1] = 0x00;
                        //}
                        //else
                        //{
                        //    pixelData[i + 1] = 0xFF;
                        //}

                        //if (pixelData[i + 2] < 0x33 || pixelData[i + 2] > 0xE5)
                        //{
                        //    pixelData[i + 2] = 0x00;
                        //}
                        //else
                        //{
                        //    pixelData[i + 2] = 0xFF;
                        //}
                    }


                    this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData,
                                                       this._ColorImageStride, 0);

                }
            }
        }

        private void TakePictureButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "snapshot.jpg";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (FileStream savedSnapshot = new FileStream(fileName, FileMode.CreateNew))
            {
                BitmapSource image = (BitmapSource)VideoStreamElement.Source;
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.QualityLevel = 70;
                jpegEncoder.Frames.Add(BitmapFrame.Create(image));
                jpegEncoder.Save(savedSnapshot);
                   
                savedSnapshot.Flush();
                savedSnapshot.Close();
                savedSnapshot.Dispose();
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.Kinect.ElevationAngle != this.Kinect.MaxElevationAngle)
            {
                this.Kinect.ElevationAngle += 3;
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.Kinect.ElevationAngle != this.Kinect.MinElevationAngle)
            {
                this.Kinect.ElevationAngle -= 3;
            }
        }

        #endregion Methods

        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }

            set
            {
                if (this._Kinect != value)
                {
                    if (this._Kinect != null)
                    {
                        UninitializeKinectSensor(this._Kinect);
                        this._Kinect = null;

                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._Kinect = value;
                        InitializeKinectSensor(this._Kinect);
                        this._Kinect.ElevationAngle = 0;
                    }
                }
            }
        }
        #endregion Properties
    }
}



