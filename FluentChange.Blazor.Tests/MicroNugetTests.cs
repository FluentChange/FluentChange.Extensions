using FluentChange.Blazor.Nuget;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FluentChange.Blazor.Tests
{
    [TestClass]
    public class MicroNugetTests
    {
        private readonly MicroNuget nuget;
      
        public MicroNugetTests()
        {
            nuget = new MicroNuget();
        }

        [TestMethod]
        public void TestVersionsSuccess()
        {                     
            var versions = nuget.Versions("FluentChange.Blazor");
            Assert.IsTrue(versions.versions.Length > 0);
        }

        [TestMethod]
        public void TestVersionsFail()
        {
            try
            {         
                var versions = nuget.Versions("FluentChange.NotExistent");               
            }
            catch (Exception ex)
            {
                Assert.AreEqual("One or more errors occurred. (Response status code does not indicate success: 404 (Not Found).)", ex.Message);
            }
        }

        [TestMethod]
        public void TestSpecSuccess()
        {
            var spec = nuget.Spec("FluentChange.Blazor","0.0.2");
            Assert.IsTrue(spec.StartsWith("<?xml"));
            Assert.IsTrue(spec.Contains("<id>FluentChange.Blazor</id>"));
            Assert.IsTrue(spec.Contains("<version>0.0.2</version>"));
            Assert.IsTrue(spec.EndsWith("</package>"));
        }

        [TestMethod]
        public void TestSpecFail()
        {
            try
            {
                var spec = nuget.Spec("FluentChange.NotExistent", "0.0.2");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("One or more errors occurred. (Response status code does not indicate success: 404 (Not Found).)", ex.Message);
            }
        }


        [TestMethod]
        public void TestDownloadSuccess()
        {
            var pkg = nuget.Download("FluentChange.Blazor", "0.0.2");
            Assert.IsTrue(pkg.Length > 0);
        }

        [TestMethod]
        public void TestDownloadFail()
        {
            try
            {
                var spec = nuget.Download("FluentChange.NotExistent", "0.0.2");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("One or more errors occurred. (Response status code does not indicate success: 404 (Not Found).)", ex.Message);
            }
        }

        [TestMethod]
        public void TestUnpackSuccess()
        {
            var pkg = nuget.Download("FluentChange.Blazor", "0.0.2");
            var entries = nuget.Unpack(pkg);
            foreach(var e in entries)
            {
                Console.WriteLine(e);
            }
         
            Assert.IsTrue(entries.Count > 0);
        }
    }
}
