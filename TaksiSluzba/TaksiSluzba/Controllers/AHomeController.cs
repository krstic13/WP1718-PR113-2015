﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml.Serialization;
using TaksiSluzba.Models;

namespace TaksiSluzba.Controllers
{
    public class AHomeController : ApiController
    {
        public static List<User> korisnici = new List<User>(); // musterije
        public static List<Driver> vozaci = new List<Driver>();
        public static List<User> admin = new List<User>();
        public static Dictionary<string, string> ulogovani = new Dictionary<string, string>();
        public static Dictionary<string, string> blokirani = new Dictionary<string, string>();

        public static List<Ride> korisnikKreiraoVoznju = new List<Ride>();
        public static List<Ride> sveVoznje = new List<Ride>();  // sadrzi sve voznje i one koje se dodeljuju nekome i one koje nisu dodeljene 

        [HttpGet, Route("")]
        public RedirectResult Index()
        {
            ReadFromXML(ROLE.ADMIN);
            ReadFromXML(ROLE.DRIVER);
            ReadFromXML(ROLE.USER);
            ReadAllRides();
            var requestUri = Request.RequestUri;
            return Redirect(requestUri.AbsoluteUri + "Content/index.html");
        }

        [HttpGet]
        [Route("api/ahome/login")]
        public IHttpActionResult Prijava([FromUri]UserLogin user)
        {

            foreach (User u in korisnici)
            {
                if (u.UserName == user.Username && u.Password == user.Password)
                {
                    if (ulogovani.Keys.Contains(u.Id))
                    {
                        // Prosledi informaciju da je vec ulogovan korisnik i da ispise gresku
                        return Ok("Korisnik je već ulogovan, nemoguće duplo logovanje");
                    }
                    else {
                        ulogovani.Add(u.Id, u.UserName);
                    }

                    return Ok(u);
                }
            }

            foreach (Driver u in vozaci)
            {
                if (u.UserName == user.Username && u.Password == user.Password)
                {
                    if (ulogovani.Keys.Contains(u.Id))
                    {
                        // Prosledi informaciju da je vec ulogovan korisnik i da ispise gresku
                        return Ok("Korisnik je već ulogovan, nemoguće duplo logovanje");
                    }
                    else
                    {
                        ulogovani.Add(u.Id, u.UserName);
                    }
                    return Ok(u);
                }
            }

            foreach (User u in admin)
            {
                if (u.UserName == user.Username && u.Password == user.Password)
                {
                    if (ulogovani.Keys.Contains(u.Id))
                    {
                        // Prosledi informaciju da je vec ulogovan korisnik i da ispise gresku
                        return Ok("Korisnik je već ulogovan, nemoguće duplo logovanje");
                    }
                    else
                    {
                        ulogovani.Add(u.Id, u.UserName);
                    }
                    return Ok(u);
                }
            }

            return Ok("Korisnik ne postoji");
        }

        [HttpGet]
        [Route("api/ahome/changeuser")]
        public IHttpActionResult ChangeUser([FromUri]User u)
        {
            //Implement changes here
            if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == "")
            {
                return Ok("Jedno od polja je ostalo prazno, izmene nedozvoljene.");
            }

            User pomocni = new User();

            if (ulogovani.Keys.Contains(u.Id))
            {
                //pomocni = ulogovani[u.Id];
                pomocni = korisnici.Find(uu => uu.Id == u.Id);
                ulogovani.Remove(u.Id);         // obrisali ga iz trenutno ulogovanih
                korisnici.Remove(pomocni);      // obrisali ga iz liste korisnika
            } else
            {
                return Ok("Error occured");
            }

            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                korisnici.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            pomocni.UserName = u.UserName;
            pomocni.Email = u.Email;
            pomocni.Gender = u.Gender;
            pomocni.JMBG = u.JMBG;
            pomocni.LastName = u.LastName;
            pomocni.Name = u.Name;
            pomocni.Password = u.Password;
            pomocni.PhoneNumber = u.PhoneNumber;

            korisnici.Add(pomocni);
            ulogovani.Add(pomocni.Id, pomocni.UserName);
            WriteToXMl(ROLE.USER);

            return Ok(pomocni);
        }

        [HttpGet]
        [Route("api/ahome/changedriver")]
        public IHttpActionResult ChangeDriver([FromUri]Driver u)
        {
            if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == ""
                || u.Automobil.GodisteAutomobila == "" || u.Automobil.Registracija == "")
            {
                return Ok("Jedno od polja je ostalo prazno, izmene nedozvoljene.");
            }

            Driver pomocni = new Driver();

