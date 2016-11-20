using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TimeNetSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public dbnetsoft.Communication.Communication Communication { get; set; }
        public dbnetsoft.Communication.CommunicationSettings PortSettings { get; set; }
        public string[] Scopes = { SheetsService.Scope.Spreadsheets };

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            PortSettings = dbnetsoft.Communication.CommunicationSettings.CreateBroadcastClient(0x2d3d);
            PortSettings.Layer = dbnetsoft.Communication.CommunicationSettings.enumLayer.BroadcastClient;
            Communication = new dbnetsoft.Communication.Communication(PortSettings); // Removed SynchronizeInvokeWrapper - not clear what it does

            MainWindow wnd = new MainWindow();
            // Show the window
            wnd.Show();
        }
    }
}
