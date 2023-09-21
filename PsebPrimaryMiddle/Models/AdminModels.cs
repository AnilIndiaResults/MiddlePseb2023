using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web.Mvc;

namespace PsebJunior.Models
{

    public class AdminLoginSession
    {
        public string CurrentSession { get; set; }
        public int AdminId { get; set; }
        public string USER { get; set; }
        public string AdminType { get; set; }
        public string USERNAME { get; set; }
        public string PAccessRight { get; set; }
        public string Dist_Allow { get; set; }
        public string ActionRight { get; set; }   
        public int LoginStatus { get; set; }
      
    }
    public class CircularModels
    {
        public HttpPostedFileBase file { get; set; }
        public DataSet StoreAllData { get; set; }
        public int? Type { get; set; }
        public int? ID { get; set; }
        public string CircularNo { get; set; }
        public string Session { get; set; }
        public string Title { get; set; }
        public string Attachment { get; set; }
        public string UrlLink { get; set; }
        public string Category { get; set; }
        public string UploadDate { get; set; }
        public string ExpiryDate { get; set; }
        public int? IsMarque { get; set; }
        public int IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public List<CircularTypeMaster> CircularTypeMasterList { get; set; }
        public string SelectedCircularTypes { get; set; }

    }

    public class CircularTypeMaster
    {
        public int Id { get; set; }
        public string CircularType { get; set; }
        public bool IsSelected { get; set; }
    }


    public class InboxModel
    {
        public IList<SelectListItem> AdminList { get; set; }       
        public DataSet StoreAllData;
        public int id { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Attachments { get; set; }
        public HttpPostedFileBase[] files { get; set; }
        public FileDetail fileDetails { get; set; }
        public List<String> CommaStringToList { get; set; }

        public string Reply { get; set; }
        public HttpPostedFileBase[] Replyfiles { get; set; }
        public List<MailReplyMaster> MailReplyMasterToList { get; set; }
        public List<String> ReplyfilesToList { get; set; }
    }

    public class MailReplyMaster
    {
        public int Rid { get; set; }
        public int MId { get; set; }
        public string Reply { get; set; }
        public string ReplyFile { get; set; }
        public int ReplyBy { get; set; }
        public int ReplyTo { get; set; }
        public string ReplyOn { get; set; }
        public int IsReplyRead { get; set; }
        public string ReplyReadOn { get; set; }        
    }

    public class AdminUserModel 
    {
        
        public string Bank_Allow { get; set; }
        public string Set_Allow { get; set; }
        public IList<SelectListItem> MenuList { get; set; }
        public IList<SelectListItem> BranchList { get; set; }
        public IList<SelectListItem> SetList { get; set; }
        public IList<SelectListItem>AdminList { get; set; }
        public IList<SelectListItem> DistList { get; set; }
        public List<SiteMenu> SiteMenuModel { get; set; }
        public DataSet StoreAllData;
        public int id { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string PAccessRight { get; set; }
        public string Usertype { get; set; }
        public DateTime CreatedDt { get; set; }
        public string Dist_Allow { get; set; }    
        public string LastActivityDate { get; set; }
        public string User_fullnm { get; set; }
        public string Designation { get; set; }
        public string Branch { get; set; }
        public string Mobno { get; set; }
        public string Remarks { get; set; }
        public int STATUS { get; set; }
        public string EmailID { get; set; }
        public string SAccessRight { get; set; }
        public string ActionRight { get; set; }
        public int utype { get; set; }
        public IList<string> listOfActionRight { get; set; }
    }

   
    public class AdminModels
    {
        public IList<SelectListItem> ErrorList { get; set; }

        public string CertNo { get; set; }
        public string CertDate { get; set; }
        public string Remarks { get; set; }

        public string SelYear { get; set; }
        public string SelExamDist { get; set; }
        public string ROLLexam { get; set; }
        public string ERRcode { get; set; }
        public HttpPostedFileBase file { get; set; }

        public string CorrectionFromDate { get; set; }
        public string CorrectionToDate { get; set; }
        public string CorrectionRecieptNo { get; set; }
        public string CorrectionRecieptDate { get; set; }
        public string CorrectionNoCapproved { get; set; }
        public string CorrectionAmount { get; set; }

        public string CorrectionUserId { get; set; }
        public string CorrectionOldPwd { get; set; }
        public string CorrectionNewPwd { get; set; }

        public string CorrectionType { get; set; }
        public string CorrectionLot { get; set; }
        public string REGNO { get; set; }
        public string TotalSearchString { get; set; }
        public DataSet StoreAllData;
        public int? ID { get; set; }
        public string SchlCode { get; set; }
        public string Candi_Name { get; set; }
        public string Father_Name { get; set; }
        public string Mother_Name { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string TotalMarks { get; set; }
        public string ObtainedMarks { get; set; }
        public string Result { get; set; }
        public string totMark { get; set; }
        public string reclock { get; set; }
        public string SearchResult { get; set; }
        public string FormName { get; set; }
        public string SdtID { get; set; }
        public string EXAM { get; set; }
        #region Challan Master

        public string ChallanID { get; set; }
        public string SchlReffAppRll { get; set; }
        public string Challan_Date { get; set; }
        public string Challan_V_Date { get; set; }
        public string FeeType { get; set; }
        public string BankName { get; set; }
        public string Fee { get; set; }
        public string Journal_No { get; set; }
        public string Branch { get; set; }
        public string Challan_Depst_Date { get; set; }
        public string Challan_Dwld_Stats { get; set; }
        public string Challan_Verify_Stats { get; set; }
        #endregion Challan Master
    }

    public class FeeModels
    {
        public HttpPostedFileBase file { get; set; }
        public DataSet StoreAllData { get; set; }
        public string AllowBanks { get; set; }
        public string RP { get; set; }
        public int? ID { get; set; }       
        public string Type { get; set; }
        public string FORM { get; set; }
        public string FeeCat { get; set; }
        public int? Class { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string BankLastDate { get; set; }
        public int? Fee { get; set; }
        public int? LateFee { get; set; }
        public int? FeeCode { get; set; }
        public int IsActive { get; set; }
    }


    public class FinalResultModels
    {
        public DataSet StoreAllData { get; set; }
        public string ROLL { get; set; }
        public string CENT { get; set; }
        public string REGNO { get; set; }    
        public int? ID { get; set; }
        public string Schl { get; set; }
        public string SchoolName { get; set; }
        public string Candi_Name { get; set; }
        public string Father_Name { get; set; }
        public string Mother_Name { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string TotalMarks { get; set; }
        public string ObtainedMarks { get; set; }
        public string Result { get; set; }
        public string Category { get; set; }
        public string reclock { get; set; }
        public string SearchResult { get; set; }
        public string FormName { get; set; }
        public string StdId { get; set; }
        public string EXAM { get; set; }
        public string SET { get; set; }
        public string PHONE { get; set; }
        public string MOBILE { get; set; }
        public string DIST { get; set; }
        public string DISTNM { get; set; }
    }
 
}