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
        public String Objavljeno { get; set; }
        public string KorisnikKojiJeOstavioKomentar { get; set; }
        public string IdVoznje { get; set; }
        public string Ocena { get; set; }

        public Comment()
        {

        }

        public Comment(string opis, DateTime datumobjave, string korisnikkojijeostaviokomentar, string voznjanakojuseodnosikomentar, string ocena)
        {
            Opis = opis;
            DatumObjave = datumobjave;
            KorisnikKojiJeOstavioKomentar = korisnikkojijeostaviokomentar;
            IdVoznje = voznjanakojuseodnosikomentar;
            Ocena = ocena;
        }

        public Comment(Comment c)
        {
            this.Opis = c.Opis;
            this.DatumObjave = c.DatumObjave;
            this.KorisnikKojiJeOstavioKomentar = c.KorisnikKojiJeOstavioKomentar;
            this.IdVoznje = c.IdVoznje;
            this.Ocena = c.Ocena;
        }

    }
}