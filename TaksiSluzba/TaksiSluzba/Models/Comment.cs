using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class Comment
    {
        public string Opis { get; set; }
        public DateTime DatumObjave { get; set; }
        public string KorisnikKojiJeOstavioKomentar { get; set; }
        public Ride VoznjaNaKojuSeOdnosiKomentar { get; set; }
        public string Ocena { get; set; }

        public Comment()
        {

        }

        public Comment(string opis, DateTime datumobjave, string korisnikkojijeostaviokomentar, Ride voznjanakojuseodnosikomentar, string ocena)
        {
            Opis = opis;
            DatumObjave = datumobjave;
            KorisnikKojiJeOstavioKomentar = korisnikkojijeostaviokomentar;
            VoznjaNaKojuSeOdnosiKomentar = voznjanakojuseodnosikomentar;
            Ocena = ocena;
        }

        public Comment(Comment c)
        {
            this.Opis = c.Opis;
            this.DatumObjave = c.DatumObjave;
            this.KorisnikKojiJeOstavioKomentar = c.KorisnikKojiJeOstavioKomentar;
            this.VoznjaNaKojuSeOdnosiKomentar = c.VoznjaNaKojuSeOdnosiKomentar;
            this.Ocena = c.Ocena;
        }

    }
}