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
        public double OdCena { get; set; }
        public double DoCena { get; set; }
        public int StatusVoznje { get; set; }
        public List<Ride> voznje { get; set; }

        public KorisnikPretraga()
        {
            voznje = new List<Ride>();
        }

    }
}