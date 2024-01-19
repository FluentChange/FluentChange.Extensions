using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

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
            Console.WriteLine("    Status " + database.Status);
            for (int i = 0; i <= 30; i++)
            {
                var dbStatus = sqlServer.Databases.Get(dbName);
                Console.WriteLine("  " + i + " Status " + dbStatus.Status);
                Task.Delay(1000);

            }

            Console.WriteLine("creating new db " + dbName + " on " + serverName + " - finished");
        }
    }
}
