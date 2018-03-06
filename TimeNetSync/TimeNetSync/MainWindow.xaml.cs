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
using System.Data.SqlClient;
using System.Globalization;

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
                SendToWebsite();
            }
        }

        private void FillViewModel()
        {
            using (SqlCeConnection connection = new SqlCeConnection(String.Concat(@"Data Source=""", ViewModel.FilePath, @"""")))
            using (SqlCeCommand command = new SqlCeCommand("SELECT Id, Bib, LastName, Code FROM Competitors", connection))
            using (SqlCeCommand resultsCommand = new SqlCeCommand("SELECT Id, Section, TimeOfDay, State FROM MultisportResults WHERE Bib = @Bib", connection))
            {
                connection.Open();

                using (SqlCeDataReader reader = command.ExecuteReader())
                {
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
                        competitor.Code = (string)reader[3];

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
                Console.WriteLine(Properties.Resources.credentialsSaved + credPath);
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
            GC.SuppressFinalize(this);
        }
        #endregion

        private void SendToGoogle()
        {
            ValueRange valueRange = new ValueRange();
            var objlist = from c in ViewModel.Competitors
                          orderby c.Bib
                          select new List<object>() { c.Bib, c.LastName, c.StartTime?.TimeOfDay.TotalDays,
                              c.RunTimeToSection(1).TotalDays, c.RunTimeToSection(2).TotalDays, c.RunTime.TotalDays };

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

        private void SendToWebsite()
        {
            if (!String.IsNullOrEmpty(ViewModel.ConnectionString))
            {
                try {
                    using (SqlConnection conn = new SqlConnection(ViewModel.ConnectionString))
                    using (SqlCommand selectCommand = new SqlCommand("SELECT CrewId FROM dbo.Crews WHERE BroeCrewId = @BroeId", conn))
                    using (SqlCommand command = new SqlCommand(@"SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
                    BEGIN TRANSACTION;
                    UPDATE dbo.Results SET TimeOfDay = @TimeOfDay WHERE CrewId = @CrewId AND TimingPointId = @TimingPointId;
                    IF @@ROWCOUNT = 0
                    BEGIN
                        INSERT dbo.Results(CrewId,TimeOfDay,TimingPointId) SELECT @CrewId,@TimeOfDay,@TimingPointId;
                    END
                    COMMIT TRANSACTION; ", conn))
                    using (SqlCommand statusCommand = new SqlCommand("UPDATE dbo.Crews SET Status = @ResultStatus WHERE CrewId = @CrewId", conn))
                    {
                        conn.Open();

                        SqlParameter broeParameter = new SqlParameter("@BroeId", System.Data.SqlDbType.Int);
                        selectCommand.Parameters.Add(broeParameter);

                        SqlParameter timeOfDay = new SqlParameter("@TimeOfDay", System.Data.SqlDbType.Time);
                        SqlParameter timingPointId = new SqlParameter("@TimingPointId", System.Data.SqlDbType.Int);
                        SqlParameter crewId = new SqlParameter("@CrewId", System.Data.SqlDbType.Int);
                        SqlParameter resultStatus = new SqlParameter("@ResultStatus", System.Data.SqlDbType.Int);
                        command.Parameters.Add(timeOfDay);
                        command.Parameters.Add(timingPointId);
                        command.Parameters.Add(crewId);
                        statusCommand.Parameters.Add(crewId);
                        statusCommand.Parameters.Add(resultStatus);

                        foreach (Competitor competitor in ViewModel.Competitors)
                        {
                            if (competitor.Code == null || (competitor.StartTime == null && competitor.State == ResultState.Ok))
                                continue;

                            // Get the CrewId
                            broeParameter.Value = Int32.Parse(competitor.Code, CultureInfo.InvariantCulture);
                            crewId.Value = (int)selectCommand.ExecuteScalar();

                            // Sync start times (point Id 1)
                            if (competitor.StartTime != null)
                            {
                                timeOfDay.Value = competitor.StartTime.TimeOfDay;
                                timingPointId.Value = 1;
                                command.ExecuteNonQuery();
                            }

                            // Sync Barnes times (point Id 2)
                            MultisportResult sectionResult = competitor.SectionTime(1);
                            if (sectionResult != null)
                            {
                                timeOfDay.Value = sectionResult.TimeOfDay;
                                timingPointId.Value = 2;
                                command.ExecuteNonQuery();
                            }

                            // Sync Hammersmith times (point Id 3)
                            sectionResult = competitor.SectionTime(2);
                            if (sectionResult != null)
                            {
                                timeOfDay.Value = sectionResult.TimeOfDay;
                                timingPointId.Value = 3;
                                command.ExecuteNonQuery();
                            }

                            // Sync finish times (point Id 4)
                            sectionResult = competitor.FinishTime;
                            if (sectionResult != null)
                            {
                                timeOfDay.Value = sectionResult.TimeOfDay;
                                timingPointId.Value = 4;
                                command.ExecuteNonQuery();
                            }

                            resultStatus.Value = (int)competitor.State;
                            statusCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ViewModel.IsConnecting = false;
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
