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

    public interface IBankRepository
    {
        DataSet GetBankDataByBCODE(BankModels BM, out int OutStatus);
        BankLoginSession BankLogin(LoginModel LM);

        DataSet GetErrorDetails();
        DataSet UpdateBankDataByBCODE(BankModels BM, out int OutStatus);

        DataSet GetAllFeeDepositByBCODE(BankModels BM, string search, out string OutError);
        DataSet DownloadChallan(BankModels BM, out int OutStatus);

        DataSet BankMisDetails(BankModels BM, int Type1, out int OutStatus);
        DataSet GetTotBankMIS(BankModels BM);

        DataSet ChangePasswordBank(BankModels BM, out int OutStatus);
        DataSet BankMisDetailsSearch(string search);

        DataTable BulkChallanBank(DataTable dt1, int adminid, int UPLOADLOT, BankModels BM, out int OutStatus, out string OutError);  
        DataTable BulkChallanBankPSEBHOD(DataTable dt1, int adminid, int UPLOADLOT, BankModels BM, out int OutStatus);

        DataSet GetChallanDetailsByIdSPBank(string ChallanId);
        DataSet GetChallanDetails(string search, string sesssion, int pageIndex);

        DataTable ImportBankMis(BankModels BM, int UPLOADLOT, out int OutStatus, out string Mobile);
        DataTable ImportBankMisSPJuniorPSEB(BankModels BM, int UPLOADLOT, out int OutStatus, out string Mobile);

        string CheckMisExcelExportRN(DataSet ds, out DataTable dt);
        string CheckMisExcelExportPSEB(DataSet ds, out DataTable dt);
    }  
}