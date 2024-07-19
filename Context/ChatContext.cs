using Microsoft.EntityFrameworkCore;
using TunaChatChit.Resources;

namespace TunaChatChit.Context
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Account)
                .WithOne()
                .HasForeignKey<User>(u => u.AccountId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.MessageContent)
                .WithOne()
                .HasForeignKey<Message>(m => m.Id);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        }
    }

    #region Account
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Province { get; set; }

        public Account Account { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
    #endregion

    #region Message
    public class Message
    {
        public int Id { get; set; }
        public int SendId { get; set; }
        public int ReceiveId { get; set; }
        public MType Type { get; set; }
        public DateTime SentTime { get; set; }

        public MessageContent MessageContent { get; set; }
    }

    public class MessageContent
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
    #endregion
}
