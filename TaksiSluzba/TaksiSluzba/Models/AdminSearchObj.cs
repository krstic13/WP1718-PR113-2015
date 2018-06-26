using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class AdminSearchObj:KorisnikPretraga
    {
        public string ImeMusterije { get; set; }
        public string PrezimeMusterije { get; set; }
        public string ImeVozaca { get; set; }
        public string PrezimeVozaca { get; set; }

        public AdminSearchObj()
        {
            ImeMusterije = "";
            PrezimeMusterije = "";
            ImeVozaca = "";
            PrezimeVozaca = "";
        }
    }
}