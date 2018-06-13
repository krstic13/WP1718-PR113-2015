using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public enum CARTYPE { putnicki, kombi}

    public class Car
    {
        public Driver Vozac { get; set; }
        public int GodisteAutomobila { get; set; }
        public string Registracija { get; set; }
        public string TaxiId { get; set; }
        public CARTYPE TipAutomobila { get; set; }

        public Car()
        {

        }

        public Car(Driver vozac, int godisteautomobila, string registracija, string taxiid, CARTYPE tipautomobila)
        {
            Vozac = vozac;
            GodisteAutomobila = godisteautomobila;
            Registracija = registracija;
            TaxiId = taxiid;
            TipAutomobila = tipautomobila;
        }


        public Car(Car c)
        {
            this.Vozac = c.Vozac;
            this.GodisteAutomobila = c.GodisteAutomobila;
            this.Registracija = c.Registracija;
            this.TaxiId = c.TaxiId;
            this.TipAutomobila = c.TipAutomobila;
        }

    }
}