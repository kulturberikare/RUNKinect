using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.ComponentModel;

namespace KinectInfoBoxJT
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Member Variables
        private string sensorStatusValue;
        private string ConectionIDValue;
        private string deviceIDValue;
        private bool isColorStreamEnabledValue;
        private bool isDepthStreamEnabledValue;
        private bool isSkeletonStreamEnabledValue;
        private int sensorAngleValue;
        private bool canStartValue;
        private bool canStopValue;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Member Variables

        #region Methods
        public void OnNotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion Methods

        #region Properties
        public string Sensorstatus
        {
            get
            {
                return this.sensorStatusValue;
            }

            set
            {
                if (this.sensorStatusValue != value)
                {
                    this.sensorStatusValue = value;
                    this.OnNotifyPropertyChanged("SensorStatus");
                }
            }
        }

        public string ConectionID
        {
            get
            {
                return this.ConectionIDValue;
            }

            set
            {
                if (this.ConectionIDValue != value)
                {
                    this.ConectionIDValue = value;
                    this.OnNotifyPropertyChanged("ConectionID");
                }
            }
        }

        public string DeviceID
        {
            get
            {
                return this.deviceIDValue;
            }

            set
            {
                if (this.deviceIDValue != value)
                {
                    this.deviceIDValue = value;
                    this.OnNotifyPropertyChanged("DeviceID");
                }
            }
        }

        public bool IsColorStreamEnabled
        {
            get
            {
                return this.isColorStreamEnabledValue;
            }

            set
            {
                if (isColorStreamEnabledValue != value)
                {
                    this.isColorStreamEnabledValue = value;
                    this.OnNotifyPropertyChanged("IsColorstreamEnabled");
                }
            }
        }

        public bool IsDepthStreamEnabled
        {
            get
            {
                return this.isDepthStreamEnabledValue;
            }

            set
            {
                if (isDepthStreamEnabledValue != value)
                {
                    this.isDepthStreamEnabledValue = value;
                    this.OnNotifyPropertyChanged("IsDepthStreamEnabled");
                }
            }
        }

        public bool IsSkeletonStreamEnabled
        {
            get
            {
                return this.isSkeletonStreamEnabledValue;
            }
            set
            {
                if (isSkeletonStreamEnabledValue != value)
                {
                    this.isSkeletonStreamEnabledValue = value;
                    this.OnNotifyPropertyChanged("IsSkeletonStreamEnabled");
                }
            }
        }

        public int SensorAngle
        {
            get
            {
                return this.sensorAngleValue;
            }

            set
            {
                if (this.sensorAngleValue != value)
                {
                    this.sensorAngleValue = value;
                    this.OnNotifyPropertyChanged("SensorAngle");
                }
            }
        }

        public bool CanStart
        {
            get
            {
                return this.canStartValue;
            }

            set
            {
                if (this.canStartValue != value)
                {
                    this.canStartValue = value;
                    this.OnNotifyPropertyChanged("CanStart");
                }
            }
        }

        public bool CanStop
        {
            get
            {
                return this.canStopValue;
            }

            set
            {
                if (this.canStopValue != value)
                {
                    this.canStopValue = value;
                    this.OnNotifyPropertyChanged("CanStop");
                }
            }
        }
        #endregion Properties
    }
}


