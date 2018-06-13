using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public enum RideStatus {KREIRANA,FORMIRANA,OBRADJENA,PRIHVACENA,OTKAZANA,NEUSPESNA,USPESNA}
    public class Ride
    {
        public DateTime DatumPorudzbine { get; set; }
        public DateTime VremePorudzbine { get; set; }
        public Location Lokacija { get; set; }
        public User Musterija { get; set; }
        public Location Odrediste { get; set; }
        //Dispecer
        public Driver Vozac { get; set; }
        public Double Iznos { get; set; }
        public Comment Komentar { get; set; }
        public RideStatus StatusVoznje { get; set; }

        public Ride()
        {

        }

        public Ride(DateTime datumporudzbine, DateTime vremeporudzbine, Location lokacija, User musterija, Location odrediste, Driver vozac, double iznos, Comment komentar, RideStatus statusVoznje)
        {
            DatumPorudzbine = datumporudzbine;
            VremePorudzbine = vremeporudzbine;
            Lokacija = lokacija;
            Musterija = musterija;
            Odrediste = odrediste;
            Vozac = vozac;
            Iznos = iznos;
            Komentar = komentar;
            StatusVoznje = statusVoznje;
        }

        public Ride(Ride r)
        {
            this.DatumPorudzbine = r.DatumPorudzbine;
            this.VremePorudzbine = r.VremePorudzbine;
            this.Lokacija = r.Lokacija;
            this.Musterija = r.Musterija;
            this.Odrediste = r.Odrediste;
            this.Vozac = r.Vozac;
            this.Iznos = r.Iznos;
            this.Komentar = r.Komentar;
            this.StatusVoznje = r.StatusVoznje;
        }
             

    }
}