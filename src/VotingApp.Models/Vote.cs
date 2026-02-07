namespace VotingApp.Models
{
    public enum Party
    {
        Republican,
        Democrat
    }

    public class Vote
    {
        public int Id { get; set; }
        public Party SelectedParty { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}

