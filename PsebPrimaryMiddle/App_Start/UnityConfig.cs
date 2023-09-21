using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using PsebJunior.Models;
using PsebPrimaryMiddle.Repository;
using PsebJunior.AbstractLayer;

namespace PsebPrimaryMiddle
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            //
            container.RegisterType<ISchoolRepository,SchoolDB>();
            container.RegisterType<IBankRepository, BankDB>();
            container.RegisterType<IChallanRepository, ChallanDB>();
            container.RegisterType<IReportRepository, ReportDB>();
            container.RegisterType<IAdminRepository, AdminDB>();
            container.RegisterType<ICorrectionPerformaRepository, CorrectionPerformaDB>();
            container.RegisterType<ICenterHeadRepository, CenterHeadDB>();
            container.RegisterType<IMigrateSchoolRepository, MigrateSchoolDB>();
            container.RegisterType<IAffiliationRepository, AffiliationDB>();
            //
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}