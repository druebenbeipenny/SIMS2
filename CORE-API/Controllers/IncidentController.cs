using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;


namespace CORE_API.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class IncidentController : ControllerBase
    {
        private readonly ILogger<IncidentController> _logger;
        private readonly UserContext _user_db; // Entity Framework context
        private readonly IncidentContext _incident_db; // Entity Framework context
        private readonly IDatabase _redis_db;

        public IncidentController(ILogger<IncidentController> logger, UserContext userDb, IncidentContext incidentDb, IDatabase redisDb)
        {
            _logger = logger;
            _user_db = userDb;
            _incident_db = incidentDb;
            _redis_db = redisDb; 
        }

       
        // Create Incident  (send session id in url like this: ?sessionId=098379832974762)
        [HttpPost("createIncident")]
        public async Task<IActionResult> CreateIncident(string sessionId, [FromBody] Incident incident)
        {
            bool loggedIn = isLoggedIn(sessionId);
            if (!loggedIn)
            {
                return Unauthorized("You need to be logged in to create an incident");
            }
            DateTime currentDateTime = DateTime.UtcNow;
            incident.CreatedAt = currentDateTime;
            incident.UpdatedAt = currentDateTime;
                
            await _incident_db.Incidents.AddAsync(incident);
            await _incident_db.SaveChangesAsync();
            //_redis_db.HashSet(sessionId, "last_action", DateTime.Now.ToString("o")); //Update Session by action (extension)
            return Ok("Incident created successfully.");
        }

        // Assign a user to an incident (missing: check if the user is a supporter and has the required level)
        [HttpPost("assignUserToIncident")]
        public async Task<IActionResult> AssignUserToIncident(string sessionId, [FromBody] AssignIncidentBody assign)
        {
            bool isadmin = isAdmin(sessionId);
            bool issupport = isSupport(sessionId);

            if (!(isadmin || issupport))
            {
                return Unauthorized("You do not have the permission to assign an user an incident");
            }
            var incident = await _incident_db.Incidents.FindAsync(assign.incidentId); //Get incident by the incidentid
            if (incident == null)
                return NotFound("Incident not found.");

            var user = _user_db.Users.SingleOrDefault(user => user.Username == assign.username); //Get user by username
            if (user == null)
                return NotFound("User not found.");

            if (!user.IsSupport && !user.IsAdmin) //Checks if the user the incident shall be assigned to is a supporter or admin
            {
                return Unauthorized("User is not authorized to handle this incident.");
            }

            //Update the incident and save the changes in DB
            incident.AssignedUserId = user.Id;
            incident.UpdatedAt = DateTime.UtcNow;
            await _incident_db.SaveChangesAsync();
            //_redis_db.HashSet(sessionId, "last_action", DateTime.Now.ToString("o")); //Update Session by action (extension)

            return Ok("User assigned to incident successfully.");
        }

        // List all incidents for a specific user (should we implement a permission check?)
        [HttpGet("listIncidents")]
        public async Task<IActionResult> ListIncidents(string sessionId, string username) //TODO add check if user is user
        {
            //Need this because we need to convert the username to the userid
            User user = _user_db.Users.SingleOrDefault(user => user.Username == username); // Get user by username

            List<Incident> incidents = _incident_db.Incidents.Where(i => i.AssignedUserId == user.Id).ToList(); //get all incident that are asigned to the user with the userid

            if (incidents == null || incidents.Count == 0)
                return Ok(new List<Incident>()); //Return empty list if there are no incidents for this userId

            return Ok(incidents);
        }

        // Change incident status (OPEN, INPROGRESS, DONE, REOPEN, CLOSE)
        [HttpPost("changeIncidentStatus")]
        public async Task<IActionResult> ChangeIncidentStatus(string sessionId, [FromBody] SetIncidentStatusBody body)
        {
            bool isadmin = isAdmin(sessionId);
            bool issupport = isSupport(sessionId);

            if (!(isadmin || issupport))
            {
                return Unauthorized("You do not have the permission to change an incident status");
            }
            var incident = _incident_db.Incidents.Find(body.incidentStatus); //Gets the incidend by id
            if (incident == null)
            {
                return NotFound("Incident not found.");
            }
            
            //Update Status and save
                incident.Status = body.incidentStatus;
                incident.UpdatedAt = DateTime.UtcNow;

                _incident_db.SaveChanges();
                return Ok("Incident status updated successfully.");
            
        }

        // Delete Incident - Admin only
        [HttpDelete("deleteIncident")]
        public async Task<IActionResult> DeleteIncident(string sessionId, int incidentId)
        {

            bool isadmin = isAdmin(sessionId);

            if (!(isadmin))
            {
                return Unauthorized("You do not have the permission to delete an incident");
            }
            Incident incident = _incident_db.Incidents.SingleOrDefault(incident => incident.Id == incidentId);
            if (incident == null)
            {
                return NotFound("This incident does not exist");
            }
            _incident_db.Incidents.Remove(incident);
            _incident_db.SaveChanges();


            return Ok("Incident deleted successfully.");
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

                User currentUser = _user_db.Users.SingleOrDefault(user => user.Username == tUsername);

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


        private bool isSupport(string sessionId)
        {
            try //Added checks so it cannot fail
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return false;
                }
                string tUsername = _redis_db.HashGet(sessionId, "username");

                User currentUser = _user_db.Users.SingleOrDefault(user => user.Username == tUsername);

                if (currentUser == null)
                {
                    return false;
                }
                return currentUser.IsSupport;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool isLoggedIn(string sessionId) {
            RedisValue r1 = _redis_db.KeyExists(sessionId);
            if (!((bool)r1))
            {
                return false;
            }
            return true;
        }

    }

    public class AssignIncidentBody
    {
        public int incidentId {  get; set; }
        public string username { get; set; }
    }

    public class SetIncidentStatusBody
    {
        public int sessionId { get; set; }
        public IncidentStatus incidentStatus { get; set; }
    }

}
