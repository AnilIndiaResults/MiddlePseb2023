using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PsebJunior.Models
{

    public class EAffiliationFee
    {
        public DataSet PaymentFormData { get; set; }
        public int Class { get; set; }
        public string FORM { get; set; }
        public string sDate { get; set; }
        public string eDate { get; set; }
        public int Fee { get; set; }
        public int LateFee { get; set; }
        public int TotFee { get; set; }
        public int FeeCode { get; set; }
        public string FeeCat { get; set; }
        public string BankLastDate { get; set; }
        public string Type { get; set; }
        public int IsActive { get; set; }
        public int ID { get; set; }
        public string AllowBanks { get; set; }
        public string RP { get; set; }
    }
    public class SelectListItemCheckBox
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }

    public class SiteMenu
    {
        public int MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public int ParentMenuID { get; set; }
        public int IsMenu { get; set; }
        public bool IsSelected { get; set; }
        public DataSet StoreAllData;
    }

    public class FileDetail
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }

    public class UploadImgndSignature
    {
        public HttpPostedFileBase std_Photo { get; set; }
        public HttpPostedFileBase std_Sign { get; set; }
    }


}