using Xunit;
using Moq;
using VotingApp.Models;
using VotingApp.Services;
using VotingApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VotingApp.Tests
{
    public class VotingServiceTests
    {
        [Fact]
        public async Task CastVote_UserHasNotVotedToday_ShouldSucceed()
        {
            // Arrange
            var mockVoteRepo = new Mock<IRepository<Vote>>();
            var mockUserRepo = new Mock<IRepository<User>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            
            mockVoteRepo.Setup(r => r.GetAllAsync())
                       .ReturnsAsync(new List<Vote>());
            
            mockUnitOfWork.Setup(u => u.Votes).Returns(mockVoteRepo.Object);
            mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            
            var service = new VotingService(mockUnitOfWork.Object);
            
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                service.CastVoteAsync(1, Party.Republican));
                
            Assert.Null(exception);
        }
        
        [Fact]
        public async Task CastVote_UserHasVotedToday_ShouldThrowException()
        {
            // Arrange
            var todayVote = new Vote 
            { 
                UserId = 1, 
                SelectedParty = Party.Republican,
                VotedAt = DateTime.UtcNow.Date.AddHours(10)
            };
            
            var mockVoteRepo = new Mock<IRepository<Vote>>();
            mockVoteRepo.Setup(r => r.GetAllAsync())
                       .ReturnsAsync(new List<Vote> { todayVote });
            
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.Votes).Returns(mockVoteRepo.Object);
            
            var service = new VotingService(mockUnitOfWork.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.CastVoteAsync(1, Party.Democrat));
        }
        
        [Fact]
        public async Task HasUserVotedToday_WhenUserVotedToday_ReturnsTrue()
        {
            // Arrange
            var votes = new List<Vote>
            {
                new Vote 
                { 
                    Id = 1,
                    UserId = 1, 
                    SelectedParty = Party.Republican,
                    VotedAt = DateTime.UtcNow.Date.AddHours(10) 
                }
            };
            
            var mockVoteRepo = new Mock<IRepository<Vote>>();
            mockVoteRepo.Setup(r => r.GetAllAsync())
                       .ReturnsAsync(votes);
            
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.Votes).Returns(mockVoteRepo.Object);
            
            var service = new VotingService(mockUnitOfWork.Object);
            
            // Act
            var result = await service.HasUserVotedTodayAsync(1);
            
            // Assert
            Assert.True(result);
        }
        
        [Theory]
        [InlineData(10, 5, 66.7, 33.3)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(100, 0, 100, 0)]
        [InlineData(75, 25, 75.0, 25.0)]
        public void VotingResult_CalculatesPercentages_Correctly(
            int republicanVotes, 
            int democratVotes, 
            double expectedRepPercent, 
            double expectedDemPercent)
        {
            // Arrange & Act
            var result = new VotingResult
            {
                RepublicanVotes = republicanVotes,
                DemocratVotes = democratVotes
            };
            
            // Assert
            Assert.Equal(expectedRepPercent, result.RepublicanPercentage, 1);
            Assert.Equal(expectedDemPercent, result.DemocratPercentage, 1);
            Assert.Equal(republicanVotes + democratVotes, result.TotalVotes);
        }
    }
}
