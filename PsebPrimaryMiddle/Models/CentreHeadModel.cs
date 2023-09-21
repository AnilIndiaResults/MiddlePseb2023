using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web.Mvc;

namespace PsebJunior.Models
{

    public class CenterHeadLoginSession
    {
        public string CurrentSession { get; set; }
        public int CenterHeadId { get; set; }
        public string UserName { get; set; }
        public string CenterHeadName { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
        public string SchoolAllows { get; set; }     
        public bool IsActive { get; set; }
        public int LoginStatus { get; set; }

        public string CenterInchargeName { get; set; }
        public string ChtUdise { get; set; }
        public string ClusterName { get; set; }    }


    public class CenterHeadMasterModelList
    {
        public List<CenterHeadMasterModel> centerHeadMasterModels { set; get; }
    }

    public class CenterHeadMasterModel
    {       
        public int CenterHeadId { get; set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public string CenterHeadName { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
        public string SchoolAllows { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }


    public class CenterSchoolModelList
    {
        public List<CenterSchoolModel> centerSchoolModels { set; get; }
    }


    public class CenterSchoolModel
    {
        public string Schl { get; set; }
        public string SchlNME { get; set; }
        public string Udisecode { get; set; }
        public string FifSet { get; set; }
        public string MidSet { get; set; }
        public string Dist { get; set; }
        public string DistNM { get; set; }
        public string UserType { get; set; }
        public string Fifth { get; set; }
        public string Middle { get; set; }
        public int NOP { get; set; }
        public int IsMarkedFilled { get; set; }
        public DateTime LastDate { get; set; }
        public int IsActive { get; set; }
        public string FinalStatus { get; set; }
        public string FinalSubmitOn  { get; set; }
        public string FinalSubmitLot { get; set; }
        public string FinalSubmitBy { get; set; }

    }

    public class CenterHeadModel
    {
        public DataSet StoreAllData { get; set; }
        public int CenterHeadId { get; set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public string CenterHeadName { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
        public bool IsActive { get; set; }
        public string SchoolAllows { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }       
    }

    public class SchoolAllotedToCenterMaster
    {
        public int Id { get; set; }
        public string CenterId { get; set; }
        public string Schl { get; set; }
        public int IsActive { get; set; }
        public DateTime LastDate { get; set; }
        public int IsMarkedFilled { get; set; }
        public string FinalSubmitOn { get; set; }
        public string FinalSubmitLot { get; set; }
        public string FinalSubmitBy { get; set; }
        public string SCHLNME { get; set; }
    }

    public class ComplaintTypeMasterModel
    {
        public int ComplaintTypeId { get; set; }
        public string ComplaintTypeName { get; set; }     
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }


    public class GenerateTicketModel
    {
        public int ActionType { get; set; }
        public HttpPostedFileBase file { get; set; }
        public List<ComplaintTypeMasterModel> complaintTypeMasterModels { get; set; }
        public List<GenerateTicketModel> generateTicketModels { get; set; }
        public DataSet StoreAllData { get; set; }
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public int ComplaintTypeId { get; set; }
        public string CenterHeadUserName { get; set; }
        public string SchoolCode { get; set; }
        public string QueryMessage { get; set; }
        public string Filepath { get; set; }
        public int IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? AdminId { get; set; }
        public int? TicketStatus { get; set; }
        public string TicketReason { get; set; }
        public string TicketStatusBy { get; set; }

        public string FinalTicketStatus { get; set; }        
        public string ComplaintTypeName { get; set; }
      

    }
}