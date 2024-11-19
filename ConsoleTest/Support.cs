using System;

namespace ConsoleTest
{
    public class Support : User
    {
        public Support(string username) : base(username, false, true) { }

        public void DisplayMenu()
        {
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]Support Menu[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]1. Logout[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]2. List Incidents[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]3. Assign User to Incident[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]4. Change Incident Status[/]");
            Spectre.Console.AnsiConsole.MarkupLine("[yellow]0. Exit[/]");
        }
    }
}
