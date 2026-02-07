using VotingApp.Models;
using System.Threading.Tasks;

namespace VotingApp.Services
{
    public interface IVotingService
    {
        Task<User?> RegisterUserAsync(string username, string email, string password);
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> HasUserVotedTodayAsync(int userId);
        Task<Vote> CastVoteAsync(int userId, Party party);
        Task<VotingResult> GetCurrentResultsAsync();
    }
}
