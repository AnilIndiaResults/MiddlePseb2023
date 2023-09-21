using PSEBONLINE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace PsebJunior.Models
{
    public class DBContext : DbContext
    {
        public DBContext() : base("name=myDBConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            Database.SetInitializer<DBContext>(null);
            base.OnModelCreating(modelBuilder);
        }

        //Online
        public DbSet<ExamCentreConfidentialResources> ExamCentreConfidentialResources { get; set; }
        public DbSet<ExamCentreResources> ExamCentreResources { get; set; }
        //Online
        public DbSet<SessionSettingMasters> SessionSettingMasters { get; set; }

        public DbSet<OnDemandCertificates> OnDemandCertificates { get; set; }
        public DbSet<OnDemandCertificatesViews> OnDemandCertificatesViews { get; set; }
        public DbSet<OnDemandCertificate_ChallanDetailsViews> OnDemandCertificate_ChallanDetailsViews { get; set; }
        /// <summary>

        public DbSet<ReExamTermStudents> ReExamTermStudents { get; set; }

        //OnlineCentreCreation
        //public DbSet<CenterMasterForOnlineCentreCreationViews> CenterMasterForOnlineCentreCreationViews { get; set; }
        public DbSet<OnlineCentreCreations> OnlineCentreCreations { get; set; }
        public DbSet<OnlineCentreCreationsViews> OnlineCentreCreationsViews { get; set; }

        /// <summary>
        /// //////////
        /// </summary>

        public DbSet<UndertakingOfQuestionPapers> UndertakingOfQuestionPapers { get; set; }

        public DbSet<tblOtherBoardDocumentsBySchool> tblOtherBoardDocumentsBySchool { get; set; }

        //StudentSchoolMigrations
        public DbSet<StudentSchoolMigrations> StudentSchoolMigrations { get; set; }


        public DbSet<Tblifsccodes> Tblifsccodes { get; set; }
        public DbSet<InfrasturePerformas> InfrasturePerformas { get; set; }
        public DbSet<InfrasturePerformasList> InfrasturePerformasList { get; set; }
        public DbSet<tblSchUsers> tblSchUsers { get; set; }
        public DbSet<ChallanModels> ChallanModels { get; set; }
        public DbSet<InfrasturePerformasListWithSchool> InfrasturePerformasListWithSchool { get; set; }


        //// ExceptionHistoryMasters
        //public DbSet<ExceptionHistoryMasters> ExceptionHistoryMasters { get; set; }

        //// AtomSettlementTransactions
        //public DbSet<AtomSettlementTransactions> AtomSettlementTransactions { get; set; }

        ////  public DbSet<EAffiliationDashBoardViewModel> eAffiliationDashBoardViewModels { get; set; }



        //// Registration
        //public DbSet<RegistrationClassFormWiseDocumentMasters> RegistrationClassFormWiseDocumentMasters { get; set; }


        ////SelfDeclarations
        //public DbSet<SelfDeclarations> SelfDeclarations { get; set; }



    }
}