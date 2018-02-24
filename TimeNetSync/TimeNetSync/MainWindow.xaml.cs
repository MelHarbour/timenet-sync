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
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Practices.Unity;
using TimeNetSync.ViewModel;
using System.Data.SqlServerCe;
using TimeNetSync.Model;
using System.Windows.Threading;

namespace TimeNetSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private App app;
        private SheetsService service;
        public CompetitorListViewModel ViewModel { get; set; }
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new CompetitorListViewModel();

            this.app = Application.Current as App;

            InitializeDrive();
            DataContext = ViewModel;
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ViewModel.IsConnecting)
            {
                FillViewModel();
                SendToGoogle();
            }
        }

        private void FillViewModel()
        {
            using (SqlCeConnection connection = new SqlCeConnection(String.Concat(@"Data Source=""", ViewModel.FilePath, @"""")))
            {
                connection.Open();
                SqlCeCommand command = new SqlCeCommand("SELECT Id, Bib, LastName FROM Competitors", connection);
                SqlCeCommand resultsCommand = new SqlCeCommand("SELECT Id, Section, TimeOfDay, State FROM MultisportResults WHERE Bib = @Bib", connection);

                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = (int)reader[0];
                    Competitor competitor = ViewModel.Competitors.FirstOrDefault(x => x.Id == id);
                    if (competitor == null)
                    {
                        competitor = new Competitor();
                        competitor.Id = (int)reader[0];
                        ViewModel.Competitors.Add(competitor);
                    }

                    competitor.Bib = (int)reader[1];
                    competitor.LastName = (string)reader[2];

                    resultsCommand.Parameters.Clear();
                    resultsCommand.Parameters.Add(new SqlCeParameter("Bib", competitor.Bib));
                    SqlCeDataReader resultsReader = resultsCommand.ExecuteReader();
                    while (resultsReader.Read())
                    {
                        int resultsId = (int)resultsReader[0];
                        MultisportResult result = competitor.Results.FirstOrDefault(x => x.Id == resultsId);
                        if (result == null)
                        {
                            result = new MultisportResult();
                            result.Id = resultsId;
                            competitor.Results.Add(result);
                        }
                        result.Bib = competitor.Bib;
                        result.Section = (int)resultsReader[1];
                        result.TimeOfDay = TimeSpan.FromMilliseconds((int)resultsReader[2] / 10);
                        result.State = (ResultState)(int)resultsReader[3];
                    }
                }
            }
        }

        private void InitializeDrive()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    service.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        private void SendToGoogle()
        {
            ValueRange valueRange = new ValueRange();
            var objlist = from c in ViewModel.Competitors
                          orderby c.Bib
                          select new List<object>() { c.Bib, c.LastName, c.StartTime?.TimeOfDay, c.RunTimeToSection(1), c.RunTimeToSection(2), c.RunTime };

            valueRange.Values = objlist.ToList<IList<object>>();

            try
            {
                SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, ViewModel.SpreadsheetId, ViewModel.RangeTarget);
                update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                UpdateValuesResponse result2 = update.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
