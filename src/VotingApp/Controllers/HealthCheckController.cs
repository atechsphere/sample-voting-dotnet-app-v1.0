using Microsoft.AspNetCore.Mvc;

namespace VotingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new 
            { 
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Voting Application"
            });
        }
    }
}
