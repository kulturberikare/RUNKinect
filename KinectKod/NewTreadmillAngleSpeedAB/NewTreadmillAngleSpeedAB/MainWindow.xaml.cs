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

using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using System.IO;

namespace NewTreadmillAngleSpeedAB
{
    /// <summary>
    /// Beräkning av vinkel samt hastighet på löpbandet. Förenklad version jämfört med TreadmillAngleSpeedAB.
    /// Kollar på avståndet i x-led mellan höger och vänster fot istället för momentana hastigheter.
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private Skeleton[] _CurrentFrameSkeletons;
        private Joint prevRightFoot = new Joint();
        private Joint prevLeftFoot = new Joint();
        private Joint startPoint = new Joint();
        public Int64 startTime = 0;
        public bool readyToStart = true;

        private int index = 0;
        private double[] speedArray = new double[10];
        private double[] angleArray = new double[10];
        private double meanVelocity;
        private double meanAngle;
        private double velocity;
        private double angle;
        // private double maxFeetDistance = 0;
        private double feetDistance;
        private double prevFeetDistance;

        private readonly Brush[] _SkeletonBrushes;

        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this._SkeletonBrushes = new[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo,
                                             Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink};

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        #endregion Constructor

        #region Primary Methods
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                case KinectStatus.Initializing:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.Kinect = e.Sensor;
                    break;

                case KinectStatus.Disconnected:
                    MessageBox.Show("Plug in Kinect Device!");
                    this.Kinect = null;
                    break;

