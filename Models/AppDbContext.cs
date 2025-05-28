using Microsoft.EntityFrameworkCore;

namespace server.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Userr> Userr { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<File> File { get; set; }
        public DbSet<FileType> FileType { get; set; }
        public DbSet<ChatMember> ChatMember { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatType> ChatType { get; set; }
        public DbSet<ChatRole> ChatRole { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<ChatGroup> ChatGroup { get; set; }
        public DbSet<FAQ> FAQ { get; set; }
        public DbSet<GroupMember> GroupMember { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>()
                .HasOne(f => f.Messages)
                .WithMany(m => m.Files)
                .HasForeignKey(f => f.ID_Messages)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<File>()
                .HasOne(f => f.FileType)
                .WithMany(ft => ft.Files)
                .HasForeignKey(f => f.ID_FileType);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Userr)
                .WithMany()
                .HasForeignKey(c => c.ID_User_CreatedBy);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.ChatType)
                .WithMany(ct => ct.Chats)
                .HasForeignKey(c => c.ID_ChatType);

                modelBuilder.Entity<ChatMember>()
        .HasOne(cm => cm.Chat)
        .WithMany(c => c.ChatMembers)
        .HasForeignKey(cm => cm.ID_Chat);

    modelBuilder.Entity<ChatMember>()
        .HasOne(cm => cm.Userr)
        .WithMany(u => u.ChatMembers)
        .HasForeignKey(cm => cm.ID_Userr);

    modelBuilder.Entity<ChatMember>()
        .HasOne(cm => cm.ChatRole)
        .WithMany(cr => cr.ChatMembers)
        .HasForeignKey(cm => cm.ID_ChatRole);

    modelBuilder.Entity<Message>()
        .HasOne(m => m.Userr)
        .WithMany(u => u.Messages)
        .HasForeignKey(m => m.ID_User);

    modelBuilder.Entity<Message>()
        .HasOne(m => m.Chat)
        .WithMany(c => c.Messages)
        .HasForeignKey(m => m.ID_Chat);

    modelBuilder.Entity<ChatGroup>()
        .HasOne(cg => cg.Chat)
        .WithMany(c => c.ChatGroups)
        .HasForeignKey(cg => cg.ID_Chat);

    modelBuilder.Entity<ChatGroup>()
        .HasOne(cg => cg.Group)
        .WithMany(g => g.ChatGroups)
        .HasForeignKey(cg => cg.ID_Group);

            base.OnModelCreating(modelBuilder);
        }
    }
} 