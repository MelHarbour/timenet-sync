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
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using TimeNet.Packets;
using TimeNet.Packets.Multisport;

namespace TimeNetSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App app;
        private StringBuilder logContent = new StringBuilder();
        private List<ResultlistCompetitor> currentResultlist;
        private SheetsService service;
        private string spreadsheetId = "1KWuuzRokyxaPFIqQckYR3OVsyaFvISNk_PKu6PwCZQ4";

        public MainWindow()
        {
            InitializeComponent();

            this.app = Application.Current as App;

            InitializeDrive();

            app.Communication.ConnectionStateChanged += new ConnectionStateChangedEventHandler(this.Communication_ConnectionStateChanged);
            app.Communication.ObjectReceived += new ObjectReceivedEventHandler(this.Communication_ObjectReceived);
            app.Communication.BroadcastAdvertisementReceived += new BroadcastAdvertisementReceivedEventHandler(this.Communication_BroadcastAdvertisementReceived);
        }

        private void InitializeDrive()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = System.IO.Path.Combine(credPath, ".credentials/sheets.googleapis.timenet-sync.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    app.Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Time.NET Sync",
            });
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
            this.OnObjectReceived(obj);
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

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            app.Communication.Reconnect();
        }
        
        private void OnObjectReceived(object obj)
        {
            if (obj is Resultlist)
            {
                OnResultlist(obj as Resultlist);
            }
        }
        
        private void OnResultlist(Resultlist resultlist)
        {
            currentResultlist = resultlist.Competitors;

            ValueRange valueRange = new ValueRange();

            var oblist = from c in this.currentResultlist
                         select new List<object>() { c.Name, ParseTimeString(c.Time) };
                        
            valueRange.Values = oblist.ToList<IList<object>>();

            SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Sheet1!A1");
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse result2 = update.Execute();
        }

        private double ParseTimeString(string time)
        {
            TimeSpan result;
            if (TimeSpan.TryParseExact(time, @"m\:ss\.ff", null, out result))
                return result.TotalSeconds;
            else if (TimeSpan.TryParseExact(time, @"s\.ff", null, out result))
                return result.TotalSeconds;
            else
                return 0;
        }
    }
}
