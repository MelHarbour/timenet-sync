using dbnetsoft.Communication;
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
using dbnetsoft.Communication.Packets;

namespace TimeNetSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App app;
        private StringBuilder logContent = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();

            this.app = Application.Current as App;

            app.Communication.ConnectionStateChanged += new ConnectionStateChangedEventHandler(this.Communication_ConnectionStateChanged);
            app.Communication.ObjectReceived += new ObjectReceivedEventHandler(this.Communication_ObjectReceived);
            app.Communication.BroadcastAdvertisementReceived += new BroadcastAdvertisementReceivedEventHandler(this.Communication_BroadcastAdvertisementReceived);
        }

        private void Communication_BroadcastAdvertisementReceived(object sender, BroadcastServerInformation broadcastInformation, string remoteHost, int remotePort)
        {
            object[] objArray1 = new object[] { DateTime.Now.ToLongTimeString(), ": Advertisement from Time.NET server received: ", remoteHost, ":", remotePort };
            logContent.AppendLine(string.Concat(objArray1));
            this.Dispatcher.Invoke(() =>
            {
                this.logTextBox.Text = logContent.ToString();
            });
        }

        private void Communication_ObjectReceived(object sender, object obj)
        {
            //throw new NotImplementedException();
        }

        private void Communication_ConnectionStateChanged(object sender, enumConnectionState connectionState)
        {
            switch (connectionState)
            {
                case enumConnectionState.Connected:
                    this.Dispatcher.Invoke(() =>
                    {
                        this.statusLabel.Background = new SolidColorBrush(Colors.LightGreen);
                    });
                    break;

                case enumConnectionState.Connecting:
                    this.Dispatcher.Invoke(() =>
                    {
                        this.statusLabel.Background = new SolidColorBrush(Colors.Yellow);
                    });
                    break;

                case enumConnectionState.Disconnected:
                    this.Dispatcher.Invoke(() =>
                    {
                        this.statusLabel.Background = new SolidColorBrush(Colors.Salmon);
                    });
                    break;
            }
            this.Dispatcher.Invoke(() =>
            {
                this.statusLabel.Content = app.Communication.ToString();
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            app.Communication.Reconnect();
        }
    }
}
