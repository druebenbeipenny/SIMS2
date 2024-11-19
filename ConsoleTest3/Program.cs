using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace ConsoleTest3
{
    public class Program
    {
        private static string _baseApiUrl = "https://localhost:7022/api";
        private static string _sessionId = null;
        private static bool _isAdmin = false;
        private static bool _isSupport = false;

        public static async Task Main(string[] args)
        {
            //Getting the apiUrl from config file
           IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            _baseApiUrl = configuration["baseApiUrl"];

            Console.WriteLine("Incident Management CLI");
            while (true)
            {
                try
                {
                    Console.WriteLine("\nMenu:");
                    if (_sessionId == null)
                    {
                        Console.WriteLine("1. Login");
                        Console.WriteLine("2. Create User");
                    }
                    else
                    {
                        Console.WriteLine("3. Logout");
                        if (_isAdmin)
                        {
                            Console.WriteLine("4. Delete User");
                            Console.WriteLine("5. Reset Password");
                        }
                        Console.WriteLine("6. Change Password");
                        Console.WriteLine("7. Create Incident");
                        if (_isSupport)
                        {
                            Console.WriteLine("8. Assign User to Incident");
                            Console.WriteLine("9. List Incidents");
                            Console.WriteLine("10. Change Incident Status");
                        }
                        if (_isAdmin)
                        {
                            Console.WriteLine("11. Delete Incident");
                        }
                    }
                    Console.WriteLine("0. Exit");

                    Console.Write("\nChoose an option: ");
                    int choice = int.Parse(Console.ReadLine());

                    if (choice == 0) break;

                    await HandleChoice(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private static async Task HandleChoice(int choice)
        {
            switch (choice)
            {
                case 1:
                    await Login();
                    break;
                case 2:
                    await CreateUser();
                    break;
                case 3:
                    await Logout();
                    break;
                case 4:
                    await DeleteUser();
                    break;
                case 5:
                    await ResetPassword();
                    break;
                case 6:
                    await ChangePassword();
                    break;
                case 7:
                    await CreateIncident();
                    break;
                case 8:
                    await AssignUserToIncident();
                    break;
                case 9:
                    await ListIncidents();
                    break;
                case 10:
                    await ChangeIncidentStatus();
                    break;
                case 11:
                    await DeleteIncident();
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        private static async Task Login()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            var requestBody = new { username, password };
            var response = await PostAsync("/User/loginUser", requestBody);

            if (response.IsSuccessStatusCode)
            {
                _sessionId = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Login successful. Session ID saved. " + _sessionId);

                //MAYBE add this to get user roles to the client. (But I think a little bit too much for this task)
                /*                var userResponse = await GetAsync($"/User/getUser?sessionId={_sessionId}&username={username}");
                                if (userResponse.IsSuccessStatusCode)
                                {
                                    var user = JsonSerializer.Deserialize<User>(await userResponse.Content.ReadAsStringAsync());
                                    _isAdmin = user.isAdmin;
                                    _isSupport = user.isSupport;
                                }*/
            }
            else
            {
                Console.WriteLine("Login failed." + response.ToString());
            }
        }

        private static async Task Logout()
        {
            var response = await PostAsync($"/User/logoutUser?sessionId={_sessionId}", null);
            if (response.IsSuccessStatusCode)
            {
                _sessionId = null;
                _isAdmin = false;
                _isSupport = false;
                Console.WriteLine("Logged out successfully.");
            }
            else
            {
                Console.WriteLine("Logout failed.");
            }
        }
        private static async Task CreateUser()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            var requestBody = new { username, password };
            var response = await PostAsync("/User/createUser", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task DeleteUser()
        {
            Console.Write("Username to delete: ");
            string username = Console.ReadLine();

            var response = await DeleteAsync($"/User/deleteUser?sessionId={_sessionId}&username={username}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task ResetPassword()
        {
            Console.Write("Username to reset password for: ");
            string username = Console.ReadLine();

            var requestBody = new { username };
            var response = await PostAsync($"/User/resetPassword?sessionId={_sessionId}", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task ChangePassword()
        {
            Console.Write("New Password: ");
            string password = Console.ReadLine();

            var requestBody = new { username = "currentUsername", password }; // Replace with actual username
            var response = await PostAsync($"/User/changePassword?sessionId={_sessionId}", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task CreateIncident()
        {
            Console.Write("Incident Description: ");
            string description = Console.ReadLine();

            var requestBody = new { description };
            var response = await PostAsync($"/Incident/createIncident?sessionId={_sessionId}", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task AssignUserToIncident()
        {
            Console.Write("Incident ID: ");
            int incidentId = int.Parse(Console.ReadLine());
            Console.Write("Username to assign: ");
            string username = Console.ReadLine();

            var requestBody = new { incidentId, username };
            var response = await PostAsync($"/Incident/assignUserToIncident?sessionId={_sessionId}", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task ListIncidents()
        {
            var response = await GetAsync($"/Incident/listIncidents?sessionId={_sessionId}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task ChangeIncidentStatus()
        {
            Console.Write("Incident ID: ");
            int incidentId = int.Parse(Console.ReadLine());
            Console.Write("New Status: ");
            string status = Console.ReadLine();

            var requestBody = new { incidentId, incidentStatus = status };
            var response = await PostAsync($"/Incident/changeIncidentStatus?sessionId={_sessionId}", requestBody);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task DeleteIncident()
        {
            Console.Write("Incident ID: ");
            int incidentId = int.Parse(Console.ReadLine());

            var response = await DeleteAsync($"/Incident/deleteIncident?sessionId={_sessionId}&incidentId={incidentId}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        //Bare Get/Post/Delete Translations (Take url and content and send request based on the method name)
        private static async Task<HttpResponseMessage> GetAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using HttpClient client = new HttpClient();
            return await client.GetAsync(_baseApiUrl + url);
        }

        private static async Task<HttpResponseMessage> PostAsync(string url, object requestBody)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using HttpClient client = new HttpClient(handler);
            string json = JsonSerializer.Serialize(requestBody);
            Console.WriteLine("JSON: " + json.ToString());
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync(_baseApiUrl + url, content);
        }

        private static async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using HttpClient client = new HttpClient(handler);
            return await client.DeleteAsync(_baseApiUrl + url);
        }

        private class User
        {
            public required string Username { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsSupport { get; set; }
        }
    }
}
