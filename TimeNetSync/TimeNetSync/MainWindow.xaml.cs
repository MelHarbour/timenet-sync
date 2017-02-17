﻿using System;
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
        private StringBuilder logContent = new StringBuilder();
        private SheetsService service;
        private string spreadsheetId = "1KWuuzRokyxaPFIqQckYR3OVsyaFvISNk_PKu6PwCZQ4";
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
            FillViewModel();
        }

        private void FillViewModel()
        {
            using (SqlCeConnection connection = new SqlCeConnection(@"Data Source=""C:\Users\Public\Documents\Time.Net 2\Competitions\Test.timeNetCompetition"""))
            {
                connection.Open();
                SqlCeCommand command = new SqlCeCommand("SELECT Id, Bib, FirstName FROM Competitors", connection);
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
                    competitor.FirstName = (string)reader[2];

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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            ValueRange valueRange = new ValueRange();
            var objlist = from c in this.ViewModel.Competitors
                          orderby c.RunTime
                          select new List<object>() { c.Bib, c.FirstName, c.RunTime };

            valueRange.Values = objlist.ToList<IList<object>>();

            SpreadsheetsResource.ValuesResource.UpdateRequest update = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Sheet1!A1");
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse result2 = update.Execute();
        }
    }
}
