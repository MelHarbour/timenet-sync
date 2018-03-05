using Google.Apis.Sheets.v4;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TimeNetSync.Services;
using Unity;

namespace TimeNetSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string[] Scopes = { SheetsService.Scope.Spreadsheets };

        protected override void OnStartup(StartupEventArgs e)
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.RegisterType<IResultsOutput, GoogleSheetResultsOutput>();
                var wnd = container.Resolve<MainWindow>();
                // Show the window
                wnd.Show();
            }
        }
    }
}
