﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Account)
                .WithOne()
                .HasForeignKey<User>(u => u.AccountId);

            modelBuilder.Entity<MessageContent>()
                .HasOne(m => m.Message)
                .WithOne()
                .HasForeignKey<MessageContent>(m => m.MessageId);

            modelBuilder.Entity<AccountRole>()
                .HasOne(ar => ar.Account)
                .WithMany(u => u.AccountRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<AccountRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.AccountRoles)
                .HasForeignKey(ur => ur.RoleId);
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin" },
                new Role { Id = 2, RoleName = "User" }
                );
        }
    }

    #region Account
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<AccountRole> AccountRoles { get; set; }
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
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<AccountRole> AccountRoles { get; set; }
    }

    public class AccountRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Account Account { get; set; }

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
        public DateTime SentTime { get; set; }
    }

    public class MessageContent
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public MType Type { get; set; }
        public string Content { get; set; }

        public Message Message { get; set; }
    }
    #endregion
}
