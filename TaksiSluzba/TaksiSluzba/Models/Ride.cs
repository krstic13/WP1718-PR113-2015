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
        public string Musterija { get; set; }
        public string MusterijaId { get; set; }
        public Location Odrediste { get; set; }
        public string Dispatcher { get; set; }
        public string Vozac { get; set; }
        public string VozacId { get; set; }
        public double Iznos { get; set; }
        public Comment Komentar { get; set; }
        public RideStatus StatusVoznje { get; set; }
        public CARTYPE TipVozila { get; set; }
        public string Id { get; set; }
        public string DATUMM { get; set; }

        public Ride()
        {

        }

        public Ride(DateTime datumporudzbine, Location lokacija, string musterija, Location odrediste, string vozac, double iznos, Comment komentar, RideStatus statusVoznje, string dispatcher, CARTYPE tipvozila)
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