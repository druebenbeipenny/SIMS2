using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;

namespace ConsoleTest
{
    public class Program
    {
        private static string _sessionId = null;

        public static async Task Main(string[] args)
        {
            AnsiConsole.MarkupLine("[yellow]Welcome to Incident Management CLI[/]");
            while (true)
            {
                try
                {
                    if (_sessionId == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]\nMenu:[/]");
                        AnsiConsole.MarkupLine("[yellow]1. Login[/]");
                        AnsiConsole.MarkupLine("[yellow]0. Exit[/]");

                        int choice = PromptChoice();
                        if (choice == 0) break;
                        if (choice == 1) await Login();
                    }
                    else
                    {
                        await DisplayPermissions();

                        AnsiConsole.MarkupLine("[yellow]\nMenu:[/]");
                        AnsiConsole.MarkupLine("[yellow]1. Logout[/]");
                        AnsiConsole.MarkupLine("[yellow]2. Create User (Admin only)[/]");
                        AnsiConsole.MarkupLine("[yellow]3. Delete User (Admin only)[/]");
                        AnsiConsole.MarkupLine("[yellow]4. Reset Password (Admin only)[/]");
                        AnsiConsole.MarkupLine("[yellow]5. Change Password[/]");
                        AnsiConsole.MarkupLine("[yellow]0. Exit[/]");

                        int choice = PromptChoice();
                        if (choice == 0) break;
                        await HandleUserChoice(choice);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[yellow]Error: {ex.Message}[/]");
                }
            }
        }

        private static int PromptChoice()
        {
            AnsiConsole.Markup("[yellow]Choose an option: [/]");
            return int.Parse(Console.ReadLine());
        }
        private static async Task Login()
        {
            AnsiConsole.Markup("[yellow]Username: [/]");
            string username = Console.ReadLine();
            AnsiConsole.Markup("[yellow]Password: [/]");
            string password = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                var loginRequest = new { Username = username, Password = password };
                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("https://localhost:7022/api/User/loginUser", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the session ID from the response
                        _sessionId = await response.Content.ReadAsStringAsync();

                        // Provide user feedback and display the session ID
                        AnsiConsole.MarkupLine("[green]Login successful![/]");
                        AnsiConsole.MarkupLine($"[yellow]Session ID: {_sessionId}[/]");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Handle the case where the user is not found
                        AnsiConsole.MarkupLine("[red]Error: User not found or invalid credentials.[/]");
                    }
                    else
                    {
                        // Generic error handling for server-side issues
                        AnsiConsole.MarkupLine("[red]Login failed due to a server error.[/]");
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Catch network-related errors
                    AnsiConsole.MarkupLine($"[red]Network error: {ex.Message}[/]");
                }
                catch (Exception ex)
                {
                    // Catch any other exceptions
                    AnsiConsole.MarkupLine($"[red]Unexpected error: {ex.Message}[/]");
                }
            }
        }

        private static async Task Logout()
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent($"\"{_sessionId}\"", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://localhost:7022/api/User/logoutUser", content);

                if (response.IsSuccessStatusCode)
                {
                    _sessionId = null;
                    AnsiConsole.MarkupLine("[yellow]Logged out successfully.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to log out.[/]");
                }
            }
        }

        private static async Task CreateUser()
        {
            AnsiConsole.Markup("[yellow]Username: [/]");
            string username = Console.ReadLine();
            AnsiConsole.Markup("[yellow]Password: [/]");
            string password = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                var userRequest = new { Username = username, Password = password };
                var content = new StringContent(JsonSerializer.Serialize(userRequest), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://localhost:7022/api/User/createUser", content);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[yellow]User created successfully![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to create user.[/]");
                }
            }
        }

        private static async Task DeleteUser()
        {
            AnsiConsole.Markup("[yellow]Username to delete: [/]");
            string username = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                var requestUri = $"https://localhost:7022/api/User/deleteUser?sessionId={_sessionId}&username={username}";

                HttpResponseMessage response = await client.DeleteAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[yellow]User deleted successfully![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to delete user.[/]");
                }
            }
        }

        private static async Task ResetPassword()
        {
            AnsiConsole.Markup("[yellow]Username to reset password for: [/]");
            string username = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent($"\"{username}\"", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"https://localhost:7022/api/User/resetPassword?sessionId={_sessionId}", content);

                if (response.IsSuccessStatusCode)
                {
                    string newPassword = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[yellow]Password reset successfully! New password: {newPassword}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to reset password.[/]");
                }
            }
        }

        private static async Task ChangePassword()
        {
            AnsiConsole.Markup("[yellow]New Password: [/]");
            string password = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                var passwordRequest = new { Password = password };
                var content = new StringContent(JsonSerializer.Serialize(passwordRequest), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"https://localhost:7022/api/User/changePassword?sessionId={_sessionId}", content);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[yellow]Password changed successfully![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to change password.[/]");
                }
            }
        }

        private static async Task DisplayPermissions()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"https://localhost:7022/api/User/getPermissions?sessionId={_sessionId}");

                if (response.IsSuccessStatusCode)
                {
                    string permissions = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[yellow]Permissions: {permissions}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Error: Unable to retrieve permissions.[/]");
                }
            }
        }

        private static async Task HandleUserChoice(int choice)
        {
            switch (choice)
            {
                case 1:
                    await Logout();
                    break;
                case 2:
                    await CreateUser();
                    break;
                case 3:
                    await DeleteUser();
                    break;
                case 4:
                    await ResetPassword();
                    break;
                case 5:
                    await ChangePassword();
                    break;
                default:
                    AnsiConsole.MarkupLine("[yellow]Invalid choice.[/]");
                    break;
            }
        }
    }
}
