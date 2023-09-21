using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Data;

namespace PsebJunior.Models
{

    public class FinalPrintStatus
    {
        public int Id { set; get; }
        public string ReportName { set; get; }
        public string Schl { set; get; }
        public string Lot { set; get; }
        public string EmailTo { set; get; }
        public string SentDate { set; get; }
        public string SentStatus { set; get; }
        public string FilePath { set; get; }        
        public bool IsActive { set; get; }
        public string CreatedBy { set; get; }

      
    }

    public class RegistrationAllStudentAdminModelList
    {
        public DataSet StoreAllData { get; set; }
        public List<RegistrationAllStudentAdminModel> RegistrationAllStudentAdminModel { set; get; }
    }

    public class RegistrationAllStudentAdminModel
    {
        public int Std_id { set; get; }
        public string form_Name { set; get; }
        public string schl { set; get; }
        public string Admission_Date { set; get; }
        public string Candi_Name { set; get; }
        public string Father_Name { set; get; }
        public string Mother_Name { set; get; }
        public string DOB { set; get; }
        public int LOT { set; get; }
        public string aadhar_num { set; get; }
        public DateTime? CreatedDate { set; get; }
        public DateTime? UPDT { set; get; }

        public string RegNo { set; get; }
        public string AadharNo { set; get; }
        public int? Fee { set; get; }
        public string ChallanStatus { set; get; }
        public string Remarks { set; get; }
        public bool? IsCancel { set; get; }
        public DateTime? CancelDT { set; get; }
    }

    public class RegistrationSearchModelList
    {
        public DataSet StoreAllData { get; set; }
        public List<RegistrationSearchModel> RegistrationSearchModel { set; get; }
    }

    public class RegistrationSearchModel
    {       
        public int Std_id { set; get; }
        public string form_Name { set; get; }
        public string schl { set; get; }
        public string Admission_Date { set; get; }

        public string Category { set; get; }
        public string Candi_Name { set; get; }
        public string Father_Name { set; get; }
        public string Mother_Name { set; get; }
        public string DOB { set; get; }
        public int LOT { set; get; }
        public string aadhar_num { set; get; }
        public DateTime? CreatedDate { set; get; }
        public DateTime? UPDT { set; get; }
        public string SubjectList { set; get; }
        //
        public string REGNO { set; get; }
        public string ProofCertificate { set; get; }
        public string ProofNRICandidates { set; get; }
        public string StudentUniqueId { set; get; }
        public string schlDist { set; get; }
    }

    public class LastEntryCandidate
    {
        public string Board_Roll_Num { set; get; }
        public string Admission_Num { set; get; }
        public string Candi_Name { set; get; }
        public string Father_Name { set; get; }
        public string Id { set; get; }
        public string Lot { set; get; }
    }
    public class CorrectionPerformaModel
    {
       // public List<SelectListItem> CorrectionTypeList { set; get; }
        public List<SelectListItem> CorrectionClassList { set; get; }
      
        //-----------------------Start Correction Performa Begin--------
        public string Grouplist { get; set; }
    public string CorrectionSet { get; set; }
    public string CorrectionBranch { get; set; }
    public string CorrectionBarCode { get; set; }
    public string CorrectionId { get; set; }
    public string CorrectionInsertDt { get; set; }
    public string CorrectionFinalSubmitDt { get; set; }
    public string CorrectionVerifyDt { get; set; }
    public string Class { get; set; }
    public string CorrectionLot { get; set; }
    public string Correctiontype { get; set; }
    public string Remark { get; set; }
    public string SelListField { get; set; }
    public string oldVal { get; set; }
    public string newVal { get; set; }
    public string newValP { get; set; }
    public string newValDOB { get; set; }
    public string schlCorNameE { get; set; }
    public string schlCorNameP { get; set; }
    public DataSet Correctiondata { get; set; }
    public DataSet Correctiondatafinal { get; set; }

        //-----------------------Start Correction Suject Begin--------
        //
        public string schlCorConDetails { set; get; }
        public string OpenRegularType { get; set; }

        //-----------------------END Correction Performa END--------
    }

    public class RegistrationModels
    {
        public Byte[] QRCode { get; set; }
        public string requestID { get; set; }
        public bool IsLateAdm { get; set; }
        public string NADID { set; get; }
        public HttpPostedFileBase fileM { get; set; }
        public string MotherTongue { get; set; }
        public string IsHardCopyCertificate { get; set; }

        public string EligibilityCriteria { get; set; }
        public string EmpUserId { get; set; }
        public string NSQFPattern { get; set; }
        public string IsSmartPhone { get; set; }
        public bool IsNRICandidate { get; set; }
        //
        public HttpPostedFileBase DocProofCertificate { get; set; }
        public HttpPostedFileBase DocProofNRICandidates { get; set; }

        public string ProofCertificate { get; set; }
        public string ProofNRICandidates { get; set; }
        public string UserId { set; get; }
        public string UserType { set; get; }
        public CorrectionPerformaModel CorrectionPerformaModel { set; get; }
        public LastEntryCandidate LastEntryCandidate { set; get; }
        public ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel { set; get; }
        public ClassFifthInitilizeListModel ClassFifthInitilizeListModel { set; get; }        
       
