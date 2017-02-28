using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeNetSync.Model;

namespace TimeNetSyncTests
{
    [TestClass]
    public class CompetitorTests
    {
        private Competitor competitor;
        private MultisportResult resultZero;
        private MultisportResult resultOne;

        [TestInitialize]
        public void Initialize()
        {
            competitor = new Competitor();
            resultZero = new MultisportResult();
            resultZero.Section = 0;
            resultZero.TimeOfDay = new TimeSpan(7, 0, 0);
            competitor.Results.Add(resultZero);

            resultOne = new MultisportResult();
            resultOne.Section = 1;
            resultOne.TimeOfDay = new TimeSpan(7, 30, 0);
            competitor.Results.Add(resultOne);
        }

        [TestMethod]
        public void StartTime_ShouldReturnTimeForSegmentZero()
        {
            Assert.AreSame(resultZero, competitor.StartTime);
        }

        [TestMethod]
        public void FinishTime_ShouldReturnTimeForSegmentOne()
        {
            Assert.AreSame(resultOne, competitor.FinishTime);
        }

        [TestMethod]
        public void RunTime_ShouldReturnDifferenceBetweenStartAndFinishTimes()
        {
            Assert.AreEqual(new TimeSpan(0, 30, 0), competitor.RunTime);
        }
    }
}
