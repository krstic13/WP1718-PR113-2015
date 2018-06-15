using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public enum RideStatus {KREIRANA,FORMIRANA,OBRADJENA,PRIHVACENA,OTKAZANA,NEUSPESNA,USPESNA,U_TOKU}
    public class Ride
    {
        public DateTime DatumIVremePorudzbine { get; set; }
        public Location LokacijaPolazna { get; set; }
        public User Musterija { get; set; }
        public Location Odrediste { get; set; }
        public User Dispatcher { get; set; }
        public Driver Vozac { get; set; }
        public string Iznos { get; set; }
        public Comment Komentar { get; set; }
        public RideStatus StatusVoznje { get; set; }
        public CARTYPE TipVozila { get; set; }
        public string Id { get; set; }

        public Ride()
        {

        }

        public Ride(DateTime datumporudzbine, Location lokacija, User musterija, Location odrediste, Driver vozac, string iznos, Comment komentar, RideStatus statusVoznje, User dispatcher, CARTYPE tipvozila)
        {
            DatumIVremePorudzbine = datumporudzbine;
            LokacijaPolazna = lokacija;
            Musterija = musterija;
            Odrediste = odrediste;
            Vozac = vozac;
            Iznos = iznos;
            Komentar = komentar;
            StatusVoznje = statusVoznje;
            Dispatcher = dispatcher;
            TipVozila = tipvozila;
        }

        public Ride(Ride r)
        {
            this.DatumIVremePorudzbine = r.DatumIVremePorudzbine;
            this.LokacijaPolazna = r.LokacijaPolazna;
            this.Musterija = r.Musterija;
            this.Odrediste = r.Odrediste;
            this.Vozac = r.Vozac;
            this.Iznos = r.Iznos;
            this.Komentar = r.Komentar;
            this.StatusVoznje = r.StatusVoznje;
        }
             

    }
}