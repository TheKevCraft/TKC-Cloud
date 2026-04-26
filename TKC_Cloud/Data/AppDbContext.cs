using Microsoft.EntityFrameworkCore;

namespace TKC_Cloud.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // File/Folder-System
    public DbSet<FileEntry> Files => Set<FileEntry>();
    public DbSet<Folder> Folders => Set<Folder>();

    // Users
    public DbSet<User> Users => Set<User>();

    // Tokens
    public DbSet<RegisterToken> RegisterTokens => Set<RegisterToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    // Upload-System
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();
    public DbSet<UploadedChunk> uploadedChunks => Set<UploadedChunk>();

    // Share-System
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<SharePermission> SharePermissions => Set<SharePermission>();


    // Advanced Configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Share>()
            .HasIndex(s => s.Token)
            .IsUnique();

        // Folder
        modelBuilder.Entity<Folder>()
            .HasMany(f => f.SubFolders)
            .WithOne(f => f.Parent)
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Upload
        modelBuilder.Entity<UploadedChunk>()
            .HasIndex(c => new { c.UploadSessionId, c.ChunkIndex })
            .IsUnique();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}