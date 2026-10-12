using Microsoft.EntityFrameworkCore;
using Rag.Api.Models;

namespace Rag.Api.Data
{
    public class RagDbContext : DbContext
    {
        public RagDbContext(
       DbContextOptions<RagDbContext> options)
       : base(options)
        {
        }

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentChunk> DocumentChunks =>
        Set<DocumentChunk>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ChunkText)
                    .IsRequired();

                entity.HasOne(x => x.Document)
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.DocumentId,
                    x.PageNumber,
                    x.ChunkIndex
                });
            });
        }
    }
}
