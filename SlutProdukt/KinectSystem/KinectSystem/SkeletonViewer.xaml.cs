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
using System.Windows.Media.Media3D;

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class SkeletonViewer : UserControl
    {
        #region Member Variables
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Pink, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Green };
        private Skeleton[] _FrameSkeletons;

        //Medlemsvariabler för counten till vinklarna samt globala varaibler till vinklarna.
        private double count = 0;
        public double AngleRK = 0;
        public double AngleLK = 0;
        public double AngleRA = 0;
        public double AngleLA = 0;
        public double AngleH = 0;

        private Joint prevRightFoot = new Joint();
        private Joint prevLeftFoot = new Joint();
        private Joint startPoint = new Joint();
        public Int64 startTime = 0;
        public bool readyToStart = true;

        private int index = 0;
        private double[] speedArray = new double[10];
        private double[] angleArray = new double[10];
        public double meanSpeed;
        public double meanAngle;
        private double speed;
        private double angle;
        private double feetDistance;
        private double prevFeetDistance;

        #endregion Member Variables

        #region Constructor
        public SkeletonViewer()
        {
            InitializeComponent();
        }
        #endregion Constructor

        #region Methods
        //Eventhanterare
        public void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonsPanel.Children.Clear();

            if (this.IsEnabled)
            {
                using (SkeletonFrame frame = e.OpenSkeletonFrame())
                {
                    if (frame != null)
                    {
                        Skeleton currentskeleton;
                        if (this.IsEnabled)
                        {
                            frame.CopySkeletonDataTo(this._FrameSkeletons);

                            for (int i = 0; i < this._FrameSkeletons.Length; i++)
                            {
                                DrawSkeleton(this._FrameSkeletons[i], this._SkeletonBrushes[i]);

                                currentskeleton = this._FrameSkeletons[i];
                                if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    Joint rightFoot = currentskeleton.Joints[JointType.FootRight];
                                    Joint leftFoot = currentskeleton.Joints[JointType.FootLeft];

                                    feetDistance = GetJointDistance(rightFoot, leftFoot);
                                    prevFeetDistance = GetJointDistance(prevRightFoot, prevLeftFoot);

                                    // Kolla om kriterierna uppfylls för att starta mätningen
                                    if (feetDistance < 0.5 &&
                                        feetDistance > 0.25 &&
                                        feetDistance < prevFeetDistance &&
                                        leftFoot.Position.X < rightFoot.Position.X && // Högerfoten närmast främre änden av löpbandet
                                        readyToStart == true &&
                                        startTime == 0)
                                    {
                                        startTime = frame.Timestamp;
                                        startPoint = leftFoot;

                                        readyToStart = false;
                                    }

                                    // Test för att se till så man inte missar slutpunkten och genomför mätningen på flera steg.
                                    if (readyToStart == false &&
                                        startTime != 0 &&
                                        (frame.Timestamp - startTime) / 1000 > 0.75)
                                    {
                                        readyToStart = true;
                                        startTime = 0;
                                    }

                                    // Kolla om kriterierna uppfylls för att avsluta mätningen.
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

                                        if (leftFootDistance > 0.2 && leftFootDistance < 0.5)
                                        {
                                            speed = TreadmillSpeed(leftFootDistance, time);
                                            speedArray[index] = speed;

                                            angle = TreadmillAngle(startPoint, endPoint);
                                            angleArray[index] = angle;
                                            index++;
                                        }

                                        if (index == 9)
                                        {
                                            Array.Sort(speedArray);
                                            // Gör om medelvärdet till km/h
                                            meanSpeed = SortedArrayMean(speedArray) * 3.6;

                                            //if (meanSpeed < 5)
                                            //{
                                            //    meanSpeed *= 1.06;
                                            //}
                                            //else if (meanSpeed >= 5 && meanSpeed < 10)
                                            //{
                                            //    meanSpeed *= 1.1;
                                            //}
                                            //else if (meanSpeed >= 10)
                                            //{
                                            //    meanSpeed *= 1.22;
                                            //}

                                            meanSpeed = Math.Round(meanSpeed, 1);

                                            using (System.IO.StreamWriter file = new System.IO.StreamWriter
                                                   (@"C:\Users\Kandidat\Documents\GitHub\RUNKinect\SlutProdukt\KinectSystem\DefaultSpeed8.txt", true))
                                            {
                                                file.WriteLine(meanSpeed.ToString());
                                                file.Close();
                                            }

                                            Array.Sort(angleArray);
                                            meanAngle = Math.Round(SortedArrayMean(angleArray), 1);
                                            index = 0;
                                        }

                                        //SpeedText.Text = String.Format("{0} km/h", meanSpeed);
                                        //AngleText.Text = String.Format("{1} degrees", meanAngle);

                                        startTime = 0; // Nu är vi klara med StartTime. Förbereder för nästa mätning.
                                        readyToStart = true;
                                    }

                                    // Spara positioner för att kunna jämföra med nästa frame.
                                    prevRightFoot = rightFoot;
                                    prevLeftFoot = leftFoot;

                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HandLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HandRight], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HipLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HipRight], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.KneeLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.KneeRight], this._SkeletonBrushes[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        //Funktion som ritar upp skelettet som fås som invärde.
        private void DrawSkeleton(Skeleton skeleton, Brush brush)
        {
            //Kollar om det finns ett skelett som följs.
            if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                //Ritar huvud och torax
                Polyline figure = CreateFigure(skeleton, brush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                             JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter});
                SkeletonsPanel.Children.Add(figure);

                figure = CreateFigure(skeleton, brush, new[] { JointType.HipLeft, JointType.HipRight });
                SkeletonsPanel.Children.Add(figure);

                //Ritar vänster ben
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonsPanel.Children.Add(figure);

                //Ritar höger ben
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                SkeletonsPanel.Children.Add(figure);

                //Ritar vänster arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonsPanel.Children.Add(figure);

                //Ritar höger arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonsPanel.Children.Add(figure);

                DefineVectors(skeleton); //Kör funktionen DefineVectors
                count++; //Ökar countern 
            }
        }

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

        // Ta medelvärde från en array. Denna funktion är framtagen för en array som är sorterad från
        // lägsta till högsta värde. Den tar bort de två lägsta och de två högsta och tar medel på
        // resterande.
        public double SortedArrayMean(double[] array)
        {
            double sum = 0;
            for (int i = 2; i < array.Length - 2; i++)
            {
                sum += array[i];
            }
            return sum / (array.Length - 4);
        }

        private void TrackJoint(Joint joint, Brush brush)
        {
            if (joint.TrackingState != JointTrackingState.NotTracked)
            {
                Point jointPoint = GetJointPoint(joint);
            }
        }

        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 4;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }


        private Point GetJointPoint(Joint joint)
        {

            DepthImagePoint point = KinectSensorOne.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.KinectSensorOne.DepthStream.Format);
            point.X *= (int)this.LayoutRootOne.ActualWidth / KinectSensorOne.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRootOne.ActualHeight / KinectSensorOne.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        //Definierar vektorer som används för att plocka fram vinkeln
        private void DefineVectors(Skeleton skeleton)
        {
            //Definierar aktuella leder och skapar en vektor med ledens x-, y-, z-position mha GetJointPoint3D.
            Vector3D ankleL = GetJointPoint3D(skeleton.Joints[JointType.AnkleLeft]);
            Vector3D ankleR = GetJointPoint3D(skeleton.Joints[JointType.AnkleRight]);
            Vector3D footL = GetJointPoint3D(skeleton.Joints[JointType.FootLeft]);
            Vector3D footR = GetJointPoint3D(skeleton.Joints[JointType.FootRight]);
            Vector3D hipL = GetJointPoint3D(skeleton.Joints[JointType.HipLeft]);
            Vector3D hipR = GetJointPoint3D(skeleton.Joints[JointType.HipRight]);
            Vector3D hipCenter = GetJointPoint3D(skeleton.Joints[JointType.HipCenter]);
            Vector3D kneeL = GetJointPoint3D(skeleton.Joints[JointType.KneeLeft]);
            Vector3D kneeR = GetJointPoint3D(skeleton.Joints[JointType.KneeRight]);

            //Definierar vinklar för liveutskrift och sparning till fil
            AngleRK = Math.Round(FindAngles(ankleR, kneeR, hipR),2);
            AngleLK = Math.Round(FindAngles(ankleL, kneeL, hipL),2);
            AngleRA = Math.Round(FindAngles(footR, ankleR, kneeR),2);
            AngleLA = Math.Round(FindAngles(footL, ankleL, kneeL),2);
            AngleH = Math.Round(FindAngles(hipL, hipCenter, hipR),2);

            //Hittar vinkeln i en led mha FindAngles och skickar sedan respektive led till respektive fil 
            WriterLK(AngleLK);
            WriterRK(AngleRK);
            WriterLA(AngleLA);
            WriterRA(AngleRA);
            WriterH(AngleH);
        }

        //Skapar en 3D-vektor med en leds x-, y-, z-position.
        private Vector3D GetJointPoint3D(Joint joint)
        {
            return new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        //Hittar vinkeln i en led utiftrån 3 punkter.
        private double FindAngles(Vector3D vector1, Vector3D vector2, Vector3D vector3)
        {
            Vector3D a1 = vector1 - vector2;
            Vector3D a2 = vector3 - vector2;

            return Vector3D.AngleBetween(a1, a2);
        }

        //Skriver vinkeln i vänster knä till en textfil som heter LeftKnee
        private void WriterLK(double angle)
        {
            //Skapar en textfil om den inte finns.
            if (!File.Exists("LeftKnee.txt"))
            {
                StreamWriter file = new StreamWriter("LeftKnee.txt");
                file.Close();
            }
            else
            {
                //Använder filen som finns och sparar angle (om den inte är NaN) och count
                using (StreamWriter file = new StreamWriter("LeftKnee.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        //Om vinkeln inte hittats, dvs om den är NaN, sparas den som värdet noll.
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i höger knä till en textfil som heter RightKnee
        private void WriterRK(double angle)
        {
            if (!File.Exists("RightKnee.txt"))
            {
                StreamWriter file = new StreamWriter("RightKnee.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightKnee.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriv vinkeln i höger fotled till en textfil som heter RightAnkle
        private void WriterRA(double angle)
        {
            if (!File.Exists("RightAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("RightAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightAnkle.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i vänster fotled till en textfil som heter LeftAnkle
        private void WriterLA(double angle)
        {
            if (!File.Exists("LeftAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("LeftAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("LeftAnkle.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i höftern till en textfil som heter Hip
        private void WriterH(double angle)
        {
            if (!File.Exists("Hip.txt"))
            {
                StreamWriter file = new StreamWriter("Hip.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("Hip.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }
        #endregion Methods

        #region Properties
        #region KinectSensorOne

        protected const string KinectDevicePropertyName = "KinectSensorOne";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewer), new PropertyMetadata(null, KinectDeviceChanged));

        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewer viewer = (SkeletonViewer)owner;

            if (e.OldValue != null)
            {
                ((KinectSensor)e.OldValue).SkeletonFrameReady -= viewer.Kinect_SkeletonFrameReady;
                viewer._FrameSkeletons = null;
            }

            if (e.NewValue != null)
            {
                viewer.KinectSensorOne = (KinectSensor)e.NewValue;
                viewer.KinectSensorOne.SkeletonFrameReady += viewer.Kinect_SkeletonFrameReady;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectSensorOne.SkeletonStream.FrameSkeletonArrayLength];
            }
        }


        public KinectSensor KinectSensorOne
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion KinectSensorOne
        #endregion Properties
    }
}