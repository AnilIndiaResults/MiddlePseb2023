using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Data;

namespace PSEBONLINE.Models
{
    public class OnDemandCertificatesViews
    {
        [Key]
        public long DemandId { get; set; }
        public long Std_id { get; set; }
        public string Schl { get; set; }
        public int Cls { get; set; }
        public string Challanid { get; set; }
        public DateTime? Challandt { get; set; }
        public bool ChallanVerify { get; set; }
        public int Fee { get; set; }
        public int LateFee { get; set; }
        public int TotalFee { get; set; }
        public int IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int IsActive { get; set; }
        public DateTime? SubmitOn { get; set; }
        //
              
        public string Roll { set; get; }
        public string AdmDate { set; get; }
        public string roll { set; get; }
        public string regno { set; get; }
        public string form { set; get; }
        public string Aadhar { set; get; }
        public string Dist { set; get; }
        public string Rp { set; get; }
        public string EXAM { set; get; }
        public string NSQF { set; get; }
        public string Category { set; get; }
        public string name { set; get; }
        public string pname { set; get; }
        public string fname { set; get; }
        public string pfname { set; get; }
        public string mname { set; get; }
        public string pmname { set; get; }
        public string Caste { set; get; }
        public string Gender { set; get; }
        public string DOB { set; get; }
        public string phy_chal { set; get; }
    }

    public class OnDemandCertificates
    {
        [Key]
        public long DemandId { get; set; }
        public long Std_id { get; set; }
        public string Schl { get; set; }
        public int Cls { get; set; }
        public string Challanid { get; set; }
        public DateTime? Challandt { get; set; }
        public bool ChallanVerify { get; set; }
        public int Fee { get; set; }
        public int LateFee { get; set; }
        public int TotalFee { get; set; }
        public int IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int IsActive { get; set; }
        public DateTime? SubmitOn { get; set; }
        public int IsChallanCancel { get; set; }
    }
    public class OnDemandCertificateModelList
    {
        public DataSet StoreAllData { get; set; }
        public List<OnDemandCertificateSearchModel> OnDemandCertificateSearchModel { set; get; }
    }

    public class OnDemandCertificateSearchModel
    {
        public string OnDemandCertificatesStatus { set; get; }
        public string IsHardCopyCertificate { set; get; }
        public long Std_id { set; get; }
        public string Roll { set; get; }
        public string SCHL { set; get; }
        public string AdmDate { set; get; }
        public string Class { set; get; }
        public string regno { set; get; }
        public string form { set; get; }
        public string Aadhar { set; get; }
        public string Dist { set; get; }
        public string Rp { set; get; }
        public string EXAM { set; get; }
        public string NSQF { set; get; }
        public string Category { set; get; }
        public string name { set; get; }
        public string pname { set; get; }
        public string fname { set; get; }
        public string pfname { set; get; }
        public string mname { set; get; }
        public string pmname { set; get; }
        public string Caste { set; get; }
        public string Gender { set; get; }
        public string DOB { set; get; }
        public string phy_chal { set; get; }
        public int IsExistsInOnDemandCertificates { set; get; }
        public long DemandId { set; get; }
        public int IsChallanCancel { set; get; }
    }


    public class OnDemandCertificate_ChallanDetailsViewsModelList
    {
        public DataSet StoreAllData { get; set; }
        public List<OnDemandCertificate_ChallanDetailsViews> OnDemandCertificate_ChallanDetailsViews { set; get; }
    }
    public class OnDemandCertificate_ChallanDetailsViews
    {
        [Key]
        public string ChallanId { set; get; }
        public int NOC { set; get; }
        public string Class { set; get; }
        public string RP { set; get; }
        public string feecode { set; get; }
        public int IsCancel { set; get; }
        public int LOT { set; get; }
        public string Bank { set; get; }
        public string BCODE { set; get; }
    
        public string CHLNDATE { set; get; }
        public string CHLNVDATE { set; get; }
        public string ChallanVerifiedOn { set; get; }
        public string DEPOSITDT { set; get; }
        public string SCHLREGID { set; get; }
        public float FEE { set; get; }
        public float TOTFEE { set; get; }
        public float LateFee { set; get; }
        public float TotalFee { set; get; }
        public string BRANCH { set; get; }
        public string Status { set; get; }
        public string StatusNumber { set; get; }
        public float verified { set; get; }
        public float downldflg { set; get; }
        public string ExpireVDate { set; get; }
        public string StudentList { set; get; }
        public string FeeDepositStatus { set; get; }
        public int FinalPrintStatus { set; get; }
        public string FinalPrintFilePath { set; get; }
    }

}