            if (ulogovani.Keys.Contains(u.Id))
            {
                // pomocni = ulogovani[u.Id];
                pomocni = vozaci.Find(uu => uu.Id == u.Id);
                ulogovani.Remove(u.Id);         // obrisali ga iz trenutno ulogovanih
                vozaci.Remove(pomocni);      // obrisali ga iz liste korisnika
            }
            else
            {
                return Ok("Error occured");
            }

            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                vozaci.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            pomocni.UserName = u.UserName;
            pomocni.Email = u.Email;
            pomocni.Gender = u.Gender;
            pomocni.JMBG = u.JMBG;
            pomocni.LastName = u.LastName;
            pomocni.Name = u.Name;
            pomocni.Password = u.Password;
            pomocni.PhoneNumber = u.PhoneNumber;

            pomocni.Automobil.GodisteAutomobila = u.Automobil.GodisteAutomobila;
            pomocni.Automobil.Registracija = u.Automobil.Registracija;
            pomocni.Automobil.TaxiId = u.Automobil.TaxiId;
            pomocni.Automobil.TipAutomobila = u.Automobil.TipAutomobila;

            pomocni.Lokacija.Adresa.MestoPostanski = u.Lokacija.Adresa.MestoPostanski;
            pomocni.Lokacija.Adresa.UlicaBroj = u.Lokacija.Adresa.UlicaBroj;
            pomocni.Lokacija.Xcoordinate = u.Lokacija.Xcoordinate;
            pomocni.Lokacija.Ycoordinate = u.Lokacija.Ycoordinate;

            vozaci.Add(pomocni);
            ulogovani.Add(pomocni.Id, pomocni.UserName);
            WriteToXMl(ROLE.DRIVER);

