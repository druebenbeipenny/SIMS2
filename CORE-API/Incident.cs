namespace CORE_API
{
    public class Incident
    {

        /*
         * ID (auto generated in db)
         * title
         * description
         * severity / priority
         */

        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public IncidentLevel Severity { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required int CreatorId { get; set; }
        public IncidentStatus Status { get; set; }
        public int AssignedUserId { get; set; }


    }
}
