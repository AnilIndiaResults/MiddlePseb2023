using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebPrimaryMiddle.Models;
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
    public interface IAffiliationRepository
    {
        AffiliationModel AffiliationContinuationBySchl(string schl, int type, out DataSet ds1);
        int AffiliationContinuation(AffiliationModel am, out string OutError);
        AffiliationModel GetAffiliationContinuationBySchlAndType(string schl, int type, out DataSet ds1);
        AffiliationFee AffiliationFee(int Cat, string schl, DateTime dt1);
        int UpdateAlreadyPaidAffiliationFee(AffiliationModel am);
        string IsValidForChallan(string schl);
        List<AffiliationDocumentMaster> AffiliationDocumentMasterList(DataTable dataTable);
        DataSet GetAffiliationDocumentDetails(int type, int eDocId, string schl, string search);
        int InsertAffiliationDocumentDetails(AffiliationDocumentDetailsModel model, int action, out string OutError);
        DataSet AffiliationContinuationReport();
        DataSet ViewAffiliationContinuation(int type1, string search, string schl, int pageIndex, out int OutStatus, int adminid);
    }
}