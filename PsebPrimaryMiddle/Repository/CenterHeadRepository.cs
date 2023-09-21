using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;


namespace PsebPrimaryMiddle.Repository
{   

    public interface ICenterHeadRepository
    {
        int ChangePassword(string UserId, string CurrentPassword, string NewPassword);
        Task<CenterHeadLoginSession> CheckCenterHeadLogin(LoginModel LM);
        DataSet CenterHeadMaster(int type, int id, string Search);
        List<CenterSchoolModel> SchoolListByCenterId(int type, int id, string Search);

       // CenterHeadMasterModel GetCenterHeadMasterByCenterHeadId(int type, int id);
        DataSet UpdateCenterHeadMaster(CenterHeadMasterModel centerHeadMasterModel, out int OutStatus);

        //CenterHeadMasterModel GetCenterHeadMasterByCenterHeadId(int type, int id);
        //DataSet SubjectWiseReports(string ReportType, int cls, string dist);
        //DataSet RegistrationReportSearch(string Search, string Session);


        #region GenerateTicket

        int InsertGenerateTicket(GenerateTicketModel generateTicketModel, out string outTicketNo);
        string ListingGenerateTicket(int type, int TicketId, out int OutStatus);
        List<GenerateTicketModel> GetGenerateTicketData(int type, string centerUser, int TicketId, string Search);
        #endregion
    }


}