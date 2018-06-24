using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class UdaljenostiObj
    {
        public Location TrentutnaLokacijaVozaca { get; set; }
        public List<Ride> PoslateVoznje { get; set; }

        public UdaljenostiObj()
        {
            PoslateVoznje = new List<Ride>();
            TrentutnaLokacijaVozaca = new Location();
        }
    }
}