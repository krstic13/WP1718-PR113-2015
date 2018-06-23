using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public class VoznjeObj
    {
        public List<Ride> PoslateVoznje { get; set; }

        public VoznjeObj()
        {
            PoslateVoznje = new List<Ride>();
        }
    }
}