using System;
using System.Linq;
using System.IO;


namespace FluentChange.Common.Database.Azure
{
    public class DatabaseHelper
    {
        public static void DeleteAndCreateDatabase(string serverName, string dbName)
        {
            DeleteAndCreateDatabase(@"..\..\..\my.azureauth", serverName, dbName);
        }
        public static void DeleteAndCreateDatabase(string pathToAuthFile, string serverName, string dbName)
        {
            var basedir = AppDomain.CurrentDomain.BaseDirectory;
            var azureAuthFile = Path.Join(basedir, pathToAuthFile);

            var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(azureAuthFile);

            var defaultSub = azure.WithDefaultSubscription();
            var servers = defaultSub.SqlServers.List();

            var sqlServer = defaultSub.SqlServers.List().Single(s => s.Name == serverName);
            var db = sqlServer.Databases.Get(dbName);
            if (db != null)
            {
                Console.WriteLine("deleting existing db " + dbName + " on " + serverName + " - started");
                db.Delete();
                Console.WriteLine("deleting existing db " + dbName + " on " + serverName + " - finished");
            }
            Console.WriteLine("creating new db " + dbName + " on " + serverName + " - started");
            var database = sqlServer.Databases.Define(dbName).WithBasicEdition().Create();
            Console.WriteLine("creating new db " + dbName + " on " + serverName + " - finished");
        }
    }
}
