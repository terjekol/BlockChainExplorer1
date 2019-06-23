using Microsoft.EntityFrameworkCore;

namespace BlockChainExplorer1.Model
{
    public class BlockchainExplorerDbContext : DbContext
    {
        public BlockchainExplorerDbContext(DbContextOptions<BlockchainExplorerDbContext> options)
        : base(options)
        {
        }
        public DbSet<Search> Search { get; set; }
    }
}
