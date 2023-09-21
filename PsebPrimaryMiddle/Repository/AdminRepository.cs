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
using System.Web.Mvc;

namespace PsebPrimaryMiddle.Repository
{
    public interface IAdminRepository
    {
         int ChangePassword(int UserId, string CurrentPassword, string NewPassword);
         AdminLoginSession CheckAdminLogin(LoginModel LM);
         List<SelectListItem> getAdminDistAllowList(string usertype, string adminid);

        #region  Inbox Master
         DataTable MailReplyMaster(InboxModel im, out string OutError);
         DataTable AddInboxMaster(InboxModel im, out string OutError);
         DataSet ViewInboxMaster(int MailId, int Type, int adminid, string search, int pageNumber, int PageSize);
         string ReadInbox(int Id, int AdminId, int type1, out string OutError);
        #endregion  Inbox Master


        #region Menu Master

         DataSet GetAllMenu(int id);
         DataSet UpdateMenuJunior(SiteMenu model, int ParentMenuId, out int OutStatus, string AssignYear);
         DataSet ListingMenuJunior(int type, int menuid, out int OutStatus);
         string CreateMenuJunior(SiteMenu model, int IsParent, int ParentMenuId, int IsMenu, string AssignYear);

        #endregion Menu Master


        #region Admin User Master     

         string ListingUser(int type, int menuid, out int OutStatus);
         string CreateAdminUser(AdminUserModel model, out int OutStatus);
         string AssignMenuToUser(string adminid, string adminlist, string pagelist, out string OutError);
         DataSet GetAllAdminUser(int id, string search);
         DataSet GetAdminIdWithMenuId(int MenuId);
         string DeleteAdminUser(string id);

        #endregion Admin User Master      


        #region Circular
         string ListingCircular(int type, int id, out int OutStatus);
         DataSet CircularTypeMaster();
         DataSet CircularMaster(string search, int pageIndex);
         int InsertCircularMaster(CircularModels FM, out string outCircularNo);

        #endregion Circular

        #region Firm Exam Data Download
         DataSet FirmExamDataDownload(int Type, string RegOpen, string FirmUser, string Search, out string ErrStatus);
         DataSet GetDataByIdandTypeRN(string id, string type, string roll);
         string CheckFirmExamDataDownloadMis(DataSet ds, out DataTable dt, string RP);
        #endregion Firm Exam Data Download

        #region Allot Regno

         DataSet SelectForMigrateSchools(string Search);
         DataSet GetAllFormNameBySchl(string SCHL);
         DataSet GetStudentRegNoNotAlloted(string search, string schl, int pageNumber);
         string ErrorAllotRegno(string stdid, string storeid, int Action, int userid, string remarks);
         DataTable ManualAllotRegno(string stdid, string regno, out int OutStatus, int userid, string remarks);

         DataSet ViewAllotRegNo(string search, string schl);
         string RemoveRegno(string storeid, int Action, int userid);
        #endregion Allot Regno

        #region Fee Entry
        DataSet GetFeeCodeMaster(int Type, int FeeCode);
        DataSet GetAllFeeMaster2016(string search);
        DataTable BulkFeeMaster(DataTable dt1, int adminid, out int OutStatus);
        int Insert_FeeMaster(FeeModels FM);
        #endregion Fee Entry

        #region  ExamErrorListMIS
        string CheckExamMisExcel(DataSet ds, out DataTable dt);
        DataSet GetErrorListMISByFirmId(int type, int AdminId, out string OutResult1);
        DataSet ExamErrorListMIS(DataTable dt1, int adminid, out int OutStatus);
        #endregion  ExamErrorListMIS

        #region  StudentRollNoMIS
        string CheckStdRollNoMisOnly(DataSet ds, out DataTable dt);
        string CheckStdRollNoMis(DataSet ds, out DataTable dt);
        string CheckStdRollNoRangeMis(DataSet ds, out DataTable dt);
        DataSet StudentRollNoMISONLY(DataTable dt1, int adminid, out int OutStatus);
        DataSet StudentRollNoMIS(DataTable dt1, int adminid, out int OutStatus);
        DataSet StudentRollNoRangeMIS(DataTable dt1, int adminid, out int OutStatus);
        #endregion  StudentRollNoMIS

        #region  ViewCandidateExamErrorList
        DataSet ViewCandidateExamErrorList(int Type, string Search, string FirmNM, string CrType, int pageIndex);
        DataSet RemoveCandidateExamError(string username, string candid, string errcode);
        #endregion  ViewCandidateExamErrorList


        #region  AllowCCE
        DataSet UnlockCCE(string Schl, string Type, int AdminId, out int OutStatus);
        DataSet ListingSchoolAllowForMarksEntry(int Type, int id, string search, out int OutStatus);
        int InsertSchoolAllowForMarksEntry(int type, SchoolAllowForMarksEntry FM, out string SchlMobile);
        #endregion  AllowCCE

        #region Practical Submission Unlocked
        DataSet CheckPracFinalSubmission(string cls, string rp, string pcent, string sub, string fplot);
        string CheckPracticalSubmissionUnlocked(DataSet ds, out DataTable dt);
        DataSet PracticalSubmissionUnlocked(string cls, string rp, string cent, string sub, string fplot, int adminid, out string OutError);
        #endregion Practical Submission Unlocked

        #region PracticalCentUpdateMaster

        string CheckStdPracticalCentUpdateMaster(DataSet ds, out DataTable dt);
        string CheckPrivatePracticalCentUpdateMaster(DataSet ds, out DataTable dt);
        string CheckRegOpenPracticalCentUpdateMaster(DataSet ds, string cls, out DataTable dt);
        DataSet GetPrivateSubjectByRefnoSub(string refno, string sub);
        DataSet CheckPracticalCenter(string cent, string cls);
        DataSet PracticalCentUpdateMaster(DataTable dt1, int adminid, string examtype, string cls, out string OutError);
        DataSet PracCentSTD(DataTable dt1, int adminid, string examtype, string cls, out string OutError);

        #endregion PracticalCentUpdateMaster

        #region UnlockClusterTheoryMarks
        DataSet UnlockClusterTheoryMarks(DataTable dt1, int adminid, out string OutError);
        string CheckUnlockClusterTheoryExcel(DataSet ds, out DataTable dt);
        #endregion


        #region  Admin Result Update MIS
        string CheckResultMisExcel(DataTable dt, string userNM, out DataTable dt1);
        DataSet AdminResultUpdateMIS(DataTable dt1, int adminid, out string OutError);
        #endregion  Admin Result Update MIS
    }



}