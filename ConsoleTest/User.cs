using System;

namespace ConsoleTest
{
    public class User
    {
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSupport { get; set; }

        public User(string username, bool isAdmin, bool isSupport)
        {
            Username = username;
            IsAdmin = isAdmin;
            IsSupport = isSupport;
        }

        public void DisplayMenu()
        {
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]User Menu[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]1. Logout[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]2. Change Password[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]0. Exit[/]");
        }
    }
}