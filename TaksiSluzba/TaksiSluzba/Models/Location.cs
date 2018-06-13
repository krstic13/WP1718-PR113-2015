using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class Location
    {
        public double Xcoordinate { get; set; }
        public double Ycoordinate { get; set; }
        public Address Adresa { get; set; }

        public Location()
        {

        }

        public Location(double xcoordinate, double ycoordinare, Address adresa)
        {
            Xcoordinate = xcoordinate;
            Ycoordinate = ycoordinare;
            Adresa = adresa;
        }

        public Location(Location l)
        {
            this.Adresa = l.Adresa;
            this.Xcoordinate = l.Xcoordinate;
            this.Ycoordinate = l.Ycoordinate;
        }
    }
}