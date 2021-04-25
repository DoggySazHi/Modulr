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
    /// <summary>
    /// A class that manages database connections to the Modulr backend.
    /// </summary>
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
        
        /// <summary>
        /// Get a test from the database using the ID of the test, asynchronously.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <returns>An async task containing the <code>Stipulatable</code>.</returns>
        public async Task<Stipulatable> GetTest(int id)
        {
            const string command = "SELECT * FROM Modulr.Stipulatables WHERE id = @ID";
            var results = (await Connection.QueryAsync(command, new {ID = id})).FirstOrDefault();
            if (results == null)
                return null;
            var description = results.description;
            var testers = JsonConvert.DeserializeObject<IEnumerable<string>>(results.testers);
            var required = JsonConvert.DeserializeObject<IEnumerable<string>>(results.required);
            var included = JsonConvert.DeserializeObject<IEnumerable<string>>(results.provided);

            return new Stipulatable(results.id, results.name, testers, required, description, included);
        }

        /// <summary>
        /// Add a test to the database with the following parameters.
        /// </summary>
        /// <param name="name">The name of the test.</param>
        /// <param name="testers">The order of compilation, as well as files that should be included.</param>
        /// <param name="required">Files that are requested from the user on the interface.</param>
        /// <returns>An integer, representing the ID of the <code>Stipulatable</code>.</returns>
        public async Task<int> AddTest(string name, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string command = "INSERT INTO Modulr.Stipulatables (`name`, testers, required) VALUES (@Name, @Testers, @Required); SELECT LAST_INSERT_ID();";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(command,
                new {Name = name, Testers = JsonConvert.SerializeObject(testers), Required = JsonConvert.SerializeObject(required)});
            return results;
        }

        /// <summary>
        /// Add a test to the database with the following parameters.
        /// </summary>
        /// <param name="name">The name of the test.</param>
        /// <param name="testers">The order of compilation, as well as files that should be included.</param>
        /// <param name="required">Files that are requested from the user on the interface.</param>
        /// <param name="description">A short description of what the test is about.</param>
        /// <param name="included">Files that are displayed for the user to download.</param>
        /// <returns>An integer, representing the ID of the <code>Stipulatable</code>.</returns>
        public async Task<int> AddTest(string name, IEnumerable<string> testers, IEnumerable<string> required, string description, IEnumerable<string> included)
        {
            const string command = "INSERT INTO Modulr.Stipulatables (`name`, testers, required) VALUES (@Name, @Testers, @Required); SELECT LAST_INSERT_ID();";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(command,
                new {Name = name, Testers = JsonConvert.SerializeObject(testers), Required = JsonConvert.SerializeObject(required)});
            return results;
        }
        
        /// <summary>
        /// Update a test's information by its ID.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <param name="name">The new name of the test.</param>
        /// <param name="testers">The new compile/include order for the test.</param>
        /// <param name="required">The new files</param>
        /// <returns>Whether a test was updated or not.</returns>
        public async Task<bool> UpdateTest(int id, string name, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string command = "UPDATE Modulr.Stipulatables SET `name` = @Name, testers = @Testers, required = @Required WHERE id = @ID";
            return await Connection.ExecuteAsync(command,
                new {Name = name,
                    Testers = JsonConvert.SerializeObject(testers),
                    Required = JsonConvert.SerializeObject(required),
                    ID = id}) != 0;
        }
        
        /// <summary>
        /// Delete a test by its ID.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <returns>Whether a deletion occurred or not.</returns>
        public async Task<bool> DeleteTest(int id)
        {
            const string command = "DELETE FROM Modulr.Stipulatables WHERE id = @ID";
            return await Connection.ExecuteAsync(command,new {ID = id}) != 0;
        }
        
        /// <summary>
        /// Update a user by their ID.
        /// </summary>
        /// <param name="googleID">The user's Google ID.</param>
        /// <param name="name">The name of the user.</param>
        /// <param name="email">The email of the user.</param>
        /// <param name="username">The username of the user. May be optional, however it shouldn't change.</param>
        public async Task Register(string googleID, string name, string email, string username = null)
        {
            username ??= name;
            const string commandUserInsert =
                "INSERT INTO Modulr.Users (google_id, name, username, email) VALUES (@GoogleID, @Name, @Username, @Email)";
            const string commandUserUpdate =
                "UPDATE Modulr.Users SET username = @Username, email = @Email WHERE google_id = @GoogleID";
            if(!await UserExists(googleID))
                await Connection.ExecuteAsync(commandUserInsert,
                    new {GoogleID = googleID, Name = name, Username = username, Email = email});
            else
                await Connection.ExecuteAsync(commandUserUpdate,
                    new {GoogleID = googleID, Name = name, Username = username, Email = email});
            await ResetTestsRemaining();
        }

        /// <summary>
        /// Check whether a user exists.
        /// </summary>
        /// <param name="googleID">The user's Google ID (NOT their Modulr ID).</param>
        /// <returns>Whether the user exists or not.</returns>
        public async Task<bool> UserExists(string googleID)
        {
            const string command = "SELECT COUNT(1) FROM Modulr.Users WHERE google_id = @GoogleID;";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(command,
                new {GoogleID = googleID});
            return results == 1;
        }
        
        /// <summary>
        /// Get the time left for the reset timer.
        /// </summary>
        /// <param name="googleID">The user's Google ID (NOT their Modulr ID).</param>
        /// <returns></returns>
        public async Task<UserTimeout> GetTimeOut(string googleID)
        {
            const string command = "SELECT tests_remaining, tests_timeout FROM Modulr.Users WHERE google_id = @GoogleID";
            var results = await Connection.QuerySingleOrDefaultAsync<UserTimeout>(command,
                new {GoogleID = googleID});
            if (results != null)
                results.Milliseconds = (long) (results.TestsTimeout - DateTimeOffset.Now).TotalMilliseconds;
            return results;
        }

        /// <summary>
        /// Reset the test attempts remaining for all users, if their timeouts have expired.
        /// </summary>
        private async Task ResetTestsRemaining()
        {
            var attempts = _config.TimeoutAttempts <= 0 ? -1 : _config.TimeoutAttempts;
            var commandUpdate = $"UPDATE Modulr.Users SET tests_remaining = {attempts} WHERE tests_timeout < CURRENT_TIMESTAMP()";
            await Connection.ExecuteAsync(commandUpdate);
        }
        
        /// <summary>
        /// Reset the timeout for a specific user.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        public async Task ResetTimeOut(int id)
        {
            const string command = "UPDATE Modulr.Users SET tests_timeout = CURRENT_TIMESTAMP() WHERE id = @ID";
            await Connection.QuerySingleOrDefaultAsync<UserTimeout>(command, new {ID = id});
            await ResetTestsRemaining();
        }
        
        /// <summary>
        /// Get all tests available from Modulr.
        /// </summary>
        /// <returns>A list of all available tests.</returns>
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

        /// <summary>
        /// Decrement the amount of attempts available for the user.
        /// </summary>
        /// <param name="googleID">The user's Google ID (NOT their Modulr ID).</param>
        public async Task DecrementAttempts(string googleID)
        {
            if (_config.TimeoutAttempts < 1)
                return;
            const string command =
                "UPDATE Modulr.Users SET tests_timeout = ADDTIME(CURRENT_TIMESTAMP(), '00:30:00') WHERE google_id = @GoogleID AND tests_remaining = @MaxTests;" +
                "UPDATE Modulr.Users SET tests_remaining = tests_remaining - 1 WHERE google_id = @GoogleID;";
            await Connection.ExecuteAsync(command, new { MaxTests = _config.TimeoutAttempts, GoogleID = googleID } );
        }
        
        /// <summary>
        /// Get a role of a user.
        /// </summary>
        /// <param name="googleID">The user's Google ID (NOT their Modulr ID).</param>
        /// <returns>The role(s) the user possesses.</returns>
        public async Task<Role> GetRole(string googleID)
        {
            const string command = "SELECT role FROM Modulr.Users WHERE google_id = @GoogleID";
            return await Connection.QuerySingleOrDefaultAsync<Role>(command, new { GoogleID = googleID } );
        }
        
        /// <summary>
        /// Get all users that exist in the Modulr database.
        /// </summary>
        /// <returns>A list of all user data.</returns>
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            const string command = "SELECT * FROM Modulr.Users;";
            return await Connection.QueryAsync<User>(command);
        }

        /// <summary>
        /// Update a user's name and role. Usernames and emails cannot be updated through this method.
        /// </summary>
        /// <param name="u">A user. Only the ID will be used to search the database.</param>
        public async Task UpdateUser(User u)
        {
            const string command = "UPDATE Modulr.Users SET `name` = @Name, `role` = @Role WHERE id = @ID";
            await Connection.ExecuteAsync(command, new {u.Name, Role = (int) u.Role, u.ID});
        }
    }
}