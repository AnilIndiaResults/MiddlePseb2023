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
    public interface ICorrectionPerformaRepository
    {

        DataSet GetCorrPunjabiName(string id);
        DataSet GetStudentRecordsCorrectionData(int type, string schlid,string Corrections);
        DataSet getCorrrectionField(string std_Class);
        DataSet GetCorrectionStudentRecordsSearchJunior(string search, int formName, int pageIndex);
        DataSet InsertSchoolCorrectionAddJunior(RegistrationModels RM);
        string AiddedCorrectionRecordDelete(string CorrectionId);
        string FinalSubmitCorrectionJunior(RegistrationModels RM);
        DataSet GetCorrectiondataFinalPrintList(RegistrationModels RM);
        DataSet SchoolCorrectionPerformaFinalPrintSession(RegistrationModels RM);
        DataSet SchoolCorrectionStatus(string schlid, string search);
        string CorrLotAcceptReject(string correctionType, string correctionLot, string acceptid, string rejectid, string removeid, string adminid, out string OutStatus);
        string CorrLotRejectRemarksSP(string corid, string remarks, string adminid);
        DataSet SchoolCorrectionPerformaRoughReport(RegistrationModels RM);


        DataSet GetCorrectionDataFirm(string search, string CrType, int pageIndex);
        DataSet GetCorrectionDataFirmUpdated(string search, string CrType, int pageIndex);
        DataSet CheckFeeAllCorrectionDataByFirmSP(int ActionType, string UserName, string FirmCorrectionLot);
        DataSet CorrectionDataFirmFinalSubmitSPRN(string userNM, out string FirmCorrectionLot, out string OutError);
        DataSet CorrectionDataFirmFinalSubmitSPRNJuniorMiddleSubjectOnly(string userNM, out string FirmCorrectionLot, out string OutError);


        DataSet AllCancelFirmSchoolCorrection(string FirmUser, string CorType, string search);
        DataSet SetCorrectionDataFirmFeeDetails(AdminModels am, string userNM);
        DataSet CorrectionDataFirmFinalSubmitSPRNByCorrectionLot(string CorrectionLot, string userNM, out string FirmCorrectionLot, out string OutError);

        #region Subject
        string DeleteSubDataByCorrectionId(string CorrectionId, out string OutError);
        DataSet PendingCorrectionSubjects(string Schl, string cls);
        DataSet SearchStudentGetByData_SubjectCORR(string sid, string frmname);
        DataSet ViewAllCorrectionSubjects(string Schl);
        DataSet SearchCorrectionStudentDetails(string cls, string schl, string sid);
        DataSet NewCorrectionSubjects(string sid, string frmname, string scode);
        DataSet SearchOldStudent_Subject(string sid, string frmname, string scode);

        string FinalSubmitSubjectCorrection(RegistrationModels RM, string Class);
        string Primary_Subject_Correction(RegistrationModels RM,  string sid, DataTable dtFifthSubject);
        string Middle_Subject_Correction(RegistrationModels RM,  string sid, DataTable dtEighthSubject);






        #endregion Subject





    }
}