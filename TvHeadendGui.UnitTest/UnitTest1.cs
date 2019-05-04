using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TvHeadendLib.Helper;
using TvHeadendLib.Interfaces;
using TvHeadendLib.Models;

namespace TvHeadend.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        //private readonly Credential _credentials = CredentialHelper.GetStoredCredential();
        private const string Url = "http://pihole:9981";

        [TestMethod]
        public void TestChannels()
        {
            ITvHeadend tvHeadEnd = new TvHeadendLib.TvHeadend(Url,null);

            Assert.IsTrue(tvHeadEnd.Channels.Count > 0);

            foreach (var channel in tvHeadEnd.Channels)
                Console.WriteLine(channel);
        }

        [TestMethod]
        public void TestRecordings()
        {

            ITvHeadend tvHeadEnd = new TvHeadendLib.TvHeadend(Url, null);

            Assert.IsTrue(tvHeadEnd.Recordings.Count > 0);

            foreach (var recording in tvHeadEnd.Recordings)
                Console.WriteLine(recording);
        }

        [TestMethod]
        public void TestCreateRecording()
        {

            ITvHeadend tvHeadEnd = new TvHeadendLib.TvHeadend(Url, null);
            //http://pihole:9981/api/dvr/entry/create?conf={"start":1555587900,"stop":1555596000,"channel":"f1351106ed1b6872d85bbf2eab0e93c9","pri":2,"title":{"ger":"Die Prinzessin von Montpensier"},"subtitle":{"ger":"(2009)"}}

            if (!DateTime.TryParse("18.04.2019 13:37:30", out var start) || !DateTime.TryParse("18.04.2019 16:15:00", out var stop)) return;

            var recordingToCreate = new Recording
            {
                Channel = "f1351106ed1b6872d85bbf2eab0e93c9",
                Comment ="Created by Tv-Browser",
                Priority = 2,
                Start = start,
                Stop = stop,
                Title = "Die Prinzessin von Montpensier",
                SubTitle = "(2009)"
            };

            var recording = tvHeadEnd.CreateRecording(recordingToCreate);

            Assert.IsFalse(string.IsNullOrWhiteSpace(recording.Uuid));
            Console.WriteLine(recording.Uuid);

            var success = tvHeadEnd.RemoveRecordingSchedule(recordingToCreate);

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestHelper()
        {

            //Create
            //http://pihole:9981/api/dvr/entry/create?conf={"start":1555587900,"stop":1555596000,"channel":"f1351106ed1b6872d85bbf2eab0e93c9","pri":2,"title":{"ger":"Die Prinzessin von Montpensier"},"subtitle":{"ger":"(2009)"}}
            /*
             *
            {"Channel":"f1351106ed1b6872d85bbf2eab0e93c9","Comment":"Created by Tv-Browser","Pri":2,"Start":1555594650,"Stop":1555604100,"SubTitle":{"Ger":"(2009)"},"Title":{"Ger":"Die Prinzessin von Montpensier"}}
            */

            Console.WriteLine("Orig:");
            Console.WriteLine(UnixTimeConverter.GetDateTimeFromUnixTime(1555587900));
            Console.WriteLine(UnixTimeConverter.GetDateTimeFromUnixTime(1555596000));

            Console.WriteLine("Auto:");
            Console.WriteLine(UnixTimeConverter.GetDateTimeFromUnixTime(1555587450));
            Console.WriteLine(UnixTimeConverter.GetDateTimeFromUnixTime(1555596900));



            var nowDateTime = DateTime.Now.TrimToSeconds();

            var unixTimestamp = UnixTimeConverter.GetUnixTimeFromDateTime(nowDateTime);

            var csharpDateTime = UnixTimeConverter.GetDateTimeFromUnixTime(unixTimestamp);

            Console.WriteLine($"{nowDateTime.Ticks} => {csharpDateTime.Ticks}");

            Assert.AreEqual(nowDateTime,csharpDateTime);
        }


        [TestMethod]
        public void TestCredentials()
        {
            //CredentialHelper.ResetCredentials();

            var credential = CredentialHelper.GetStoredCredential(true);

            Assert.IsNotNull(credential);

            Console.WriteLine($"{credential.UserName} - {credential.Password}");

        }

        [TestMethod]

        public void TestCredentialSetting()
        {
            //CredentialHelper.ResetCredentials();
            var credentialStoreServiceName = CredentialHelper.GetCredentialStoreServiceName();

            Console.WriteLine(credentialStoreServiceName);

            //Assert.IsNotNull(credential);

            //Console.WriteLine($"{credential.UserName} - {credential.Password}");

        }

    }
}
