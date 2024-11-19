namespace CORE_API
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSupport { get; set; } 
        public int SupportLevel { get; set; }

        public required string PasswordHash { get; set; }

    }


}
