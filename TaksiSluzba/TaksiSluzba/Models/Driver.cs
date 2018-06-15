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

        public Driver()
        {

        }

        public Driver(Location lokacija, Car automobil)
        {
            Lokacija = lokacija;
            Automobil = automobil;
        }

        public Driver(Driver d)
        {
            this.Lokacija = d.Lokacija;
            this.Automobil = d.Automobil;
        }
    }
}