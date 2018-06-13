using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class Address
    {
        public string Ulica { get; set; }
        public string NaseljenoMesto { get; set; }
        public int PozivniBroj { get; set; }
        public int Broj { get; set; }


        public Address()
        {
                
        }
        public Address(string ulica, string naseljenomesto, int pozivnibroj, int broj)
        {
            Ulica = ulica;
            NaseljenoMesto = naseljenomesto;
            PozivniBroj = pozivnibroj;
            Broj = broj;
        }

        public Address(Address a)
        {
            this.Ulica = a.Ulica;
            this.NaseljenoMesto = a.NaseljenoMesto;
            this.PozivniBroj = a.PozivniBroj;
            this.Broj = a.Broj;
        }
    }
}