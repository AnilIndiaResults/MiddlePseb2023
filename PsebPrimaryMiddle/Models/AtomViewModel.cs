using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PsebJunior.Models
{
    public class AtomViewModel
    {
        public AtomViewModel(string checkoutUrl)
        {
            CheckoutUrl = checkoutUrl;
        }
        public string CheckoutUrl { get; set; }
    }
}