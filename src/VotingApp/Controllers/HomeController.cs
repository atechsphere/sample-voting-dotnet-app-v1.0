using Microsoft.AspNetCore.Mvc;
using VotingApp.Models;
using VotingApp.Services;

namespace VotingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVotingService _votingService;
        
        public HomeController(IVotingService votingService)
        {
            _votingService = votingService;
        }
        
        public async Task<IActionResult> Index()
        {
            var results = await _votingService.GetCurrentResultsAsync();
            return View(results);
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
