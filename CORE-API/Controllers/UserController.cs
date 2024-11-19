using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CORE_API.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserContext _mssql_db; // Entity Framework context
        private readonly IDatabase _redis_db;

        public UserController(ILogger<UserController> logger, UserContext mssql_db, IDatabase redis_db)
        {
            _logger = logger;
            _mssql_db = mssql_db;
            _redis_db = redis_db;
        }

        // Create User
        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] UsernamePasswordBody request)
        {
            // Check if the user already exists
            User existing = _mssql_db.Users.SingleOrDefault(user => user.Username == request.Username);
            if (existing != null)
            {
                return Conflict("User does already exist");
            }

            // Hash the password
            var passwordHash = Utils.HashPassword(request.password);

            // Create a new user with the provided properties
            User user = new User
            {
                Username = request.Username,
                IsAdmin = request.IsAdmin,
                IsSupport = request.IsSupport,
                SupportLevel = 0,
                PasswordHash = passwordHash
            };

            // Add the new user to the database
            _mssql_db.Users.Add(user);
            await _mssql_db.SaveChangesAsync();

            return Ok("User created successfully.");
        }


        // Delete User - Admin only or User ID match
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser(string sessionId, string username)
        {

            string tUsername = await _redis_db.HashGetAsync(sessionId, "username");
            User currentUser = _mssql_db.Users.SingleOrDefault(user => user.Username == tUsername);

            User user = _mssql_db.Users.SingleOrDefault(user => user.Username == username);
            if (user == null)
            {
                return NotFound("This user does not exist");
            }

            if (tUsername != username && !currentUser.IsAdmin) //Check if currentUser wants to delete his own account or is admin
            {
                return Unauthorized("You are unauthorized to delete this user");
            }
            _mssql_db.Users.Remove(user);
            _mssql_db.SaveChanges();
            

            return Ok("User deleted successfully.");
        }

        // Reset Password - Admin only or same user
        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(string sessionId, [FromBody] JustAUsername uPara)
        {
            User user = _mssql_db.Users.SingleOrDefault(user => user.Username == uPara.username);
            if (user == null)
            {
                return NotFound("This user does not exist");
            }

            if (!isAdmin(sessionId))
            {
                return Unauthorized("You do not have the permission to reset passwords");
            }

            String password = Utils.GeneratePassword(10);

            user.PasswordHash = Utils.HashPassword(password);
            _mssql_db.SaveChanges();

            return Ok("Password reset successfully to " + password);
        }

        // Change Password
        [HttpPost("changePassword/{userId}")]  //Maybe change so the user needs to supply his actual password and not a sessionid
        public async Task<IActionResult> ChangePassword(string sessionId, [FromBody] UsernamePasswordBody request)
        {
            RedisValue tUsername = await _redis_db.HashGetAsync(sessionId, "username");
            if (tUsername.IsNull)
            {
                return NotFound("This user does not exist");
            }
            User user = _mssql_db.Users.SingleOrDefault(user => user.Username == tUsername.ToString());
            if (user == null)
            {
                return NotFound("This user does not exist");
            }



            user.PasswordHash = Utils.HashPassword(request.password);
            _mssql_db.SaveChanges();

            return Ok("Password updated successfully");
        }


        //LOGIN
        [HttpPost("loginUser")]
        public async Task<IActionResult> loginUser([FromBody] UsernamePasswordBody request)
        {
            _logger.LogInformation($"Login attempt by user: {request.Username} at {DateTime.UtcNow}");

            User existing = _mssql_db.Users.SingleOrDefault(user => user.Username == request.Username);
            if (existing == null)
            {
                _logger.LogWarning($"Failed login attempt for username: {request.Username}");
                return NotFound("User does not exist");
            }

            var passwordHash = Utils.HashPassword(request.password);

            if (passwordHash != existing.PasswordHash)
            {
                _logger.LogWarning($"Invalid password for user: {request.Username}");
                return Unauthorized("Invalid credentials");
            }

            string sessionId = Guid.NewGuid().ToString();
            _logger.LogInformation($"User {request.Username} successfully logged in with session ID: {sessionId}");

            // Remaining login logic...
            return Ok(sessionId);
        }


        //LOGOUT
        [HttpPost("logoutUser")]
        public async Task<IActionResult> logoutUser(string sessionId)
        {

            bool success = _redis_db.KeyDelete(sessionId);

            if (success)
            {
                return Ok("Logged out sucessfully");
            }
            else
            {
                return NotFound("Session not found");
            }

        }

        //getPermissions
        [HttpGet("getPermissions")]
        public async Task<IActionResult> getPermissions(string sessionId)
        {

            //Get the username from redis from the sessionId
            var username = _redis_db.HashGet(sessionId,"username");
            if (username==RedisValue.Null)
            {
                return NotFound("No such sessionId"); //If the session is already deleted return 404
            }
            //Get the user object by the username
            User tUser = _mssql_db.Users.SingleOrDefault(user => user.Username == username.ToString());
            if (tUser == null)
            {
                return NotFound("No such sessionId");
            }
            //Set permission booleans
            bool isAdmin = tUser.IsAdmin;
            bool isSupport = tUser.IsSupport;

            var userPermissions = new { isAdmin, isSupport }; //Create a custom object based on the booleans (need to do this because of how JsonSerializer works)

            string json = JsonSerializer.Serialize(userPermissions); //Convert the permissionObject to json string

            return Ok($"{json}");   

        }

        // List all users
        [HttpGet("listUsers")]
        public async Task<IActionResult> ListUsers(string sessionId)
        {
            string tUsername = _redis_db.HashGet(sessionId, "username");
            //Need this because we need to convert the username to the userid
            User user = _mssql_db.Users.SingleOrDefault(user => user.Username == tUsername); // Get user by username

            //TODO Do we need a permission check here?
            /*if (!user.IsAdmin)
            {
                return Unauthorized("You are not authorized to request a user list");
            }*/

            // Select only username, admin, support. Do not want password hash in userlist
            var users = _mssql_db.Users.Select(u => new { u.Username, u.IsAdmin, u.IsSupport }).ToList();

            return Ok(users);
        }

        private bool isAdmin(string sessionId)
        {
            try //Added checks so it cannot fail
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return false;
                }
                string tUsername = _redis_db.HashGet(sessionId, "username");

                User currentUser = _mssql_db.Users.SingleOrDefault(user => user.Username == tUsername);

                if (currentUser == null)
                {
                    return false;
                }
                return currentUser.IsAdmin;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class UsernamePasswordBody
    {
        public required string Username { get; set; }
        public required string password { get; set; }
        public bool IsAdmin { get; set; } = false; // Default to false
        public bool IsSupport { get; set; } = false; // Default to false
    }


    public class JustAUsername
    {
        public required string username { get; set; }
    }

    

}
