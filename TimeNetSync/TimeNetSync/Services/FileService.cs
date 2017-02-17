using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Services
{
    public class FileService : IFileService
    {
        public string OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".timeNetCompetition";
            openFileDialog.Filter = "Time.NET Files (*.timeNetCompetition)|*.timeNetCompetition";
            openFileDialog.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Time.NET 2\\Competitions");

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            else
                return string.Empty;
        }
    }
}
