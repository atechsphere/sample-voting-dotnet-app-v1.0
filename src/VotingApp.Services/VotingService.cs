using VotingApp.Data;
using VotingApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VotingApp.Services
{
    public class VotingService : IVotingService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public VotingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<User?> RegisterUserAsync(string username, string email, string password)
        {
            // Implementation placeholder - in real app, hash password
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }
        
        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            
            return null;
        }
        
        public async Task<bool> HasUserVotedTodayAsync(int userId)
        {
            var votes = await _unitOfWork.Votes.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            return votes.Any(v => v.UserId == userId && v.VotedAt.Date == today);
        }
        
        public async Task<Vote> CastVoteAsync(int userId, Party party)
        {
            if (await HasUserVotedTodayAsync(userId))
                throw new InvalidOperationException("User has already voted today");
                
            var vote = new Vote
            {
                UserId = userId,
                SelectedParty = party,
                VotedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Votes.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();
            
            return vote;
        }
        
        public async Task<VotingResult> GetCurrentResultsAsync()
        {
            var votes = await _unitOfWork.Votes.GetAllAsync();
            
            return new VotingResult
            {
                RepublicanVotes = votes.Count(v => v.SelectedParty == Party.Republican),
                DemocratVotes = votes.Count(v => v.SelectedParty == Party.Democrat)
            };
        }
    }
}
