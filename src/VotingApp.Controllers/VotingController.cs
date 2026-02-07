// VotingController.cs
using Microsoft.AspNetCore.Mvc;
using VotingApp.Models;
using VotingApp.Services;

namespace VotingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotingController : ControllerBase
    {
        private readonly IVotingService _votingService;
        
        public VotingController(IVotingService votingService)
        {
            _votingService = votingService;
        }
        
        [HttpPost("vote")]
        public async Task<IActionResult> CastVote([FromBody] VoteRequest request)
        {
            try
            {
                var vote = await _votingService.CastVoteAsync(
                    request.UserId, 
                    request.Party);
                    
                return Ok(new { 
                    success = true, 
                    message = "Vote cast successfully",
                    voteId = vote.Id 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }
        
        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            var results = await _votingService.GetCurrentResultsAsync();
            return Ok(results);
        }
        
        [HttpGet("has-voted/{userId}")]
        public async Task<IActionResult> HasUserVoted(int userId)
        {
            var hasVoted = await _votingService.HasUserVotedTodayAsync(userId);
            return Ok(new { hasVoted });
        }
    }
    
    public class VoteRequest
    {
        public int UserId { get; set; }
        public Party Party { get; set; }
    }
}
