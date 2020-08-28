using Microsoft.EntityFrameworkCore;

namespace product_api_netcore.Models
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
