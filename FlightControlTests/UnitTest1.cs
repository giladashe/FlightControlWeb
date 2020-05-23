using FlightControlWeb.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net;


namespace FlightControlTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            /*var moq = new Mock<WebRequest>();
            Mock<HttpWebRequest> moqHttpRequest = new Mock<HttpWebRequest>();
            Mock<HttpWebResponse> moqHttpResponse = new Mock<HttpWebResponse>();
            List<Segment> segments = new List<Segment>();
            segments.Add(new Segment(20.3, 10.5, 200));
            segments.Add(new Segment(40.5, 20.5, 500));
            FlightPlan plan = new FlightPlan(100,"ELAL",new InitialLocation(30.3,40.5, "2020-11-27T01:56:22Z"), segments);
            moqHttpResponse.Setup(_ => _.StatusCode).Returns(HttpStatusCode.OK);

            moqHttpResponse.Setup(_ => _.GetResponseStream()).Returns(
                new MemoryStream(resultContentBytes));

            moqHttpRequest.Setup(_ => _.GetResponse()).Returns(moqHttpResponse.Object);
            moq.Setup(_ => _.Create(It.IsAny<string>())).Returns(moqHttpResponse.Object);

            var retriever = new HtmlRetriever(moq.Object);

            var result = retriever.Retrieve("test");

            result.Should().Be(resultContent);*/
        }
    }
}