        public string DP { set; get; } //Disability Percentage
        public string CandId { set; get; }
        public string ExamRoll { set; get; }
  
       // public string Correctiontype { get; set; }
        //public string CorrectionLot { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }      
        
        public string Grouplist { get; set; }
        public string Class { get; set; }

        public string CandiMedium { get; set; }
        public string MetricBoard { get; set; }
        public string checkCategory { get; set; }
        public int IsImported { get; set; }
        public DataSet StoreAllData { get; set; }
        public bool Permanent { get; set; }
        public bool Provisional { get; set; }
        public string Metric_Roll_Num { get; set; }
        public string MetricMonth { get; set; }
        public string MetricYear { get; set; }
        public string scribeWriter { get; set; }
        public string DA { get; set; }
       


        //-------------Subject Details-----------
        public int sub_id { get; set; }
        public string sub_code { get; set; }
        public string subS1 { get; set; }
        public string subm1 { get; set; }
        public string subS2 { get; set; }
        public string subM2 { get; set; }
        public string subS3 { get; set; }
        public string subm3 { get; set; }
        public string subS4 { get; set; }
        public string subM4 { get; set; }
        public string subS5 { get; set; }
        public string subM5 { get; set; }
        public string subS6 { get; set; }
        public string subM6 { get; set; }
        public string subS7 { get; set; }
        public string subM7 { get; set; }
        public string subS8 { get; set; }
        public string subM8 { get; set; }

        public string subS9 { get; set; }
        public string subM9 { get; set; }
        public string s9 { get; set; }
        public string m9 { get; set; }
        public string PreNSQF { get; set; }
        public string NSQF { get; set; }
        public string NsqfsubS6Upd { get; set; }
        public string NsqfsubS6 { get; set; }
        public string ns10 { get; set; }
      
        
        public int Std_id { get; set; }
        public bool Agree { get; set; }
        public string form_Name { get; set; }
        public string Category { get; set; }
        public string Board { get; set; }
        public string Other_Board { get; set; }
        public string Board_Roll_Num { get; set; }
        public string Prev_School_Name { get; set; }
        public string Registration_num { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string AWRegisterNo { get; set; }
        public string Admission_Num { get; set; }
        public string Admission_Date { get; set; }
        public string Class_Roll_Num_Section { get; set; }
        //-----personal Information
        public int PI_Id { get; set; }
        public string Candi_Name { get; set; }
        public string Candi_Name_P { get; set; }
        public string Father_Name { get; set; }
        public string Father_Name_P { get; set; }
        public string Mother_Name { get; set; }
        public string Mother_Name_P { get; set; }
        public string Caste { get; set; }
        public string Gender { get; set; }
        public string Differently_Abled { get; set; }
        public string Religion { get; set; }
        public string DOB { get; set; }
        public string Belongs_BPL { get; set; }
        public string Mobile { get; set; }
        public string AadharEnroll { get; set; }
        public string Aadhar_num { get; set; }
        public string E_punjab_Std_id { get; set; }
        public string Address { get; set; }
        public string LandMark { get; set; }
        public string Block { get; set; }
        public int Tehsil { get; set; }
        public string MYTehsil { get; set; }
        public string DIST { get; set; }
        public string DISTNM { get; set; }
        public bool IsPrevSchoolSelf { get; set; }
        public bool IsPSEBRegNum { get; set; }
        public char Section { get; set; }
      

        public string PinCode { get; set; }
        public HttpPostedFileBase std_Photo { get; set; }
        public HttpPostedFileBase std_Sign { get; set; }
        public HttpPostedFileBase file { get; set; }
        public DateTime EnterDate { get; set; }
        //[Required(ErrorMessage = "Enter the Issued date.")]
        //[DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }
        public string fname { get; set; }
        public string SCHL { get; set; }
        public string SESSION { get; set; }
        public string LOT { get; set; }
        public string IDNO { get; set; }
        public string MyGroup { get; set; }
        public string SelList { set; get; }
    }

    public class ShowModel
    {
        public int Id { get; set; }
        public string Year { get; set; }
        public string Lot { get; set; }
        public string form { get; set; }
        public string RegDate { get; set; }
        public string Exam { get; set; }
        public string SCHL { get; set; }
        public string regno { get; set; }
        public string Name { get; set; }
        public string FNAME { get; set; }
        public string MNAME { get; set; }
        public string DOB { get; set; }
        public string sex { get; set; }
        public string caste { get; set; }
        public int pagec { get; set; }
    }
    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            int MaxContentLength = 1024 * 1024 * 3; //3 MB
            string[] AllowedFileExtensions = new string[] { ".jpeg", ".jpg" };

            var file = value as HttpPostedFileBase;

            if (file == null)
                return false;
            else if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
            {
                ErrorMessage = "Please upload Your Photo of type: " + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "Your Photo is too large, maximum allowed size is : " + (MaxContentLength / 1024).ToString() + "MB";
                return false;
            }
            else
                return true;
        }
    }
}