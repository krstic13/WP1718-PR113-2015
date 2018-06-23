using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class KorisnikPretraga
    {
        public DateTime OdVreme { get; set; }
        public DateTime DoVreme { get; set; }
        public int OdOcena { get; set; }
        public int DoOcena { get; set; }
        public int OdCena { get; set; }
        public int DoCena { get; set; }

    }
}