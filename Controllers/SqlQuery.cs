using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class SqlQuery : IDisposable
    {
        private IDbConnection Connection { get; }
        private readonly ModulrConfig _config;

        public void Dispose()
        {
            if(Connection.State != ConnectionState.Closed)
                Connection.Close();
            GC.SuppressFinalize(this);
        }

        ~SqlQuery()
        {
            Dispose();
        }

        static SqlQuery()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public SqlQuery(ModulrConfig config)
        {
            _config = config;
            if (config.UseMySql)
                Connection = new MySqlConnection(_config.MySqlConnection);
            else
                Connection = new SqlConnection(_config.SqlConnection);
        }
        
        /// <summary>
        /// Get a test from the database using the ID of the test, asynchronously.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <returns>An async task containing the <code>Stipulatable</code>.</returns>
        public async Task<Stipulatable> GetTest(int id)
        {
            const string commandMySql = "SELECT * FROM Modulr.Stipulatables WHERE id = @ID";
            var results = (await Connection.QueryAsync(ConvertSql(commandMySql), new {ID = id})).FirstOrDefault();
            if (results == null)
                return null;
            var description = results.description;
            var included = JsonConvert.DeserializeObject<IEnumerable<string>>(results.included);
            var testers = JsonConvert.DeserializeObject<IEnumerable<string>>(results.testers);
            var required = JsonConvert.DeserializeObject<IEnumerable<string>>(results.required);

            return new Stipulatable(results.id, results.name, description, included, testers, required);
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
        public async Task<int> AddTest(string name, string description, IEnumerable<string> included, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string commandMySql = "INSERT INTO Modulr.Stipulatables (`name`, testers, required, included, description) VALUES (@Name, @Testers, @Required, @Included, @Description); SELECT LAST_INSERT_ID();";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(ConvertSql(commandMySql),
                new { Name = name, Testers = JsonConvert.SerializeObject(testers), Required = JsonConvert.SerializeObject(required), Included = JsonConvert.SerializeObject(included), Description = description });
            return results;
        }

        /// <summary>
        /// Update a test's information by its ID.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <param name="name">The new name of the test.</param>
        /// <param name="description">The description associated with the test</param>
        /// <param name="included">The files that are available for the user to download.</param>
        /// <param name="testers">The new compile/include order for the test.</param>
        /// <param name="required">The files that are requested from the user.</param>
        /// <returns>Whether a test was updated or not.</returns>
        public async Task<bool> UpdateTest(int id, string name, string description, IEnumerable<string> included, IEnumerable<string> testers, IEnumerable<string> required)
        {
            const string commandMySql = "UPDATE Modulr.Stipulatables SET `name` = @Name, `description` = @Description, included = @Included, testers = @Testers, required = @Required WHERE id = @ID";
            return await Connection.ExecuteAsync(ConvertSql(commandMySql),
                new { Name = name,
                    Description = description,
                    Included = JsonConvert.SerializeObject(included),
                    Testers = JsonConvert.SerializeObject(testers),
                    Required = JsonConvert.SerializeObject(required),
                    ID = id }) != 0;
        }
        
        /// <summary>
        /// Delete a test by its ID.
        /// </summary>
        /// <param name="id">The ID of the test.</param>
        /// <returns>Whether a deletion occurred or not.</returns>
        public async Task<bool> DeleteTest(int id)
        {
            const string commandMySql = "DELETE FROM Modulr.Stipulatables WHERE id = @ID";
            return await Connection.ExecuteAsync(ConvertSql(commandMySql),new {ID = id}) != 0;
        }
        
        /// <summary>
        /// Put the user in the database.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="email">The email of the user. Must be unique.</param>
        /// <param name="googleID">The user's Google ID. May be optional, especially if they're using Modulr login.</param>
        /// <param name="username">The username of the user. May be optional, however it shouldn't change.</param>
        public async Task Register(string name, string email, string googleID = null, string username = null)
        {
            username ??= name;
            const string commandUserInsertMySql =
                "INSERT INTO Modulr.Users (google_id, name, username, email) VALUES (@GoogleID, @Name, @Username, @Email)";
            const string commandUserUpdateMySql =
                "UPDATE Modulr.Users SET username = @Username, email = @Email WHERE google_id = @GoogleID";
            if(!await UserExists(email))
                await Connection.ExecuteAsync(ConvertSql(commandUserInsertMySql),
                    new {GoogleID = googleID, Name = name, Username = username, Email = email});
            else
                await Connection.ExecuteAsync(ConvertSql(commandUserUpdateMySql),
                    new {GoogleID = googleID, Name = name, Username = username, Email = email});
            await ResetTestsRemaining();
        }

        /// <summary>
        /// Check whether a user exists.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <returns>Whether the user exists or not.</returns>
        public async Task<bool> UserExists(string email)
        {
            const string commandMySql = "SELECT COUNT(1) FROM Modulr.Users WHERE email = @Email;";
            var results = await Connection.QuerySingleOrDefaultAsync<int>(ConvertSql(commandMySql),
                new { Email = email });
            return results == 1;
        }
        
        /// <summary>
        /// Reset the test attempts remaining for all users, if their timeouts have expired.
        /// </summary>
        private async Task ResetTestsRemaining()
        {
            var attempts = _config.TimeoutAttempts <= 0 ? -1 : _config.TimeoutAttempts;
            var commandUpdateMySql = $"UPDATE Modulr.Users SET tests_remaining = {attempts} WHERE tests_timeout < CURRENT_TIMESTAMP()";
            await Connection.ExecuteAsync(ConvertSql(commandUpdateMySql));
        }
        
        /// <summary>
        /// Reset the timeout for a specific user.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        public async Task ResetTimeOut(int id)
        {
            const string commandMySql = "UPDATE Modulr.Users SET tests_timeout = CURRENT_TIMESTAMP() WHERE id = @ID";
            await Connection.QuerySingleOrDefaultAsync<UserTimeout>(ConvertSql(commandMySql), new {ID = id});
            await ResetTestsRemaining();
        }
        
        /// <summary>
        /// Get all tests available from Modulr.
        /// </summary>
        /// <returns>A list of all available tests.</returns>
        public async Task<List<Stipulatable>> GetAllTests()
        {
            const string commandMySql = "SELECT * FROM Modulr.Stipulatables ORDER BY id";
            return (await Connection.QueryAsync(ConvertSql(commandMySql))).ToList().Select(o =>
            {
                var included = JsonConvert.DeserializeObject<List<string>>(o.included);
                var testers = JsonConvert.DeserializeObject<List<string>>(o.testers);
                var required = JsonConvert.DeserializeObject<List<string>>(o.required);
                return new Stipulatable(o.id, o.name, o.description, included, testers, required);
            }).ToList();
        }

        /// <summary>
        /// Decrement the amount of attempts available for the user.
        /// </summary>
        /// <param name="id">The user's Modulr ID.</param>
        public async Task DecrementAttempts(int id)
        {
            if (_config.TimeoutAttempts < 1)
                return;
            const string commandMySql =
                "UPDATE Modulr.Users SET tests_timeout = ADDTIME(CURRENT_TIMESTAMP(), '00:30:00') WHERE id = @ID AND tests_remaining = @MaxTests;" +
                "UPDATE Modulr.Users SET tests_remaining = tests_remaining - 1 WHERE id = @ID;";
            await Connection.ExecuteAsync(ConvertSql(commandMySql), new { MaxTests = _config.TimeoutAttempts, ID = id } );
        }
        
        /// <summary>
        /// Get a role of a user.
        /// </summary>
        /// <param name="id">The user's Modulr ID.</param>
        /// <returns>The role(s) the user possesses.</returns>
        public async Task<Role> GetRole(int id)
        {
            const string commandMySql = "SELECT role FROM Modulr.Users WHERE id = @ID";
            return await Connection.QuerySingleOrDefaultAsync<Role>(ConvertSql(commandMySql), new { ID = id } );
        }
        
        /// <summary>
        /// Get all users that exist in the Modulr database.
        /// </summary>
        /// <returns>A list of all user data.</returns>
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            const string commandMySql = "SELECT * FROM Modulr.Users ORDER BY id;";
            return await Connection.QueryAsync<User>(ConvertSql(commandMySql));
        }
        
        /// <summary>
        /// Get a user by their Google ID or their login cookie.
        /// </summary>
        /// <param name="id">The Google ID or login cookie.</param>
        /// <returns>The user associated with the parameter.</returns>
        public async Task<User> ResolveUser(string id)
        {
            const string commandMySql = "SELECT * FROM Modulr.Users WHERE google_id = @Token OR login_cookie = @Token;";
            return await Connection.QuerySingleAsync<User>(ConvertSql(commandMySql), new { Token = id });
        }

        /// <summary>
        /// Get login information (password, salt, etc.) about a specific user.
        /// Note: fields can be null, especially if they are using Google logins.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        /// <returns>A <code>UserLogin</code> that contains the fields representing login information.</returns>
        public async Task<UserLogin> GetUserLogin(int id)
        {
            // Do not return the login if they're banned (bit-wise operation on 2)
            const string commandMySql = "SELECT password, salt, login_cookie, login_expire FROM Modulr.Users WHERE id = @ID AND role & 2 != 2;";
            return await Connection.QuerySingleOrDefaultAsync<UserLogin>(ConvertSql(commandMySql), new { ID = id });
        }
        
        /// <summary>
        /// Logout a user by resetting their login cookie.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        public async Task LogoutUser(int id)
        {
            const string commandMySql = "UPDATE Modulr.Users SET login_expiration = CURRENT_TIMESTAMP() WHERE id = @ID;";
            await Connection.ExecuteAsync(ConvertSql(commandMySql), new { ID = id });
        }
        
        /// <summary>
        /// Update the password and salt for the user. Other fields in the <code>UserLogin</code> are ignored.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        /// <param name="login">A <code>UserLogin</code> that contains both the password hash and the salt.</param>
        public async Task UpdateUserLogin(int id, UserLogin login)
        {
            const string commandMySql = "UPDATE Modulr.Users SET password = @Password, salt = @Salt WHERE id = @ID;";
            await Connection.ExecuteAsync(ConvertSql(commandMySql), new { login.Password, login.Salt, ID = id });
        }

        /// <summary>
        /// Update the cookie key and expiration for the user. Other fields in the <code>UserLogin</code> are ignored.
        /// </summary>
        /// <param name="id">The Modulr ID of the user.</param>
        /// <param name="login">A <code>UserLogin</code> that contains both the cookie key and expiration.</param>
        public async Task UpdateUserCookie(int id, UserLogin login)
        {
            const string commandMySql = "UPDATE Modulr.Users SET login_cookie = @LoginCookie, login_expiration = @LoginExpiration WHERE id = @ID;";
            await Connection.ExecuteAsync(ConvertSql(commandMySql), new { login.LoginCookie, login.LoginExpiration, ID = id });
        }

        /// <summary>
        /// Update a user's name and role. Usernames and emails cannot be updated through this method.
        /// </summary>
        /// <param name="u">A user. Only the ID will be used to search the database.</param>
        public async Task UpdateUser(User u)
        {
            const string commandMySql = "UPDATE Modulr.Users SET `name` = @Name, `role` = @Role WHERE id = @ID";
            await Connection.ExecuteAsync(ConvertSql(commandMySql), new {u.Name, Role = (int) u.Role, u.ID});
        }

        /// <summary>
        /// Convert the MySQL command into whatever the database requires.
        /// </summary>
        /// <param name="commandMySql">The command normally used for MySQL.</param>
        /// <returns>The same command or converted for MS-SQL.</returns>
        private string ConvertSql(string commandMySql)
        {
            if (_config.UseMySql)
                return commandMySql;
            return commandMySql
                .Replace("Modulr", "Tester")
                .Replace("ADDTIME(CURRENT_TIMESTAMP(), '00:30:00')", "DATEADD(MINUTE, 30, SYSDATETIMEOFFSET())")
                .Replace("CURRENT_TIMESTAMP()", "SYSDATETIMEOFFSET()")
                .Replace("LAST_INSERT_ID()", "SCOPE_IDENTITY()")
                .Replace("`", "");
        }
    }
}