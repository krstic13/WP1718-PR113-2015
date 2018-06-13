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
        public User KorisnikKojiJeOstavioKomentar { get; set; }
        public Ride VoznjaNaKojuSeOdnosiKomentar { get; set; }
        public int Ocena { get; set; }

        public Comment()
        {

        }

        public Comment(string opis, DateTime datumobjave, User korisnikkojijeostaviokomentar, Ride voznjanakojuseodnosikomentar, int ocena)
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