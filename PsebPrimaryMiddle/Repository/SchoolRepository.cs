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
    public interface ISchoolRepository
    {
      

        Task<LoginSession> CheckLogin(LoginModel LM);
        // SchoolMaster
        Task<SchoolDataBySchlModel> GetSchoolDataBySchl(string sid);
        DataSet SchoolMasterViewSP(int type, string SCHL, string search);
        SchoolModels GetSchoolDataBySchl(string SCHL, out DataSet result);

        //update school profile
        int UpdateUSIJunior(SchoolModels SM);
        //Change PAssword
        int SchoolChangePassword(string SCHL, string CurrentPassword, string NewPassword);       
                
        //searching
        DataSet SearchSchoolDetailsPaging(string search, int startIndex, int endIndex);
        
        // Undertaking
        int InsertUndertaking(string SCHL, string NoOfPrimary, string NoOfMiddle, out string OutError);

        // Admin Card       
        DataSet PrintAdmitCard(string Search, string schl, string cls);


        #region CCE Marks Entry Panel      

        DataSet GetCCEMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotCCEMarks(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);
        DataSet CCEMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError);
        #endregion CCE Entry Panel



        #region Marks Entry Panel For Primary Class 


        // SchoolAllowForMarksEntry SchoolAllowForMarksEntry(string SCHL, string cls);
        DataSet GetMarksEntryDataBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotMarksEntry(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);

        DataSet MarksEntryReport(string CenterId, int reporttype,string search, string schl, string cls, out string OutError);
        #endregion Marks Entry Panel For Primary Class 


        #region  School Cut List
        DataSet GetCentreSchl(string schl, string cls);
        DataSet CutList(string Search, string schl, string CLASS, string Type, string Status);
        #endregion  School Cut List


        #region   Practical Exams

        DataSet GetPracticalMarks_Schl(string SCHL, string cls);
        DataSet PracExamEnterMarks(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber);

        DataSet PracExamViewList(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub);
        DataSet ViewPracExamEnterMarks(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub);
        DataSet ViewPracExamFinalSubmit(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub);
        string AllotPracMarks(string RP, DataTable dtSub, string class1, out int OutStatus, out string OutError);
        string RemovePracMarks(string RP, DataTable dtSub, string class1, out int OutStatus, out string OutError);
        string PracExamFinalSubmit(string ExamCent, string class1, string RP, string cent, string sub, string schl, DataTable dtSub, out int OutStatus, out string OutError);
        #endregion   Practical Exams


        #region Signature Chart and Confidential List Primary Middle Both

        DataSet SignatureChart(int type, string cls, string SCHL, string cent);
        DataSet GetSignatureChart(SchoolModels sm);
        DataSet GetConfidentialList(SchoolModels sm);
        #endregion Signature Chart and Confidential List Primary Middle Both

        #region CapacityLetter
        DataSet CapacityLetter(string SCHL);
        #endregion CapacityLetter


        #region PhyChlMarksEntry Marks Entry Panel      

        DataSet GetPhyChlMarksEntryMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotPhyChlMarksEntryMarks(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);
        DataSet PhyChlMarksEntryMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError);
        #endregion PhyChlMarksEntry Entry Panel


        #region ElectiveTheory Marks Entry Panel      

        DataSet GetElectiveTheoryMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotElectiveTheoryMarks(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);
        DataSet ElectiveTheoryMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError);
        #endregion ElectiveTheory Entry Panel

        #region  School Result Declare 

        DataSet GetSchoolResultDetails(string Search, string schl, string class1, string rp);
        #endregion  School Result Declare 

        SchoolPremisesInformation SchoolPremisesInformationBySchl(string SCHL, out DataSet ds1);


        #region  Pre Board Exam Marks Entry Panel  

        DataSet GetPreBoardExamTheoryBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotPreBoardExamTheoryPM(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);
        DataSet PreBoardExamTheoryReportPM(string CenterId, int reporttype, string search, string schl, string cls, out string OutError);
        #endregion  Pre Board Exam Marks Entry Panel  


        #region SchoolBasedExams Marks Entry Panel      

        DataSet GetSchoolBasedExamsMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1);
        string AllotSchoolBasedExamsMarks(string submitby, string stdid, DataTable dtSub, string cls, out int OutStatus);
        DataSet SchoolBasedExamsMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError);
        #endregion  SchoolBasedExams Marks Entry Panel     

        #region Signature Chart and Confidential List Primary  Pankaj/Dheeraj
        DataSet SignatureChartPrimarySub(SchoolModels sm);
        DataSet SignatureChartPrimary(SchoolModels sm);
        DataSet ConfidentialListPrimary(SchoolModels sm);
        #endregion Signature Chart and Confidential List Primary   Pankaj/Dheeraj

        #region Signature Chart and Confidential List Middle   Pankaj/Dheeraj

        DataSet SignatureChartMiddleSub(SchoolModels sm);
        DataSet SignatureChartMiddle(SchoolModels sm);
        DataSet ConfidentialListMiddle(SchoolModels sm);
        DataSet ConfidentialListMiddleDetail(SchoolModels sm);
        #endregion Signature Chart and Confidential List Middle   Pankaj/Dheeraj


    }

}