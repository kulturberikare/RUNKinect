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

namespace KinectInfoBoxJT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private MainWindowViewModel viewModel;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
            this.viewModel = new MainWindowViewModel();
            this.viewModel.CanStart = false;
            this.viewModel.CanStop = false;
            this.DataContext = this.viewModel;
        }
        #endregion Constructor

        #region Methods
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.Kinect = KinectSensor.KinectSensors
                    .FirstOrDefault(x => x.Status == KinectStatus.Connected);
                KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                InitializeKinectSensor(this.Kinect);               

                SetKinectInfo();
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.viewModel.Sensorstatus = e.Status.ToString();
        }

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                this.viewModel.CanStart = false;
                this.viewModel.CanStop = true;
                sensor.ColorStream.Enable();
                sensor.DepthStream.Enable();
                sensor.SkeletonStream.Enable();
                sensor.Start();
            }
        }

        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                this.viewModel.CanStart = true;
                this.viewModel.CanStop = false;
                sensor.Stop();
            }
        }
        #endregion Methods

        private void SetKinectInfo()
        {
            this.viewModel.ConectionID = this.Kinect.DeviceConnectionId;
            this.viewModel.DeviceID = this.Kinect.UniqueKinectId;
            this.viewModel.Sensorstatus = this.Kinect.Status.ToString();
            this.viewModel.IsColorStreamEnabled = this.Kinect.ColorStream.IsEnabled;
            this.viewModel.IsDepthStreamEnabled = this.Kinect.DepthStream.IsEnabled;
            this.viewModel.IsSkeletonStreamEnabled = this.Kinect.SkeletonStream.IsEnabled;
            this.viewModel.SensorAngle = this.Kinect.ElevationAngle;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.StartSensor();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            this.StopSensor();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.StopSensor();
            this.Close();

        }

        private void StartSensor()
        {
            if (this.Kinect != null && !this.Kinect.IsRunning)
            {
                this.Kinect.Start();
                this.viewModel.CanStart = false;
                this.viewModel.CanStop = true;
            }
        }

        private void StopSensor()
        {
            if (this.Kinect != null && this.Kinect.IsRunning)
            {
                this.Kinect.Stop();
                this.viewModel.CanStart = true;
                this.viewModel.CanStop = false;
            }
        }
        #region Properties
        private KinectSensor Kinect
        {
            get
            {
                return this._Kinect;
            }

            set
            {
                if (this._Kinect != value)
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
        #endregion Properties
    }
}
