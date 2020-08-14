using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PartyMemberManager.Dal.Entities;

namespace PartyMemberManager.Dal
{
    public class PMContext : DbContext
    {
        public PMContext(DbContextOptions<PMContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ErasData;Integrated Security=True");
        }

        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<Operator> Operators { get; set; }
        public virtual DbSet<OperatorModule> OperatorModules { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<TrainClassType> TrainClassTypes { get; set; }
        public virtual DbSet<TrainClass> TrainClasses { get; set; }
        public virtual DbSet<TrainResult> TrainResults { get; set; }
        public virtual DbSet<CadreTrain> CadreTrains { get; set; }
        public virtual DbSet<PartyActivist> PartyActivists { get; set; }
        public virtual DbSet<PotentialMember> PotentialMembers { get; set; }
        public virtual DbSet<Nation> Nations { get; set; }
        public virtual DbSet<ActiveApplicationSurvey> ActiveApplicationSurveies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Operator>().HasKey(o => o.Id);
            modelBuilder.Entity<Operator>().HasIndex(o => o.LoginName).IsUnique(true);
            modelBuilder.Entity<Operator>()
                .HasOne(o => o.Department)
                .WithMany()
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Module>().HasKey(m => m.Id);
            modelBuilder.Entity<Module>().HasIndex(m => m.Name).IsUnique(true);
            modelBuilder.Entity<Module>()
                .HasOne(m => m.ParentModule)
                .WithMany(m => m.ChildModules)
                .HasForeignKey(m => m.ParentModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OperatorModule>().HasKey(m => m.Id);
            modelBuilder.Entity<OperatorModule>()
                .HasOne(om => om.User)
                .WithMany()
                .HasForeignKey(om => om.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OperatorModule>()
                .HasOne(om => om.Module)
                .WithMany()
                .HasForeignKey(om => om.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Log>().HasKey(l => l.Id);

            modelBuilder.Entity<Department>().HasKey(d => d.Id);

            modelBuilder.Entity<TrainClass>().HasKey(t => t.Id);
            modelBuilder.Entity<TrainClass>()
                .HasOne(t => t.TrainClassType)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActiveApplicationSurvey>()
                .HasOne(o => o.Department)
                .WithMany()
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
