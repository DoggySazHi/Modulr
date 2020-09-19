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
    public class MySqlQuery : IDisposable
    {
        public MySqlConnection Connection { get; private set; }

        public void Dispose()
        {
            if(Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        ~MySqlQuery()
        {
            Dispose();
        }

        public MySqlQuery(ModulrConfig config)
        {
            Connection = new MySqlConnection(config.MySqlConnection);
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }
        
        public async Task<Stipulatable> GetTest(int id)
        {
            const string command = "SELECT * FROM Modulr.Stipulatables WHERE id = @ID";
            var results = (await Connection.QueryAsync(command, new {ID = id})).FirstOrDefault();
            if (results == null)
                return null;
            var testers = JsonConvert.DeserializeObject<IEnumerable<string>>(results.testers);
            var required = JsonConvert.DeserializeObject<IEnumerable<string>>(results.required);
            return new Stipulatable(results.id, results.name, testers, required);
        }

        public async Task<int> AddTest(string name, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string command = "INSERT INTO Modulr.Stipulatables (name, testers, required) VALUES (name = @Name, testers = @Testers, required = @Required); SELECT LAST_INSERT_ID();";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(command,
                new {Name = name, Testers = testers, Required = required});
            return results;
        }
        
        public async Task Register(string googleID, string name, string email, string username = null)
        {
            username ??= name;
            const string command = "INSERT INTO Modulr.Users (google_id, name, username, email) VALUES (@GoogleID, @Name, @Username, @Email) ON DUPLICATE KEY UPDATE google_id = @GoogleID, name = @Name, username = @Username, email = @Email";
            await Connection.ExecuteAsync(command,
                new {GoogleID = googleID, Name = name, Username = username, Email = email});
        }
        
        public async Task<UserTimeout> GetTimeOut(string googleID)
        {
            const string command = "SELECT tests_remaining, tests_timeout FROM Modulr.Stipulatables WHERE google_id = @GoogleID";
            var results = await Connection.QuerySingleOrDefaultAsync<UserTimeout>(command,
                new {GoogleID = googleID});
            return results;
        }
        
        public async Task<IEnumerable<Stipulatable>> GetAllTests()
        {
            const string command = "SELECT * FROM Modulr.Stipulatables";
            return (await Connection.QueryAsync(command)).ToList().Select(o =>
            {
                var testers = JsonConvert.DeserializeObject<IEnumerable<string>>(o.testers);
                var required = JsonConvert.DeserializeObject<IEnumerable<string>>(o.required);
                return new Stipulatable(o.id, o.name, testers, required);
            });
        }
    }
}