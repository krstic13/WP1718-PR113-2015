using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class KorisnikPretraga
    {
        public string OdVreme { get; set; }
        public string DoVreme { get; set; }
        public string OdOcena { get; set; }
        public string DoOcena { get; set; }
        public string OdCena { get; set; }
        public string DoCena { get; set; }
        public int StatusVoznje { get; set; }
        public VoznjeObj voznje { get; set; }

        public KorisnikPretraga()
        {
            //voznje = new List<Ride>();
        }

    }
}