            return Ok(pomocni);
        }

        [HttpGet]
        [Route("api/ahome/changeadmin")]
        public IHttpActionResult ChangeAdmin([FromUri]User u)
        {
            if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == "")
            {
                return Ok("Jedno od polja je ostalo prazno, izmene nedozvoljene.");
            }

            User pomocni = new User();

            if (ulogovani.Keys.Contains(u.Id))
            {
                //pomocni = ulogovani[u.Id];
                pomocni = admin.Find(uu => uu.Id == u.Id);
                ulogovani.Remove(u.Id);         // obrisali ga iz trenutno ulogovanih
                admin.Remove(pomocni);      // obrisali ga iz liste korisnika
            }
            else
            {
                return Ok("Error occured");
            }

            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                admin.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            pomocni.UserName = u.UserName;
            pomocni.Email = u.Email;
            pomocni.Gender = u.Gender;
            pomocni.JMBG = u.JMBG;
            pomocni.LastName = u.LastName;
            pomocni.Name = u.Name;
            pomocni.Password = u.Password;
            pomocni.PhoneNumber = u.PhoneNumber;

            admin.Add(pomocni);
            ulogovani.Add(pomocni.Id, pomocni.UserName);
            WriteToXMl(ROLE.ADMIN);

            return Ok(pomocni);
        }

        [HttpGet]
        [Route("api/ahome/blocking")]
        public IHttpActionResult Blocking([FromUri]Driver u)
        {
            /* if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == ""
                 || u.Automobil.GodisteAutomobila == "" || u.Automobil.Registracija == "")
             {
                 return Ok("Jedno od polja je ostalo prazno, izmene nedozvoljene.");
             }

             if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
             {
                 return Ok("Ovo korisnicko ime vec postoji, dodavanje nije moguce");
             }

             u.Blokiran = false;
             u.Id = (korisnici.Count() + admin.Count() + vozaci.Count()).ToString();
             u.Uloga = ROLE.DRIVER;
             u.Voznje = new List<Ride>();

             vozaci.Add(u);
             */
            return Ok(u);
        }

        [HttpGet]
        [Route("api/ahome/najblizi")]
        public IHttpActionResult Najblizi([FromUri]double x, [FromUri]double y)
        {
            List<Driver> slobodniVozaci = new List<Driver>();
            Dictionary<string, double> udaljenosti = new Dictionary<string, double>();

            foreach (Driver d in vozaci) {
                if(d.Slobodan == true)
                {
                    slobodniVozaci.Add(d);
                }
            }

            foreach (Driver d in slobodniVozaci)
            {
                Double xx = Convert.ToDouble(d.Lokacija.Xcoordinate);
                Double yy = Convert.ToDouble(d.Lokacija.Ycoordinate);
                Double r = Math.Sqrt(Math.Pow((xx - yy), 2) + Math.Pow((x - y), 2));
                udaljenosti.Add(d.Id,r);
            }

            udaljenosti = udaljenosti.OrderBy(o=>o.Value).ToDictionary(k=>k.Key, k=>k.Value);

            Dictionary<string, string> povratni = new Dictionary<string, string>();
            int counter = 0;

            foreach (string i in udaljenosti.Keys)
            {
                Driver d = slobodniVozaci.Find(D => D.Id == i);
                povratni.Add(d.Id,d.UserName);
                counter++;
                if (counter == 5) break;
            }

            return Ok(povratni);
        }

        [Route("api/ahome/kreiranjekorisnika")]
        public IHttpActionResult KreiranjeKorisnika(Driver u)
        {
            if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == ""
                || u.Automobil.GodisteAutomobila == "" || u.Automobil.Registracija == "")
            {
                return Ok("Jedno od polja je ostalo prazno, izmene nedozvoljene.");
            }

            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                return Ok("Ovo korisnicko ime vec postoji, dodavanje nije moguce");
            }

            u.Blokiran = false;
            u.Id = (korisnici.Count() + admin.Count() + vozaci.Count() + 1).ToString();
            u.Uloga = ROLE.DRIVER;
            u.Voznje = new List<Ride>();

            vozaci.Add(u);
            WriteToXMl(ROLE.DRIVER);

            return Ok(u);
        }

        [Route("api/ahome/registration")]
        public IHttpActionResult Registracija(User u)
        {
            if (u.Email == "" || u.JMBG == "" || u.LastName == "" || u.Name == "" || u.Password == "" || u.PhoneNumber == "" || u.UserName == "")
            {
                return Ok("Neispravna registracija koristnika");
            }

            if (korisnici.Exists(k => k.UserName == u.UserName))
            {
                return Ok("Korisnik vec postoji");
            }

            if (admin.Exists(k => k.UserName == u.UserName))
            {
                return Ok("Korisnik vec postoji");
            }

            if (vozaci.Exists(k => k.UserName == u.UserName))
            {
                return Ok("Korisnik vec postoji");
            }

            u.Uloga = ROLE.USER;
            u.Id = (korisnici.Count + admin.Count + vozaci.Count+ 1).ToString();
            korisnici.Add(u);

            WriteToXMl(ROLE.USER);

            return Ok(u);
        }

        [Route("api/ahome/kreiranjevoznje")]
        public IHttpActionResult KreiranjeVoznje(Ride r)
        {
            // FORMIRANA
            r.StatusVoznje = RideStatus.FORMIRANA;
            User dispecer;
            dispecer = admin.Find(k=> k.Id == r.Dispatcher);
            //
            admin.Remove(dispecer);
            
            //

            r.Dispatcher = dispecer.UserName;

            Driver d = vozaci.Find(k => k.Id == r.Vozac);
            //
            vozaci.Remove(d);
            //

            r.Id = r.Vozac +"_"+d.Voznje.Count;
            r.DatumIVremePorudzbine = DateTime.Now;

            d.Voznje.Add(r);
            dispecer.Voznje.Add(r);

            admin.Add(dispecer);
            vozaci.Add(d);

            sveVoznje.Add(r);

            WriteAllRides();

            return Ok(r);
        }

        [HttpDelete]
        [Route("api/ahome/logoutuser")]
        public IHttpActionResult LogOut(User u)
        {

            if (ulogovani.Keys.Contains(u.Id))
            {
                ulogovani.Remove(u.Id);
            }

            return Ok("Loged Out");
        }

        private void ReadAllRides()
        {
            string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\RIDES.xml";
            XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(List<Ride>));

            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                   sveVoznje = (List<Ride>)serializer.Deserialize(reader);
                }
            }
            catch { }
        }

        private void WriteToXMl(ROLE uloga) // za svako dodavanje i izmenu 
        {
            string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\" + uloga.ToString() + ".xml";
            XmlSerializer serializer;
            if (uloga == ROLE.DRIVER)
                serializer = new XmlSerializer(typeof(List<Driver>));
            else
                serializer = new XmlSerializer(typeof(List<User>));

            using (TextWriter writer = new StreamWriter(path))
            {
                if (uloga == ROLE.DRIVER)
                    serializer.Serialize(writer, vozaci);
                else if (uloga == ROLE.USER)
                    serializer.Serialize(writer, korisnici);
                else
                    serializer.Serialize(writer, admin);
            }
        }

        private void ReadFromXML(ROLE uloga)
        {
            string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\" + uloga.ToString() + ".xml";
            XmlSerializer serializer;
            if (uloga == ROLE.DRIVER)
                serializer = new XmlSerializer(typeof(List<Driver>));
            else
                serializer = new XmlSerializer(typeof(List<User>));

            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    if (uloga == ROLE.ADMIN)
                        admin = (List<User>)serializer.Deserialize(reader);
                    else if (uloga == ROLE.USER)
                        korisnici = (List<User>)serializer.Deserialize(reader);
                    else if (uloga == ROLE.DRIVER)
                        vozaci = (List<Driver>)serializer.Deserialize(reader);
                }
            }
            catch { }
        }

        private void WriteAllRides()
        {
            string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\RIDES.xml";
            XmlSerializer serializer;
                serializer = new XmlSerializer(typeof(List<Ride>));

            using (TextWriter writer = new StreamWriter(path))
            {
                    serializer.Serialize(writer, sveVoznje);
            }
        }

        
    }
}

//svaki gumb za admina da se odradi

/*
 $.ajax({
                    url: "/api/main/logoutuser",
                    data: un,
                    type: "DELETE"              
                });
*/
