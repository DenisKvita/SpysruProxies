using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpysruProxiesEngineNs
{
    public class ProxyLookupEventArgs : EventArgs, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        private int id;
        public int Id
        {
            get{
                return this.id;
            }
            set
            {
                if (value != this.id)
                {
                    this.id = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string host;
        public string Host {
            get
            {
                return this.host;
            }
            set
            {
                if (value != this.host)
                {
                    this.host = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string protocol;
        public string Protocol {
            get
            {
                return this.protocol;
            }
            set
            {
                if (value != this.protocol)
                {
                    this.protocol = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string port;
        public string Port
        {
            get
            {
                return this.port;
            }
            set
            {
                if (value != this.port)
                {
                    this.port = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string details;
        public string Details
        {
            get
            {
                return this.details;
            }
            set
            {
                if (value != this.details)
                {
                    this.details = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public enum CheckState
        {
            Unknown,
            Checking,
            Online,
            Offline,
            Error
        }

        private CheckState stt;
        public CheckState State
        {
            get
            {
                return this.stt;
            }
            set
            {
                if (value != this.stt)
                {
                    this.stt = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ProxyLookupEventArgs()
        {

        }
        public ProxyLookupEventArgs(int id, string protocol, string host, string port)
        {
            Id = id;
            Protocol = protocol;
            Host = host;
            Port = port;
            State = CheckState.Unknown;
        }
    }
}
