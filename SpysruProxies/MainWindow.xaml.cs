using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading;
using System.Xml;

using SpysruProxiesEngineNs;
using System.ComponentModel;

namespace SpysruProxies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpysruProxiesEngine eng;
        SynchronizationContext syncCtx;
        public ObservableCollection<ProxyLookupEventArgs> proxies;
        string Protocol;

        public MainWindow()
        {
            eng = new SpysruProxiesEngine();
            proxies = new ObservableCollection<ProxyLookupEventArgs>();
            Protocol = "Any";

            InitializeComponent();
            syncCtx = System.Threading.SynchronizationContext.Current;

            export.IsEnabled = false;
            this.ProxyListUI.ItemsSource = proxies;
            
            eng.RaiseProxyLookup += HandleProxyLookupEvent;
            eng.RaiseProxyLookupEnd += HandleProxyLookupEndEvent;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TextWriter twr = new StreamWriter("e:\\prj\\spysru.prj\\website.log", true);
            //twr.WriteLine("{0}: ComboBox_SelectionChanged() {1}", DateTime.Now
            //    , ((ComboBoxItem)e.AddedItems[0]).Content.ToString());
            //twr.Close();
            Protocol = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            if (proxies != null) proxies.Clear();
            if (refresh != null) this.refresh.IsEnabled = false;
            if (export != null) this.export.IsEnabled = false;
            if (protocol != null) this.protocol.IsEnabled = false;
            Application.Current.MainWindow.Cursor = Cursors.AppStarting;
            eng.lookup(Protocol);
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            proxies.Clear();
            this.refresh.IsEnabled = false;
            this.export.IsEnabled = false;
            this.protocol.IsEnabled = false;
            Application.Current.MainWindow.Cursor = Cursors.AppStarting;
            eng.lookup(Protocol);
        }

        void HandleProxyLookupEvent(object sender, ProxyLookupEventArgs e)
        {
            syncCtx.Send(proxyInfoAdd, e);
        }

        public void proxyInfoAdd(Object state)
        {
            proxies.Add((ProxyLookupEventArgs)state);
        }

        void HandleProxyLookupStateChangeEvent(object sender, ProxyLookupEventArgs e)
        {
            syncCtx.Send(proxyInfoStateUpdate, e);
        }

        public void proxyInfoStateUpdate(Object state)
        {
            ProxyLookupEventArgs pinfo= (ProxyLookupEventArgs)state;
            try
            {
                ProxyLookupEventArgs ee = proxies.Single(prx => prx.Id == pinfo.Id);
                ee.State = pinfo.State;
            }
            catch (System.InvalidOperationException)
            {
            }
        }
        void HandleProxyLookupEndEvent(object sender, EventArgs e)
        {
            syncCtx.Send(OnProxyLookupEndUiUpdate, new object());
        }


        public void OnProxyLookupEndUiUpdate(Object state)
        {
            //TextWriter twr = new StreamWriter("e:\\prj\\spysru.prj\\website.log", true);
            //twr.WriteLine("{0}: OnProxyLookupEndUiUpdate()", DateTime.Now);
            //twr.Close();
            this.refresh.IsEnabled = true;
            this.export.IsEnabled = true;
            this.protocol.IsEnabled = true;
            Application.Current.MainWindow.Cursor = Cursors.Arrow;
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            this.export.IsEnabled = false;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "All files (*.*)|*.*|Xml files (*.xml)|*.xml";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() != true)
            {
                this.export.IsEnabled = true;
                return;
            }

            XmlDocument x = new XmlDocument();
            
            XmlElement root = x.CreateElement("proxies");
            foreach(ProxyLookupEventArgs prx in this.proxies)
            {
                XmlElement p = x.CreateElement("proxy");
                p.SetAttribute("protocol",prx.Protocol);
                p.SetAttribute("host",prx.Host);
                p.SetAttribute("port",prx.Port);
                p.SetAttribute("state", prx.State.ToString());
                p.SetAttribute("details", prx.Details);
                root.AppendChild(p);
            }
            x.AppendChild(root);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            using (XmlWriter writer = XmlWriter.Create(saveFileDialog.FileName,settings))
            {
                x.Save(writer);
            }
            this.export.IsEnabled = true;
        }

    }
}
