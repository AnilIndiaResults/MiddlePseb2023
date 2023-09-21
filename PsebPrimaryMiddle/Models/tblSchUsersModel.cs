using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PsebJunior.Models
{
    public partial class tblSchUsers
    {
        [Key]
        public string geolocation { get; set; }
        public string imgpath { get; set; }
        public string schl { get; set; }
    }
}
