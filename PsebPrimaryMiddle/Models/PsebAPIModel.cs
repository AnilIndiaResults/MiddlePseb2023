using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PsebJunior.Models
{
    public class PsebAPIModel
    {
    }

    public class SchoolPasswordAPIViewModel
    {
        public string Success { get; set; }
        public string statusCode { get; set; }
        public string SuccessMessage { get; set; }
        public SchoolChangePasswordModel Object { get; set; }
    }

    public class SchoolApiViewModel
    {
        public string Success { get; set; }
        public string statusCode { get; set; }
        public string SuccessMessage { get; set; }
        public SchoolModels Object { get; set; }  
    }
}