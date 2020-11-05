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
        public MySqlConnection Connection { get; }
        private readonly ModulrConfig _config;

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
            _config = config;
            Connection = new MySqlConnection(_config.MySqlConnection);
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
            var command =
                "INSERT INTO Modulr.Users (google_id, name, username, email) VALUES (@GoogleID, @Name, @Username, @Email) ON DUPLICATE KEY UPDATE google_id = @GoogleID, name = @Name, username = @Username, email = @Email;" +
                $"UPDATE Modulr.Users SET tests_remaining = {_config.TimeoutAttempts} WHERE tests_timeout < CURRENT_TIMESTAMP();";
            await Connection.ExecuteAsync(command,
                new {GoogleID = googleID, Name = name, Username = username, Email = email});
        }
        
        public async Task<UserTimeout> GetTimeOut(string googleID)
        {
            const string command = "SELECT tests_remaining, tests_timeout FROM Modulr.Users WHERE google_id = @GoogleID";
            var results = await Connection.QuerySingleOrDefaultAsync<UserTimeout>(command,
                new {GoogleID = googleID});
            if (results != null)
                results.Milliseconds = (long) (results.TestsTimeout - DateTimeOffset.Now).TotalMilliseconds;
            return results;
        }
        
        public async Task<List<Stipulatable>> GetAllTests()
        {
            const string command = "SELECT * FROM Modulr.Stipulatables";
            return (await Connection.QueryAsync(command)).ToList().Select(o =>
            {
                var testers = JsonConvert.DeserializeObject<List<string>>(o.testers);
                var required = JsonConvert.DeserializeObject<List<string>>(o.required);
                return new Stipulatable(o.id, o.name, testers, required);
            }).ToList();
        }

        public async Task DecrementAttempts(string googleID)
        {
            if (_config.TimeoutAttempts < 1)
                return;
            const string command =
                "UPDATE Modulr.Users SET tests_timeout = ADDTIME(CURRENT_TIMESTAMP(), '00:30:00') WHERE google_id = @GoogleID AND tests_remaining = 3;" +
                "UPDATE Modulr.Users SET tests_remaining = tests_remaining - 1 WHERE google_id = @GoogleID;";
            await Connection.ExecuteAsync(command, new { GoogleID = googleID } );
        }
        
        public async Task<Role> GetRole(string googleID)
        {
            const string command = "SELECT role FROM Modulr.Users WHERE google_id = @GoogleID";
            return await Connection.QuerySingleOrDefaultAsync<Role>(command, new { GoogleID = googleID } );
        }
    }
}