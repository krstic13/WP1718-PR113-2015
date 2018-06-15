using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class Address
    {
        public string UlicaBroj { get; set; }
        public string MestoPostanski { get; set; }


        public Address()
        {
                
        }
        public Address(string ulicaIbroj, string mestoIpostanski)
        {
            UlicaBroj = ulicaIbroj;
            MestoPostanski = mestoIpostanski;
        }

    }
}