using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FluentChange.Blazor.Nuget
{
    public class MicroNuget
    {
        private readonly HttpClient Http;
        public MicroNuget()
        {
            Http = new HttpClient();
            Http.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/");
        }

        //https://docs.microsoft.com/de-de/nuget/api/package-base-address-resource#enumerate-package-versions

        public NugetVersionInfo Versions(string name)
        {
            name = name.ToLowerInvariant();
            var uri = name + "/index.json";
            var versions = Http.GetFromJsonAsync<NugetVersionInfo>(uri).Result;

            return versions;
        }


        public string Spec(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();
            var uri = name + "/" + version + "/" + name + ".nuspec";
            var specXML = Http.GetStringAsync(uri).Result;

            return specXML;
        }

        public byte[] Download(string name, string version)
        {
            name = name.ToLowerInvariant();
            version = version.ToLowerInvariant();
            var uri = name + "/" + version + "/" + name + "." + version + ".nupkg";
            var bytes = Http.GetByteArrayAsync(uri).Result;
            return bytes;
        }

        public List<string> Unpack(byte[] pkg)
        {
            using (var bytestream = new MemoryStream(pkg))
            using (var zip = new ZipArchive(bytestream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {


                    //using (var stream = entry.Open())
                    //{
                    //    // do whatever we want with stream
                    //    // ...
                    //}
                }
                return zip.Entries.Select(e => e.Name).ToList();
            }
        }
    }

    public class NugetVersionInfo
    {
        public string[] versions { get; set; }
    }



}
