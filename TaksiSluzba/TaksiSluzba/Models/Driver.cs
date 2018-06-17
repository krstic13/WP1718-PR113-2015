using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class Driver:User
    {
        public Location Lokacija { get; set; }
        public Car Automobil { get; set; }
        public bool Slobodan { get; set; }

        public Driver()
        {
            Slobodan = true;
        }

        public Driver(Location lokacija, Car automobil)
        {
            Slobodan = true;
            Lokacija = lokacija;
            Automobil = automobil;
        }

        public Driver(Driver d)
        {
            this.Slobodan = d.Slobodan;
            this.Lokacija = d.Lokacija;
            this.Automobil = d.Automobil;
        }
    }
}