using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Modulr.Models;
using Modulr.Tester;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Modulr.Controllers
{
    public class MySqlTestQuery : IDisposable
    {
        private readonly MySqlConnection _connection;

        public void Dispose()
        {
            if(_connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        ~MySqlTestQuery()
        {
            Dispose();
        }

        public MySqlTestQuery(ModulrConfig config)
        {
            _connection = new MySqlConnection(config.MySqlConnection);
        }
        
        public async Task<Stipulatable> GetTest(int id)
        {
            const string command = "SELECT * FROM Modulr.Stipulatables WHERE id = @ID";
            var results = (await _connection.QueryAsync(command, new {ID = id})).FirstOrDefault();
            if (results == null)
                return null;
            var testers = JsonConvert.DeserializeObject<IEnumerable<string>>(results.testers);
            var required = JsonConvert.DeserializeObject<IEnumerable<string>>(results.required);
            return new Stipulatable(results.id, results.name, testers, required);
        }

        public async Task<int> AddTest(string name, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string command = "INSERT INTO Modulr.Stipulatables (name, testers, required) VALUES (name = @Name, testers = @Testers, required = @Required); SELECT LAST_INSERT_ID();";
            var results = await _connection.QuerySingleOrDefaultAsync<int>(command,
                new {Name = name, Testers = testers, Required = required});
            return results;
        }
    }
}