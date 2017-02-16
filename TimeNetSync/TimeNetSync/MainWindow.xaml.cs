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
        private readonly IUnityContainer _container;
        public CategoryListViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new CategoryListViewModel();

            this.app = Application.Current as App;

            InitializeDrive();
            DataContext = ViewModel;
            FillViewModel();
        }

        private void FillViewModel()
        {
            SqlCeConnection connection = new SqlCeConnection(@"Data Source=""C:\Users\Public\Documents\Time.Net 2\Competitions\Test.timeNetCompetition""");
            connection.Open();
            SqlCeCommand command = new SqlCeCommand("SELECT Pos, Name FROM Classes", connection);
            SqlCeDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Category category = new Category();
                category.Pos = (int)reader[0];
                category.Name = (string)reader[1];
                ViewModel.Categories.Add(category);
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
    }
}
