using System;
using System.Threading.Tasks;
using VotingApp.Models;

namespace VotingApp.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VotingDbContext _context;
        private IRepository<User>? _users;
        private IRepository<Vote>? _votes;
        private bool _disposed = false;

        public UnitOfWork(VotingDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => 
            _users ??= new Repository<User>(_context);
            
        public IRepository<Vote> Votes => 
            _votes ??= new Repository<Vote>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
