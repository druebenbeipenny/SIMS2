using System;

namespace SessionIdGenerator
{
    public class Program
    {
        // Dummy values for demonstration purposes
        private static readonly string ValidUsername = "user123";
        private static readonly string ValidPassword = "password123";

        public static void Main2(string[] args) //Only one main method possible
        {
            // Prompt for username and password
            Console.WriteLine("Enter Username:");
            string username = Console.ReadLine();

            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();

            // Validate the credentials
            if (IsValidUser(username, password))
            {
                // Generate a UUID (session ID) for the session
                Guid sessionId = GenerateUUIDv4();
                Console.WriteLine("Session ID generated: " + sessionId);
            }
            else
            {
                Console.WriteLine("Error: Invalid username or password.");
            }
        }

        private static bool IsValidUser(string username, string password)
        {
            // Simple validation against the hardcoded username and password
            return username == ValidUsername && password == ValidPassword;
        }

        private static Guid GenerateUUIDv4()
        {
            // Generates a random UUID (Version 4)
            return Guid.NewGuid();
        }
    }
}
