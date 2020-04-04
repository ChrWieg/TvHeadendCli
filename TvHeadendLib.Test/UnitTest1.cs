using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;

namespace TvHeadendLib.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tvHeadend = new TvHeadend("http://192.168.63.10:9981/", true);
            var version = tvHeadend.GetTvHeadendVersion();
        }
    }
}
