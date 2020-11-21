
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
        public virtual DbSet<Nation> Nations { get; set; }
        public virtual DbSet<ActiveApplicationSurvey> ActiveApplicationSurveies { get; set; }
        public virtual DbSet<YearTerm> YearTerms { get; set; }
        public virtual DbSet<TrainClass> TrainClasses { get; set; }
        public virtual DbSet<PartyActivist> PartyActivists { get; set; }
        public virtual DbSet<ActivistTrainResult> ActivistTrainResults { get; set; }
        public virtual DbSet<PotentialMember> PotentialMembers { get; set; }
        public virtual DbSet<PotentialTrainResult> PotentialTrainResults { get; set; }

        public virtual DbSet<ProvinceTrainClass> ProvinceTrainClasses { get; set; }
        public virtual DbSet<ProvinceCadreTrain> ProvinceCadreTrains { get; set; }
        public virtual DbSet<SchoolCadreTrain> SchoolCadreTrains { get; set; }
        public virtual DbSet<Course> Courses { get; set; }

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

            modelBuilder.Entity<YearTerm>()
                .HasKey(y => y.Id);
            modelBuilder.Entity<YearTerm>().HasIndex(y => new { y.StartYear, y.Term }).IsUnique(true);

            modelBuilder.Entity<TrainClass>().HasKey(t => t.Id);
            modelBuilder.Entity<TrainClass>()
                .HasOne(t => t.TrainClassType)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainClass>()
                .HasOne(o => o.Department)
                .WithMany()
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainClass>()
                .HasOne(t => t.YearTerm)
                .WithMany()
                .HasForeignKey(t => t.YearTermId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActiveApplicationSurvey>()
                .HasOne(o => o.Department)
                .WithMany()
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PartyActivist>()
                .HasOne(o => o.Department)
                .WithMany()
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PartyActivist>()
                .HasOne(p => p.Nation)
                .WithMany()
                .HasForeignKey(p => p.NationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PartyActivist>()
                .HasOne(p => p.TrainClass)
                .WithMany()
                .HasForeignKey(p => p.TrainClassId);

            modelBuilder.Entity<ActivistTrainResult>()
                .HasOne(a => a.PartyActivist)
                .WithMany()
                .HasForeignKey(a => a.PartyActivistId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ActivistTrainResult>()
                .HasIndex(p => p.CertificateNumber).IsUnique(true);

            modelBuilder.Entity<PotentialMember>()
                .HasOne(p => p.TrainClass)
                .WithMany()
                .HasForeignKey(p => p.TrainClassId);
            modelBuilder.Entity<PotentialMember>()
                .HasOne(p => p.PartyActivist)
                .WithMany()
                .HasForeignKey(p => p.PartyActivistId);

            modelBuilder.Entity<PotentialTrainResult>()
                .HasOne(p => p.PotentialMember)
                .WithMany()
                .HasForeignKey(a => a.PotentialMemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PotentialTrainResult>()
                .HasIndex(p => p.CertificateNumber).IsUnique(true);

            modelBuilder.Entity<ProvinceCadreTrain>()
                .HasOne(p => p.ProvinceTrainClass)
                .WithMany()
                .HasForeignKey(a => a.ProvinceTrainClassId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasOne(p => p.Nation)
                .WithMany()
                .HasForeignKey(p => p.NationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
