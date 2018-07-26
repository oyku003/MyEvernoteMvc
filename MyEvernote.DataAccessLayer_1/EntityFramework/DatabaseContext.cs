using MyEvernote.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyEvernote.DataAccessLayer_1.EntityFramework
{
  public  class DatabaseContext :DbContext
    {
        public DbSet<EvernoteUser> EvernoteUsers { get; set; }

        public DbSet<Note> Notes { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Liked> Likes { get; set; }

        public DatabaseContext()
        {
            Database.SetInitializer(new MyInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)//Modellerin bazı işlemlerini bu metodu ezerek ayarlayabilirz.Örnegğin cascadeing baglantılar
        {
            //Fluent Api
            modelBuilder.Entity<Note>()
                .HasMany(n => n.Commnets)
                .WithRequired(c => c.Note)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Note>()
              .HasMany(n => n.Likes)
              .WithRequired(c => c.Note)
              .WillCascadeOnDelete(true);//Note birden cok like ile ilişkili.Likeda da note gerekli sonuncusu da cascade
        }

    }




}
