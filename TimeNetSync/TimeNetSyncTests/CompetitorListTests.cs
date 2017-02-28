using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeNetSync.ViewModel;

namespace TimeNetSyncTests
{
    [TestClass]
    public class CompetitorListTests
    {
        [TestMethod]
        public void ToggleConnectCommand_ShouldToggleIsConnecting()
        {
            CompetitorListViewModel viewModel = new CompetitorListViewModel();

            bool isConnecting = viewModel.IsConnecting;
            viewModel.ToggleConnectingCommand.Execute(null);
            Assert.AreNotEqual(isConnecting, viewModel.IsConnecting);
        }
    }
}
