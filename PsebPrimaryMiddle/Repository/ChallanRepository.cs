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
    public interface IChallanRepository
    {

        DataSet CancelOfflineChallanBySchl(string cancelremarks, string challanid, out string outstatus, string Schl, string Type);
        DataSet ShiftChallanDetails(ShiftChallanDetails scd, out string OutError, out int OutSID);
        DataSet ViewShiftChallanDetails(int Sid, int Type, string search, int pageNumber, int PageSize);


        //
        #region Calculate Fee of Registration 
        DataSet GetFinalPrintChallan(string SchoolCode);
        DataSet GetCalculateFeeBySchool(string cls, string search, string schl, DateTime? date = null);
        DataSet GetCalculateFeeBySchoolWithoutLateFee(string cls, string search, string schl, DateTime? date = null);
        DataSet GetSchoolLotDetails(string schl);
        DataSet GetChallanDetailsById(string ChallanId);
        DataSet GetMissingCheckFeeStatusSPJunior(string Search);
        DataSet CheckFeeStatus(string SCHL, string type, string id, DateTime date);
        DataSet ReGenerateChallaanByIdJunior(string ChallanId, string BCODE, string usertype, out int OutStatus, out string CHALLANIDOut);
        string InsertPaymentFormJunior(ChallanMasterModel CM, out string SchoolMobile);
        string InsertPaymentForm(ChallanMasterModel CM,  out string SchoolMobile);
        #endregion Calculate Fee of Registration 
        DataSet GetChallanDetailsBySearch(string search);

        #region Challan Deposit Details
        DataSet GetChallanDetailsByStudentList(string studentlist);
        DataSet ChallanDepositDetails(ChallanDepositDetailsModel cdm, out string OutError);
        string ChallanDepositDetailsCancel(string cancelremarks, string challanid, out string outstatus, int AdminId);
        #endregion Challan Deposit Details





    }
}