                default:
                    MessageBox.Show("//TODO Show an error state");
                    break;
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

                    ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,
                                                                  PixelFormats.Bgr32, null, pixelData,
                                                                  frame.Width * frame.BytesPerPixel);
                }
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton currentskeleton;
                    frame.CopySkeletonDataTo(this._CurrentFrameSkeletons);

                    // Ritgrejer
                    //Polyline figure;
                    //Brush userBrush;
                    //LayoutRoot.Children.Clear();
                    // Ritgrejer
                    for (int i = 0; i < this._CurrentFrameSkeletons.Length; i++)
                    {
                        currentskeleton = this._CurrentFrameSkeletons[i];

                        if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            //#region DrawSkeleton
                            //userBrush = this._SkeletonBrushes[i % this._SkeletonBrushes.Length];

                            //figure = CreateFigure(currentskeleton, userBrush, new[] { JointType.HipLeft, JointType.KneeLeft,
                            //                 JointType.AnkleLeft, JointType.FootLeft});

                            //LayoutRoot.Children.Add(figure);

                            //figure = CreateFigure(currentskeleton, userBrush, new[] { JointType.HipRight, JointType.KneeRight,
                            //                 JointType.AnkleRight, JointType.FootRight});

                            //LayoutRoot.Children.Add(figure);
                            //#endregion DrawSkeleton
                            

                            Joint rightFoot = currentskeleton.Joints[JointType.FootRight];
                            Joint leftFoot = currentskeleton.Joints[JointType.FootLeft];

                            feetDistance = GetJointDistance(rightFoot, leftFoot);
                            prevFeetDistance = GetJointDistance(prevRightFoot, prevLeftFoot);

                            #region Test1
                            //using (System.IO.StreamWriter file = new System.IO.StreamWriter
                            //       (@"C:\Users\Alex\Documents\GitHub\RUNKinect\KinectKod\NewTreadmillAngleSpeedAB\FeetDistance12.txt", true))
                            //{
                            //    file.WriteLine(feetDistance.ToString());
                            //    file.Close();
                            //}

                            //FootDistance.Text = String.Format("{0} \n {1} maxavstånd", Math.Round(feetDistance, 2),
                            //                                                           Math.Round(maxFeetDistance, 2));
                            #endregion Test1

                            if (feetDistance < 0.5 &&
                                feetDistance > 0.25 &&
                                feetDistance < prevFeetDistance &&
                                leftFoot.Position.X < rightFoot.Position.X && // Högerfoten närmast främre änden av löpbandet
                                readyToStart == true &&
                                startTime == 0)
                            {
                                startTime = frame.Timestamp;
                                startPoint = leftFoot;

                                #region DrawStartPoint
                                ColorImagePoint point = this._Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(startPoint.Position, ColorImageFormat.RgbResolution640x480Fps30);

                                point.X = (int)((point.X * ColorImageElement.ActualWidth /
                                                  this._Kinect.ColorStream.FrameWidth) -
                                                  (FootFront.ActualWidth / 2.0));

                                point.Y = (int)((point.Y * ColorImageElement.ActualHeight /
                                                  this._Kinect.ColorStream.FrameHeight) -
                                                  (FootFront.ActualHeight / 2.0));

                                Canvas.SetLeft(FootFront, point.X);
                                Canvas.SetTop(FootFront, point.Y);
                                #endregion DrawStartPoint

                                readyToStart = false;
                            }

                            if (readyToStart == false &&
                                startTime != 0 &&
                                (frame.Timestamp - startTime) / 1000 > 0.75)
                            {
                                readyToStart = true;
                                startTime = 0;
                            }

                            if (feetDistance > 0.25 &&
                                feetDistance > prevFeetDistance &&
                                leftFoot.Position.X > rightFoot.Position.X &&
                                startTime != 0 &&
                                readyToStart == false)
                            {
                                double timeDifferenceMs = frame.Timestamp - startTime; // Skillnad mellan nuvarande tid och startTime i ms.
                                double time = timeDifferenceMs / 1000; // Gör om till s

                                Joint endPoint = leftFoot;
                                float leftFootDistance = GetJointDistance(startPoint, endPoint);

                                //MessageBox.Show("Då!");

                                #region DrawEndPoint
                                ColorImagePoint point = this._Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(endPoint.Position, ColorImageFormat.RgbResolution640x480Fps30);

                                point.X = (int)((point.X * ColorImageElement.ActualWidth /
                                                  this._Kinect.ColorStream.FrameWidth) -
                                                  (FootFront.ActualWidth / 2.0));

                                point.Y = (int)((point.Y * ColorImageElement.ActualHeight /
                                                  this._Kinect.ColorStream.FrameHeight) -
                                                  (FootFront.ActualHeight / 2.0));

                                Canvas.SetLeft(FootBack, point.X);
                                Canvas.SetTop(FootBack, point.Y);
                                #endregion DrawEndPoint

                                if (leftFootDistance > 0.2 && leftFootDistance < 0.5)
                                {
                                    velocity = TreadmillSpeed(leftFootDistance, time);
                                    speedArray[index] = velocity;

                                    angle = TreadmillAngle(startPoint, endPoint);
                                    angleArray[index] = angle;
                                    index++;
                                }

                                if (index == 9)
                                {
                                    Array.Sort(speedArray);
                                    //double first = speedArray[1];

                                    //MessageBox.Show(first.ToString());

                                    meanVelocity = Math.Round(ArrayMean(speedArray) * 3.6, 1);
                                    meanAngle = Math.Round(ArrayMean(angleArray), 1);
                                    index = 0;
                                    // maxFeetDistance = 0;
                                }

                                //if (feetDistance > maxFeetDistance)
                                //{
                                //    maxFeetDistance = feetDistance;
                                //}

                                TestText.Text = String.Format("{0} m/s \n {1} s \n {2} avstånd(m) \n {3} mätningar har gjorts \n {4} km/h medel",
                                                              Math.Round(velocity, 2), Math.Round(time, 2), Math.Round(leftFootDistance, 2),
                                                              index, meanVelocity);

                                startTime = 0; // Nu är vi klara med StartTime. Förbereder för nästa mätning.
                                readyToStart = true;                                
                            }

                            //ExtraTest.Text = String.Format("\n\n\n {0} i x \n {1} i y \n {2} i z", Math.Round(startPoint.Position.X, 2),
                            //                                                                       Math.Round(startPoint.Position.Y, 2),
                            //                                                                       Math.Round(startPoint.Position.Z, 2));

                            prevRightFoot = rightFoot;
                            prevLeftFoot = leftFoot;

                            //TestText.Text = string.Format("{0} m", Math.Round(feetDistance, 3));
                        }
                    }
                }
            }
        }

        #endregion Primary Methods

        #region Helper Methods

        // Metod för att räkna ut avståndet mellan två joints.
        public float GetJointDistance(Joint rightJoint, Joint leftJoint)
        {
            float xDistance = rightJoint.Position.X - leftJoint.Position.X;
            float yDistance = rightJoint.Position.Y - leftJoint.Position.Y;
            float zDistance = rightJoint.Position.Z - leftJoint.Position.Z;

            return (float)Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2) + Math.Pow(zDistance, 2));
        }

        // Metod för att få ut vinkeln på löpbandet.
        // Beräknar motstående katet (skillnad i y-led) samt närstående katet (skillnad i x-led).
        // Därefter arctan på hela kalaset.
        public double TreadmillAngle(Joint footstart, Joint footend)
        {
            double oppcat = (footstart.Position.Y - footend.Position.Y);
            double nearcat = (footstart.Position.X - footend.Position.X);

            // Lägg ev. till Math.Round
            double angle = (Math.Atan(oppcat / nearcat)) * (180 / Math.PI);
            angle = Math.Abs(angle);
            return angle;
        }

        // Metod för att få ut hastigheten på löpbandet.
        // Använder sig av tiden som förflutit mellan att foten sätts i löpbandet (neg. hastighet x-riktning)
        // och när foten dras upp från löpbandet (pos. hastighet y-riktning).
        public double TreadmillSpeed(double distance, double time)
        {
            double speed = (distance / time);
            return speed;
        }

        // Ta medelvärde från en array.
        public double ArrayMean(double[] array)
        {
            double sum = 0;
            for (int i = 2; i < array.Length - 2; i++)
            {
                sum += array[i];
            }
            return sum / (array.Length - 4);
        }

        // Sortera bort värden ur array som är för låga eller för höga.

        // Set ElevationAngle.
        private void SetSensorAngle(int angleValue)
        {
            if (angleValue > this._Kinect.MinElevationAngle ||
                angleValue < this._Kinect.MaxElevationAngle ||
                angleValue == this._Kinect.ElevationAngle)
            {
                this._Kinect.ElevationAngle = angleValue;
            }
        }

        //#region DrawHelp
        //private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        //{
        //    Polyline figure = new Polyline();
        //    figure.StrokeThickness = 8;
        //    figure.Stroke = brush;

        //    for (int i = 0; i < joints.Length; i++)
        //    {
        //        figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
        //    }

        //    return figure;
        //}

        //private Point GetJointPoint(Joint joint)
        //{
        //    ColorImagePoint point = this._Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position,
        //                                                                 this._Kinect.ColorStream.Format);

        //    point.X *= (int)this.LayoutRoot.ActualWidth / this._Kinect.ColorStream.FrameWidth;
        //    point.Y *= (int)this.LayoutRoot.ActualHeight / this._Kinect.ColorStream.FrameHeight;

        //    return new Point(point.X, point.Y);
        //}
        //#endregion DrawHelp

        #endregion Helper Methods

        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }

            set
            {
                if (this._Kinect != value)
                {
                    //Uninitialize
                    if (this._Kinect != null)
                    {
                        this._Kinect.Stop();
                        this._Kinect.SkeletonFrameReady -= Kinect_SkeletonFrameReady;
                        this._Kinect.ColorFrameReady -= Kinect_ColorFrameReady;
                        this._Kinect.SkeletonStream.Disable();
                        this._CurrentFrameSkeletons = null;
                    }

                    this._Kinect = value;

                    // Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                            {
                                smoothingParam.Smoothing = 0.5f;
                                smoothingParam.Correction = 0.1f;
                                smoothingParam.Prediction = 0.5f;
                                smoothingParam.JitterRadius = 0.1f;
                                smoothingParam.MaxDeviationRadius = 0.1f;
                            };

                            this._Kinect.SkeletonStream.Enable(smoothingParam);                                                                                                                                          
                            this._Kinect.ColorStream.Enable();
                            this._CurrentFrameSkeletons = new Skeleton[this._Kinect.SkeletonStream.FrameSkeletonArrayLength];
                            this._Kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
                            this._Kinect.ColorFrameReady += Kinect_ColorFrameReady;
                            this._Kinect.Start();
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}
