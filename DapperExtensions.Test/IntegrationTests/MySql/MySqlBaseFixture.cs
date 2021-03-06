using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DapperExtensions.Test.IntegrationTests.MySql
{
    public class MySqlBaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            var connection = new MySqlConnection("Server=localhost;Database=dapperTest;Uid=dapperExtensions;Pwd=password;");
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new MySqlDialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);
            var files = new List<string>
                                {
                                    ReadScriptFile("CreateFooTable"),
                                    ReadScriptFile("CreateMultikeyTable"),
                                    ReadScriptFile("CreatePersonTable"),
                                    ReadScriptFile("CreateCarTable"),
                                    ReadScriptFile("CreateAnimalTable")
                                };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        public string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}