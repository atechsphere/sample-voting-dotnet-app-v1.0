using System;
using System.Threading.Tasks;
using VotingApp.Models;

namespace VotingApp.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Vote> Votes { get; }
        Task<int> SaveChangesAsync();
    }
}
