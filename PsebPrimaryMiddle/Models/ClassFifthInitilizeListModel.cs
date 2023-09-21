using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PsebJunior.Models
{
    public class ClassFifthInitilizeListModel
    {
        public List<SelectListItem> YesNoListText { get; set; }
        public List<SelectListItem> MyMedium { get; set; }
        public List<SelectListItem> MyCaste { get; set; }
        public List<SelectListItem> MyReligion { get; set; }
        public List<SelectListItem> MyBoard { get; set; }
        public List<SelectListItem> Mon { get; set; }
        public List<SelectListItem> MyWritter { get; set; }
        public List<SelectListItem> SectionList { get; set; }
        //DB
        public List<SelectListItem> SessionYearList { get; set; }
        public List<SelectListItem> DAList { get; set; }
        public List<SelectListItem> MyDist { get; set; }
    }


    public class ClassMiddleInitilizeListModel
    {
        public List<SelectListItem> YesNoListText { get; set; }
        public List<SelectListItem> MyMedium { get; set; }
        public List<SelectListItem> MyCaste { get; set; }
        public List<SelectListItem> MyReligion { get; set; }
        public List<SelectListItem> MyBoard { get; set; }
        public List<SelectListItem> Mon { get; set; }
        public List<SelectListItem> MyWritter { get; set; }
        public List<SelectListItem> SectionList { get; set; }
        //DB
        public List<SelectListItem> SessionYearList { get; set; }
        public List<SelectListItem> DAList { get; set; }
        public List<SelectListItem> MyDist { get; set; }
        public List<SelectListItem> ElectiveSubjects { get; set; }
        
    }
}