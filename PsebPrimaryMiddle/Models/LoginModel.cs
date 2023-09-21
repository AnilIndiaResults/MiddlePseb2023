using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PsebJunior.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "username")]
        public string UserName { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Required]
        [Display(Name = "Session")]
        public string Session { get; set; }
       

    }

    public class LoginSession
    {
        public string CurrentSession { get; set; }
        public string STATUS { get; set; }
        public string DIST { get; set; }
        public string SCHL { get; set; }
        //public string SCHLE { get; set; }
        //public string STATIONE { get; set; }
        //public string DISTE { get; set; }
        public string middle { get; set; }
        public string fifth { get; set; }

        public string Senior { get; set; }
        public string Matric { get; set; }
        public bool Approved { get; set; }
        public string MOBILE { get; set; }
        public string EMAILID { get; set; }
        public int LoginStatus { get; set; }
        public DateTime DateFirstLogin { get; set; }       
        public string SCHLNME { get; set; }
        public string PRINCIPAL { get; set; }
        public string SCHLNMP { get; set; }
        // NEw ADded
        public string EXAMCENT { get; set; }
        public string PRACCENT { get; set; }
        public string USERTYPE { get; set; }
        public string CLUSTERDETAILS { get; set; }

        public int IsMeritoriousSchool { get; set; }
    }


    public class SchoolDataBySchlModel
    {       
        public string STATUS { get; set; }      
        public string SCHL { get; set; }
        public string PASSWORD { get; set; }       
        public string middle { get; set; }
        public string fifth { get; set; }
        public bool Approved { get; set; }
        public string MOBILE { get; set; }
        public string EMAILID { get; set; }
        public int LoginStatus { get; set; }      
        public string SCHLNME { get; set; }       
    }


}