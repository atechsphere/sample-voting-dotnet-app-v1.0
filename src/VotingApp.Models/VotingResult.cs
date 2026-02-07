namespace VotingApp.Models
{
    public class VotingResult
    {
        public int RepublicanVotes { get; set; }
        public int DemocratVotes { get; set; }
        public int TotalVotes => RepublicanVotes + DemocratVotes;
        public double RepublicanPercentage => TotalVotes > 0 ? (RepublicanVotes * 100.0) / TotalVotes : 0;
        public double DemocratPercentage => TotalVotes > 0 ? (DemocratVotes * 100.0) / TotalVotes : 0;
    }
}
