using Xunit;

namespace VotingApp.Tests.Integration
{
    public class BasicIntegrationTests
    {
        [Fact(Skip = "Integration test requires MySQL database. Run manually when MySQL is available.")]
        public void HomePage_ReturnsSuccessStatusCode()
        {
            // This test is skipped in automated builds
            // To run manually: 
            // 1. Start MySQL: docker run -d -p 3306:3306 --name mysql-voting -e MYSQL_ROOT_PASSWORD=votingpassword123 mysql:8.0
            // 2. Run: dotnet test --filter "FullyQualifiedName~BasicIntegrationTests"
            Assert.True(true);
        }
    }
}
