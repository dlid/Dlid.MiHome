using Dlid.MiHome.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Tests
{
    [TestClass]
    public class MiHomeRequestTests
    {

        [TestMethod]
        public void MiHomeRequest_RequestID()
        {

            var device = new MiDevice("192.168.0.119", "aabbcc");
            device._deviceId = new byte[] { 0x00, 0x11, 0x22, 0x33 };
            device._serverTimestamp = new Protocol.ServerTimestamp(91);

            var request = new MiHomeRequest(device._miToken, device._deviceId, device._serverTimestamp, device.NetworkOptions, new
            {
                method = "app_start"
            });

            var json = request.CreateJson();

            var jobj = JObject.Parse(json);
            Assert.AreEqual(2, jobj.Children().Count());
            Assert.IsNotNull(jobj.SelectToken("$.method"));
            Assert.AreEqual("app_start", (jobj.SelectToken("$.method") as JValue).Value);
            Assert.AreEqual((Int64)0, (jobj.SelectToken("$.id") as JValue).Value);
        }

        [TestMethod]
        public void MiHomeRequest_RequestID_Update()
        {

            var device = new MiDevice("192.168.0.119", "aabbcc");
            device._deviceId = new byte[] { 0x00, 0x11, 0x22, 0x33 };
            device._serverTimestamp = new Protocol.ServerTimestamp(91);

            var request = new MiHomeRequest(device._miToken, device._deviceId, device._serverTimestamp, device.NetworkOptions, new
            {
                method = "app_start"
            });

            request.RequestId++;

            var json = request.CreateJson();

            var jobj = JObject.Parse(json);
            Assert.AreEqual(2, jobj.Children().Count());
            Assert.IsNotNull(jobj.SelectToken("$.method"));
            Assert.AreEqual("app_start", (jobj.SelectToken("$.method") as JValue).Value);
            Assert.AreEqual((Int64)1, (jobj.SelectToken("$.id") as JValue).Value);
        }

        [TestMethod]
        public void MiHomeRequest_ProvidedID()
        {

            var device = new MiDevice("192.168.0.119", "aabbcc");
            device._deviceId = new byte[] { 0x00, 0x11, 0x22, 0x33 };
            device._serverTimestamp = new Protocol.ServerTimestamp(91);

            var request = new MiHomeRequest(device._miToken, device._deviceId, device._serverTimestamp, device.NetworkOptions, new
            {
                method = "app_start",
                id = "hello"
            });

            var json = request.CreateJson();

            var jobj = JObject.Parse(json);
            Assert.AreEqual(2, jobj.Children().Count());
            Assert.IsNotNull(jobj.SelectToken("$.method"));
            Assert.AreEqual("app_start", (jobj.SelectToken("$.method") as JValue).Value);
            Assert.AreEqual("hello", (jobj.SelectToken("$.id") as JValue).Value);
        }

        [TestMethod]
        public void MiHomeRequest_ProvidedID_Update()
        {

            var device = new MiDevice("192.168.0.119", "aabbcc");
            device._deviceId = new byte[] { 0x00, 0x11, 0x22, 0x33 };
            device._serverTimestamp = new Protocol.ServerTimestamp(91);

            var request = new MiHomeRequest(device._miToken, device._deviceId, device._serverTimestamp, device.NetworkOptions, new
            {
                method = "app_start",
                id = "hello"
            });

            // This should not affect the ID when it was provided in the original payload
            request.RequestId++;

            var json = request.CreateJson();

            var jobj = JObject.Parse(json);
            Assert.AreEqual(2, jobj.Children().Count());
            Assert.IsNotNull(jobj.SelectToken("$.method"));
            Assert.AreEqual("app_start", (jobj.SelectToken("$.method") as JValue).Value);
            Assert.AreEqual("hello", (jobj.SelectToken("$.id") as JValue).Value);
        }

        }
    }
