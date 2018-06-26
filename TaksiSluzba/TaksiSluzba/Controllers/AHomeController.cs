using System;
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
        public static Dictionary<string, string> neBlokirani = new Dictionary<string, string>();
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
            ReadCustomerMadeRide();
            var requestUri = Request.RequestUri;

            foreach (User u in korisnici)
            {
                if (u.Blokiran == true)
                {
                    blokirani.Add(u.Id, u.UserName);
                }
                else {
                    neBlokirani.Add(u.Id, u.UserName);
                }
            }

            foreach (Driver d in vozaci) {
                if (d.Blokiran == true)
                {
                    blokirani.Add(d.Id, d.UserName);
                }
                else
                {
                    neBlokirani.Add(d.Id, d.UserName);
                }
            }
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
                    else if (u.Blokiran == true)
                    {
                        return Ok("Korisnik je blokiran, logovanje nije moguce");
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
                    else if (u.Blokiran == true)
                    {
                        return Ok("Korisnik je blokiran, logovanje nije moguce");
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
            //
            if (u.JMBG.Count() != 13) {
                return Ok("Dužina matičnog broja mora biti 13 brojeva.");
            }

            bool digitsOnly = u.JMBG.All(char.IsDigit);
            

            if (!digitsOnly) { return Ok("Matični broj ne sme posedovati slovo u sebi."); }

            if (!u.Email.Contains('@')) { return Ok("Email nije korektnog formata."); }

            if (u.PhoneNumber.Count() < 6 || u.PhoneNumber.Any(char.IsLetter)) { return Ok("Broj telefona mora biti duži od 6 i ne sme imati slova"); }
            //

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
            u.Id = pomocni.Id;
            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                korisnici.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            if (u.UserName != pomocni.UserName)
            {
                // izmenimo u svakoj voznji koja postoji
                IzmeniSveVoznjeAdmina(u.Id, u.UserName);
                IzmeniKorisnikaUVozacima(u.Id, u.UserName);
                IzmeniKorisnikaUAdminima(u.Id, u.UserName);
                IzmeniSveVoznjeAdmina(u.Id, u.UserName);
                IzmeniKorisnikKreiraoAdmina(u.Id, u.UserName);
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
            //
            if (u.JMBG.Count() != 13)
            {
                return Ok("Dužina matičnog broja mora biti 13 brojeva.");
            }

            bool digitsOnly = u.JMBG.All(char.IsDigit);


            if (!digitsOnly) { return Ok("Matični broj ne sme posedovati slovo u sebi."); }

            if (!u.Email.Contains('@')) { return Ok("Email nije korektnog formata."); }

            if (u.PhoneNumber.Count() < 6 || u.PhoneNumber.Any(char.IsLetter)) { return Ok("Broj telefona mora biti duži od 6 i ne sme imati slova"); }
            //
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
            u.Id = pomocni.Id;

            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                vozaci.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            if (u.UserName != pomocni.UserName)
            {
                // izmenimo u svakoj voznji koja postoji
                //IzmeniSveVoznjeAdmina(u.Id, u.UserName);
                //IzmeniKorisnikaUVozacima(u.Id, u.UserName);
                //IzmeniKorisnikaUAdminima(u.Id, u.UserName);
                IzmeniVozacaUKorisnicima(u.Id,u.UserName);
                IzmeniVozacaUVozacima(u.Id,u.UserName);
                IzmeniVozacaUAdminima(u.Id,u.UserName);
                IzmeniSveVoznjeAdmina(u.Id, u.UserName);
                IzmeniKorisnikKreiraoAdmina(u.Id, u.UserName);
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
            //
            if (u.JMBG.Count() != 13)
            {
                return Ok("Dužina matičnog broja mora biti 13 brojeva.");
            }

            bool digitsOnly = u.JMBG.All(char.IsDigit);


            if (!digitsOnly) { return Ok("Matični broj ne sme posedovati slovo u sebi."); }

            if (!u.Email.Contains('@')) { return Ok("Email nije korektnog formata."); }

            if (u.PhoneNumber.Count() < 6 || u.PhoneNumber.Any(char.IsLetter)) { return Ok("Broj telefona mora biti duži od 6 i ne sme imati slova"); }
            //

            User pomocni = new User();

            if (ulogovani.Keys.Contains(u.Id))
            {
                pomocni = admin.Find(uu => uu.Id == u.Id);
                ulogovani.Remove(u.Id);         // obrisali ga iz trenutno ulogovanih
                admin.Remove(pomocni);      // obrisali ga iz liste korisnika
            }
            else
            {
                return Ok("Error occured");
            }
            u.Id = pomocni.Id;
            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                admin.Add(pomocni);
                ulogovani.Add(pomocni.Id, pomocni.UserName);
                return Ok("Ovo korisnicko ime vec postoji izmene nisu moguce");
            }
            if (u.UserName != pomocni.UserName) {
                // izmenimo u svakoj voznji koja postoji
                IzmeniAdminaUVozacima(u.Id,u.UserName);
                IzmeniAdminaUKorisnicima(u.Id,u.UserName);
                IzmeniAdminoveVoznje(u.Id,u.UserName);
                IzmeniKorisnikKreiraoAdmina(u.Id,u.UserName);
                IzmeniSveVoznjeAdmina(u.Id,u.UserName);
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
        
        [HttpGet, Route("api/ahome/blockedList")]
        public IHttpActionResult BlockedList()
        {
            return Ok(blokirani);
        }

        [HttpGet]
        [Route("api/ahome/unblockedList")]
        public IHttpActionResult UnBlockedList() {
            return Ok(neBlokirani);
        }

        [HttpGet]
        [Route("api/ahome/najblizi")]
        public IHttpActionResult Najblizi([FromUri]double x, [FromUri]double y)
        {
            List<Driver> slobodniVozaci = new List<Driver>();
            Dictionary<string, double> udaljenosti = new Dictionary<string, double>();

            foreach (Driver d in vozaci) {
                if (d.Blokiran == false)
                {
                    if (d.Slobodan == true)
                    {
                        //if (d.Automobil.TipAutomobila == mm.TipVozila)
                        //{
                            slobodniVozaci.Add(d);
                        //}
                        //slobodniVozaci.Add(d);
                    }
                }
            }

            foreach (Driver d in slobodniVozaci)
            {
                Double xx = Convert.ToDouble(d.Lokacija.Xcoordinate);
                Double yy = Convert.ToDouble(d.Lokacija.Ycoordinate);
                /* Double r = Math.Sqrt(Math.Pow((xx - yy), 2) + Math.Pow((x - y), 2));*/
                Double r = Math.Sqrt(Math.Pow((xx - x), 2) + Math.Pow((yy - y), 2));
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

            //
            if (u.JMBG.Count() != 13)
            {
                return Ok("Dužina matičnog broja mora biti 13 brojeva.");
            }

            bool digitsOnly = u.JMBG.All(char.IsDigit);


            if (!digitsOnly) { return Ok("Matični broj ne sme posedovati slovo u sebi."); }

            if (!u.Email.Contains('@')) { return Ok("Email nije korektnog formata."); }

            if (u.PhoneNumber.Count() < 6 || u.PhoneNumber.Any(char.IsLetter)) { return Ok("Broj telefona mora biti duži od 6 i ne sme imati slova"); }
            //
            if (korisnici.Exists(k => k.UserName == u.UserName) || admin.Exists(a => a.UserName == u.UserName) || vozaci.Exists(v => v.UserName == u.UserName))
            {
                return Ok("Ovo korisnicko ime vec postoji, dodavanje nije moguce");
            }

            u.Blokiran = false;
            u.Id = (korisnici.Count() + admin.Count() + vozaci.Count() + 1).ToString();
            u.Uloga = ROLE.DRIVER;
            u.Voznje = new List<Ride>();

            neBlokirani.Add(u.Id, u.UserName);

            vozaci.Add(u);
            WriteToXMl(ROLE.DRIVER);

            return Ok(u);
        }

        [Route("api/ahome/izmenaLokacije")]
        public IHttpActionResult IzmeniLokacijuVozaca(Driver korisnik) {

            // proveri da nije poslat null 
            if(korisnik.Lokacija!= null) {
                // Driver d = vozaci.Find(dd => dd.Id == korisnik.Id);
                Driver d = new Driver();
                foreach (Driver k in vozaci) {
                    if (k.Id == korisnik.Id) { d = k; break; }
                        
                }

                if (d != null) {
                vozaci.Remove(d);
                d.Lokacija = korisnik.Lokacija;
                vozaci.Add(d);
                return Ok(d);
                }
            }

            return Ok("Nije prosledjena validna lokacija.");
        }

        [Route("api/ahome/korisnikKreiraVoznju")]
        public IHttpActionResult korisnikKreiraVoznju(Ride r) // User u = korisnici.Find(uu=> uu.Id == r.MusterijaId); OVU LINIJU SVUDA PROVERI
        {
            if (r.LokacijaPolazna == null)
            {
                return Ok("");
            }
            r.StatusVoznje = RideStatus.KREIRANA;
            // NADJI TOG KORISNIKA U LISTI KORISNIKA I SMESTI VOZNJU U NJEGOVOJ LISTI VOZNI
            User u = korisnici.Find(uu=> uu.Id == r.MusterijaId);

            r.Musterija = u.UserName;
            r.MusterijaId = u.Id;
            //r.Musterija = u.UserName; // Musterija je zapravo id musterije
            korisnici.Remove(u);
            // ID VOZNJE DODELITI KADA SE DODELI ?? ODMAH
            r.Id = (sveVoznje.Count + korisnikKreiraoVoznju.Count).ToString();
            r.DatumIVremePorudzbine = DateTime.UtcNow;
            //r.DATUMM = (r.DatumIVremePorudzbine).ToString("MMMM dd, yyyy");
            r.DATUMM = (r.DatumIVremePorudzbine).ToString("dd/MM/yy  H:mm:ss");
            korisnikKreiraoVoznju.Add(r);
            WriteCustomerMadeRide();
            u.Voznje.Add(r);
            korisnici.Add(u);
            WriteToXMl(ROLE.USER);
            return Ok(u);
        }

        [HttpGet]
        [Route("api/ahome/blokiranjga")]
        public IHttpActionResult BlokiranjeKorisnika([FromUri]string nesto) {

            if (nesto == "" || nesto == null) {
                return Ok("Ne mozete blokirati ako nema koga blokirati");
            }

            User u;
            Driver d;

            u = korisnici.Find(k => k.Id == nesto);
            d = vozaci.Find(m => m.Id == nesto);

            if (d == null)
            {
                //znaci da blokiramo korisnika
                neBlokirani.Remove(nesto);
                blokirani.Add(nesto,u.UserName);
                korisnici.Remove(u);
                u.Blokiran = true;
                korisnici.Add(u);
                WriteToXMl(ROLE.USER);
                return Ok(u);
            }
            else {
                neBlokirani.Remove(nesto);
                /*if (blokirani.Keys.Contains(nesto)) {
                    blokirani.Remove(nesto);
                }*/
                blokirani.Add(nesto, d.UserName);
                vozaci.Remove(d);
                d.Blokiran = true;
                vozaci.Add(d);
                WriteToXMl(ROLE.DRIVER);
                return Ok(d);
            }
        }

        [HttpGet]
        [Route("api/ahome/odblokirajga")]
        public IHttpActionResult OdBlokiranjeKorisnika([FromUri]string nesto) {
            if (nesto == "" || nesto == null)
            {
                return Ok("Pokusali ste da sprovedete nemogucu akciju. Ne mozete odblokirati ako nema osoba za odblokiranje");
            }
            User u;
            Driver d;

            u = korisnici.Find(k => k.Id == nesto);
            d = vozaci.Find(m => m.Id == nesto);

            if (d == null)
            {
                blokirani.Remove(nesto);
                neBlokirani.Add(nesto, u.UserName);
                korisnici.Remove(u);
                u.Blokiran = false;
                korisnici.Add(u);
                WriteToXMl(ROLE.USER);
                return Ok(u);
            }
            else
            {
                blokirani.Remove(nesto);
                neBlokirani.Add(nesto, d.UserName);
                vozaci.Remove(d);
                d.Blokiran = false;
                vozaci.Add(d);
                WriteToXMl(ROLE.DRIVER);
                return Ok(d);
            }

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
            neBlokirani.Add(u.Id, u.UserName);

            WriteToXMl(ROLE.USER);

            return Ok(u);
        }

        [Route("api/ahome/kreiranjevoznje")]
        public IHttpActionResult KreiranjeVoznje(Ride r)
        {
            if (r.LokacijaPolazna == null) {
                return Ok("Niste odabrali lokaciju");
            }

            // FORMIRANA
            r.StatusVoznje = RideStatus.FORMIRANA;
            User dispecer;
            dispecer = admin.Find(k=> k.Id == r.Dispatcher);
            //
            admin.Remove(dispecer);
            //
            r.Dispatcher = dispecer.UserName;

            Driver d = vozaci.Find(k => k.Id == r.Vozac); // PROVERI DA LI OVDE TREBA vozaci.Find(k => k.username == r.vozac);
            r.VozacId = r.Vozac;
            r.Vozac = d.UserName;
            r.Id = (sveVoznje.Count + korisnikKreiraoVoznju.Count).ToString();
            r.DatumIVremePorudzbine = DateTime.Now;
            r.DATUMM = (r.DatumIVremePorudzbine).ToString("dd/MM/yy  H:mm:ss");
            //dispecer.Voznje.Add(r);
            if (r.TipVozila != d.Automobil.TipAutomobila)
            {
                admin.Add(dispecer);
                return Ok("Odabrani vozač ne poseduje izabrani tip automobila");
            }
            vozaci.Remove(d);

           // r.Id = r.Vozac +"_"+d.Voznje.Count;


            d.Voznje.Add(r);
            dispecer.Voznje.Add(r);

            admin.Add(dispecer);
            vozaci.Add(d);

            sveVoznje.Add(r);
            WriteToXMl(ROLE.ADMIN);
            WriteAllRides();

            return Ok(dispecer);
        }

        [HttpGet]
        [Route("api/ahome/korisnikponistavavoznju")]//PROVERI DA LI IMA KOD ADMINA TE VOZNJE
        public IHttpActionResult KorisnikPonistavaVoznju([FromUri]string nesto) {
            //PROVERI DA LI IMA KOD ADMINA TE VOZNJE

            Ride r = sveVoznje.Find(rr => rr.Id == nesto);
            Ride r1 = korisnikKreiraoVoznju.Find(rr=> rr.Id == nesto);

            User utoreturn = new User();

            if (r != null)
            {
                sveVoznje.Remove(r);
                r.StatusVoznje = RideStatus.OTKAZANA;
                string IdKorisnika = r.MusterijaId;
                sveVoznje.Add(r);

                User u = korisnici.Find(uu=> uu.Id == IdKorisnika);
                if (u != null)
                {
                    

                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == nesto);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        s.StatusVoznje = RideStatus.OTKAZANA;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
            }

            if (r1 != null)
            {
                korisnikKreiraoVoznju.Remove(r1);
                r1.StatusVoznje = RideStatus.OTKAZANA;
                string IdKorisnika = r1.MusterijaId;
                korisnikKreiraoVoznju.Add(r1);

                User u = korisnici.Find(uu => uu.Id == IdKorisnika);
                if (u != null)
                {

                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == nesto);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        s.StatusVoznje = RideStatus.OTKAZANA;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
            }

            WriteToXMl(ROLE.USER);
            WriteCustomerMadeRide();

            return Ok(utoreturn);
        }

        [HttpGet]
        [Route("api/ahome/korisnikpostavljakomentarnaponistenu")]//PROVERI DA LI IMA KOD ADMINA TE VOZNJE
        public IHttpActionResult KorisnikKomentarisePoinistenu([FromUri]string idVoznje, [FromUri] string komentar,[FromUri]int ocena)
        {
            //PROVERI DA LI IMA KOD ADMINA TE VOZNJE

            string []nekiBroj = idVoznje.Split('/');

            idVoznje = nekiBroj[0];

            Ride r = sveVoznje.Find(rr => rr.Id == idVoznje);
            Ride r1 = korisnikKreiraoVoznju.Find(rr => rr.Id == idVoznje);

            User utoreturn = new User();

            if (r != null)
            {
                sveVoznje.Remove(r);
                string IdKorisnika = r.MusterijaId;
                r.Komentar = new Comment();
                r.Komentar.DatumObjave = DateTime.Now;
                r.Komentar.Objavljeno = (r.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                r.Komentar.IdVoznje = idVoznje;
                r.Komentar.Opis = komentar;
                r.Komentar.Ocena = ocena;
                r.Komentar.KorisnikKojiJeOstavioKomentar = "";

                User u = korisnici.Find(uu => uu.Id == IdKorisnika);
                if (u != null)
                {
                    r.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == idVoznje);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        s.Komentar = new Comment();
                        s.Komentar.Opis = komentar;
                        s.Komentar.IdVoznje = idVoznje;
                        s.Komentar.DatumObjave = DateTime.Now;
                        s.Komentar.Objavljeno = (s.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                        s.Komentar.Ocena = ocena;
                        s.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
                sveVoznje.Add(r);
            }

            if (r1 != null)
            {
                sveVoznje.Remove(r1);
                string IdKorisnika = r1.MusterijaId;
                r1.Komentar = new Comment();
                r1.Komentar.DatumObjave = DateTime.Now;
                r1.Komentar.Objavljeno = (r1.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                r1.Komentar.IdVoznje = idVoznje;
                r1.Komentar.Opis = komentar;
                r1.Komentar.Ocena = ocena;

                User u = korisnici.Find(uu => uu.Id == IdKorisnika);
                if (u != null)
                {
                    r1.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == idVoznje);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        //s.Komentar = komentar;
                        s.Komentar = new Comment();
                        s.Komentar.Opis = komentar;
                        s.Komentar.IdVoznje = idVoznje;
                        s.Komentar.DatumObjave = DateTime.Now;
                        s.Komentar.Objavljeno = (s.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                        s.Komentar.Ocena = ocena;
                        s.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
                korisnikKreiraoVoznju.Add(r1);
            }

            WriteToXMl(ROLE.USER);
            WriteCustomerMadeRide();

            return Ok(utoreturn);
        }

        [HttpGet]
        [Route("api/ahome/korisnikgledavoznju")]
        public IHttpActionResult KorisnikGledaVoznju([FromUri]string idVoznje)
        {

            string[] nekiBroj = idVoznje.Split('/');
            string name = nekiBroj[0];
            //Vratimo celog korisnika sa sve voznjom da bi imali sta da stavimo u session storage
            if (idVoznje == "" || idVoznje == null) { return Ok("Nepostojeci id"); }
            User u = new User();
            Driver d = new Driver();

            foreach (User uu in korisnici)
            {
                if (uu.Voznje.Exists(v=> v.Id == idVoznje)) {
                    u = uu;
                    break;
                }
            }

            if (u.UserName == null) {
                foreach (User uu in admin)
                {
                    if (uu.Voznje.Exists(v => v.Id == idVoznje))
                    {
                        u = uu;
                        break;
                    }
                }
            }

            if (u.UserName == null)
            {
                foreach (Driver uu in vozaci)
                {
                    if (uu.Voznje.Exists(v => v.Id == idVoznje))
                    {
                        d = uu;
                        return Ok(d);
                        //break;
                    }

                }
            }

            return Ok(u);
        }

        [HttpGet]
        [Route("api/ahome/korisnikmenjakomentar")] // PROVERI KAO I KOD SVIH OSTALIH DA LI IMA I KOD ADMINA U LISTI 
        public IHttpActionResult KorisnikMenjaKomentar([FromUri]string idVoznje, [FromUri] string komentar, [FromUri]int ocena) {
            string[] nekiBroj = idVoznje.Split('/');

            idVoznje = nekiBroj[0];

            Ride r = sveVoznje.Find(rr => rr.Id == idVoznje);
            Ride r1 = korisnikKreiraoVoznju.Find(rr => rr.Id == idVoznje);

            User utoreturn = new User();

            if (r != null)
            {
                sveVoznje.Remove(r);
                string IdKorisnika = r.MusterijaId;
                r.Komentar = new Comment();
                r.Komentar.DatumObjave = DateTime.Now;
                r.Komentar.Objavljeno = (r.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                r.Komentar.IdVoznje = idVoznje;
                r.Komentar.Opis = komentar;
                r.Komentar.Ocena = ocena;
                r.Komentar.KorisnikKojiJeOstavioKomentar = "";

                User u = korisnici.Find(uu => uu.Id == IdKorisnika);
                if (u != null)
                {
                    r.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == idVoznje);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        s.Komentar = new Comment();
                        s.Komentar.Opis = komentar;
                        s.Komentar.IdVoznje = idVoznje;
                        s.Komentar.DatumObjave = DateTime.Now;
                        s.Komentar.Objavljeno = (s.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                        s.Komentar.Ocena = ocena;
                        s.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
                sveVoznje.Add(r);
            }

            if (r1 != null)
            {
                sveVoznje.Remove(r1);
                string IdKorisnika = r1.MusterijaId;
                r1.Komentar = new Comment();
                r1.Komentar.DatumObjave = DateTime.Now;
                r1.Komentar.Objavljeno = (r1.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                r1.Komentar.IdVoznje = idVoznje;
                r1.Komentar.Opis = komentar;
                r1.Komentar.Ocena = ocena;

                User u = korisnici.Find(uu => uu.Id == IdKorisnika);
                if (u != null)
                {
                    r1.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                    korisnici.Remove(u);
                    Ride s = u.Voznje.Find(k => k.Id == idVoznje);
                    if (s != null)
                    {
                        u.Voznje.Remove(s);
                        //s.Komentar = komentar;
                        s.Komentar = new Comment();
                        s.Komentar.Opis = komentar;
                        s.Komentar.IdVoznje = idVoznje;
                        s.Komentar.DatumObjave = DateTime.Now;
                        s.Komentar.Objavljeno = (s.Komentar.DatumObjave).ToString("dd/MM/yy  H:mm:ss");
                        s.Komentar.Ocena = ocena;
                        s.Komentar.KorisnikKojiJeOstavioKomentar = u.UserName;
                        u.Voznje.Add(s);
                    }
                    korisnici.Add(u);
                    utoreturn = u;
                }
                korisnikKreiraoVoznju.Add(r1);
            }

            WriteToXMl(ROLE.USER);
            WriteCustomerMadeRide();

            return Ok(utoreturn);
        }

        [Route("api/ahome/korisnikmenjapocetnulok")]
        public IHttpActionResult KorisnikMenjaPocetnuLok(Ride r) {
            if (r.LokacijaPolazna == null)
            {
                return Ok("");
            }
            r.StatusVoznje = RideStatus.KREIRANA;
            // NADJI TOG KORISNIKA U LISTI KORISNIKA I SMESTI VOZNJU U NJEGOVOJ LISTI VOZNI
            User u = korisnici.Find(uu => uu.Id == r.MusterijaId);

            Ride rr = u.Voznje.Find(l=> l.Id == r.Id);

            if (rr != null) {
                u.Voznje.Remove(rr);

                r.Dispatcher = rr.Dispatcher;
                r.Iznos = rr.Iznos;
                r.StatusVoznje = rr.StatusVoznje;
                r.Vozac = rr.Vozac;

                r.Musterija = u.UserName;
                r.MusterijaId = u.Id;
                //r.Musterija = u.UserName; // Musterija je zapravo id musterije
                korisnici.Remove(u);
                r.Id = rr.Id;
                r.DatumIVremePorudzbine = DateTime.UtcNow;
                r.DATUMM = (r.DatumIVremePorudzbine).ToString("dd/MM/yy  H:mm:ss");
                korisnikKreiraoVoznju.Add(r);

                if (korisnikKreiraoVoznju.Exists(o => o.Id == r.Id))
                {
                    Ride m = korisnikKreiraoVoznju.Find(o => o.Id == r.Id);
                    korisnikKreiraoVoznju.Remove(m);
                    korisnikKreiraoVoznju.Add(r);
                }

                WriteCustomerMadeRide();
                u.Voznje.Add(r);
                korisnici.Add(u);
                WriteToXMl(ROLE.USER);
                return Ok(u);
            }
            return Ok("");
        }

        [HttpGet]
        [Route("api/ahome/najblizi2")]
        public IHttpActionResult Najblizi2([FromUri]string idVoznje)
        {
            List<Driver> slobodniVozaci = new List<Driver>();
            Dictionary<string, double> udaljenosti = new Dictionary<string, double>();

            Ride mm = sveVoznje.Find(l =>l.Id == idVoznje);
            if (mm == null) {
                mm = korisnikKreiraoVoznju.Find(l => l.Id == idVoznje);
            }

            Double x = Convert.ToDouble(mm.LokacijaPolazna.Xcoordinate);
            Double y = Convert.ToDouble(mm.LokacijaPolazna.Ycoordinate);

            foreach (Driver d in vozaci)
            {
                if (d.Blokiran == false)
                {
                    if (d.Slobodan == true) {
                        if (d.Automobil.TipAutomobila == mm.TipVozila)
                        {
                            slobodniVozaci.Add(d);
                        }
                    }
                }
            }

            foreach (Driver d in slobodniVozaci)
            {
                Double xx = Convert.ToDouble(d.Lokacija.Xcoordinate);
                Double yy = Convert.ToDouble(d.Lokacija.Ycoordinate);
                Double r = Math.Sqrt(Math.Pow((xx - x), 2) + Math.Pow((yy - y), 2));
                udaljenosti.Add(d.Id, r);
            }

            udaljenosti = udaljenosti.OrderBy(o => o.Value).ToDictionary(k => k.Key, k => k.Value);

            Dictionary<string, string> povratni = new Dictionary<string, string>();
            int counter = 0;

            foreach (string i in udaljenosti.Keys)
            {
                Driver d = slobodniVozaci.Find(D => D.Id == i);
                povratni.Add(d.Id, d.UserName);
                counter++;
                if (counter == 5) break;
            }

            return Ok(povratni);
        }

        [HttpGet]
        [Route("api/ahome/adminOpDodeli")]
        public IHttpActionResult AdminOpDodeli([FromUri]string idVoznje,[FromUri] string vozac,[FromUri] string dispId)
        {
            string[] pom = idVoznje.Split('/');
            idVoznje = pom[0];

            User adm = admin.Find(kk=>kk.Id == dispId);

            Driver d = vozaci.Find(dd=>dd.Id == vozac);
            Ride r = sveVoznje.Find(rr => rr.Id == idVoznje);
            Ride ar = r;
            if (r == null)
            {
                //
                if (r.VozacId != "") {
                    Driver ddd = vozaci.Find(ff=>ff.Id == r.VozacId);
                    if (ddd != null) { vozaci.Remove(ddd); Ride rrr = ddd.Voznje.Find(rr => rr.Id == idVoznje); ddd.Voznje.Remove(rrr); vozaci.Add(ddd); }
                }

                //
                vozaci.Remove(d);
                r = korisnikKreiraoVoznju.Find(rr => rr.Id == idVoznje);
                korisnikKreiraoVoznju.Remove(r);
                r.Vozac = "";
                r.VozacId = "";
                r.Vozac = d.UserName;

                r.VozacId = d.Id;
                r.StatusVoznje = RideStatus.OBRADJENA;
                r.DispatcherId = dispId;
                r.Dispatcher = adm.UserName;
                //Nadji kod admina tu voznju i kod njega izmenj
                User ad = new User();
                ad.UserName = "";
                foreach (User koris in admin)
                {
                    if (koris.Voznje.Exists(v => v.Id == idVoznje))
                    {
                        ad = koris;
                        break;
                    }
                }
                if (ad.UserName != "")
                {
                    admin.Remove(ad);
                    Ride del = ad.Voznje.Find(ff => ff.Id == idVoznje);
                    if (adm.Voznje.Contains(del)) { adm.Voznje.Remove(del); }
                    admin.Remove(adm);
                    ad.Voznje.Remove(del);
                    r.Dispatcher = adm.UserName;
                    ad.Voznje.Add(r);
                    adm.Voznje.Add(r);
                    admin.Add(ad);
                    admin.Add(adm);
                    WriteToXMl(ROLE.ADMIN);
                }
                else {
                    ad = adm;
                    admin.Remove(ad);
                    // NADJI bas tu voznju i unisti je
                    Ride del = ad.Voznje.Find(ff => ff.Id == idVoznje);
                    ad.Voznje.Remove(del);
                    //ad.Voznje.Remove(ar);
                    r.Dispatcher = ad.UserName;
                    ad.Voznje.Add(r);
                    admin.Add(adm);
                    WriteToXMl(ROLE.ADMIN);
                }
                //
                d.Voznje.Add(r);
                korisnikKreiraoVoznju.Add(r);
                vozaci.Add(d);          // ispisi i u xml-ovima 
                WriteCustomerMadeRide();
                WriteToXMl(ROLE.DRIVER);
                return Ok(adm); // vrati izmenjenog admina i stavi ga u sesiju 
            }
            else {

                if (r.VozacId != "")
                {
                    Driver ddd = vozaci.Find(ff => ff.Id == r.VozacId);
                    if (ddd != null) { vozaci.Remove(ddd); Ride rrr = ddd.Voznje.Find(rr => rr.Id == idVoznje); ddd.Voznje.Remove(rrr); vozaci.Add(ddd); }
                }

                sveVoznje.Remove(r);
                vozaci.Remove(d);
                r.Vozac = "";
                r.VozacId = "";
                r.Vozac = d.UserName;
                r.VozacId = d.Id;
                r.StatusVoznje = RideStatus.OBRADJENA;

                //
                User ad = new User();
                ad.UserName = "";
                foreach (User koris in admin)
                {
                    if (koris.Voznje.Exists(v => v.Id == idVoznje))
                    {
                        ad = koris;
                        break;
                    }
                }
                if (ad.UserName != "")
                {
                    admin.Remove(ad);
                    // NADJI bas tu voznju i unisti je
                    Ride del = ad.Voznje.Find(ff=>ff.Id==idVoznje);
                    ad.Voznje.Remove(del);
                    //ad.Voznje.Remove(ar);
                    r.Dispatcher = adm.UserName;
                    ad.Voznje.Add(r);
                    admin.Add(ad);
                    WriteToXMl(ROLE.ADMIN);
                }
                if (ad == null)
                {
                    ad = adm;
                    admin.Remove(ad);
                    admin.Remove(adm);
                    Ride del = ad.Voznje.Find(ff => ff.Id == idVoznje);
                    if (adm.Voznje.Contains(del)) { adm.Voznje.Remove(del); }
                    
                    ad.Voznje.Remove(del);
                    r.Dispatcher = ad.UserName;
                    ad.Voznje.Add(r);
                    admin.Add(ad);
                    
                    adm.Voznje.Add(r);
                    admin.Add(adm);
                    WriteToXMl(ROLE.ADMIN);
                }

                d.Voznje.Add(r);
                vozaci.Add(d);
                sveVoznje.Add(r);
                WriteAllRides();
                WriteToXMl(ROLE.DRIVER);
                //
                return Ok(adm);
            }

        }

        [HttpGet]
        [Route("api/ahome/vozacMenjaUuPrihvacena")]
        public IHttpActionResult VozacPrihvacena([FromUri]string idVoznje)
        {// promeni kod korisnika admina vozaca 
            User u = new Models.User();
            User dispec = new User();
            Driver vozac = new Driver();

            Ride r = sveVoznje.Find(f=>f.Id == idVoznje);
            if (r != null)
            {
                sveVoznje.Remove(r);
                r.StatusVoznje = RideStatus.PRIHVACENA;
                sveVoznje.Add(r);
            }

            r = korisnikKreiraoVoznju.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                korisnikKreiraoVoznju.Remove(r);
                r.StatusVoznje = RideStatus.PRIHVACENA;
                korisnikKreiraoVoznju.Add(r);
            }

            u = korisnici.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (u!=null)
            {
                korisnici.Remove(u);
                r = u.Voznje.Find(p => p.Id == idVoznje);
                u.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.PRIHVACENA;
                u.Voznje.Add(r);
                korisnici.Add(u);
            }
            // Nadjemo dispecera
            dispec = admin.Find(p=>p.Voznje.Exists(pp=>pp.Id == idVoznje));
            if (dispec!=null)
            {
                admin.Remove(dispec);
                r = dispec.Voznje.Find(p => p.Id == idVoznje);
                dispec.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.PRIHVACENA;
                dispec.Voznje.Add(r);
                admin.Add(dispec);
            }

            //nadjemo vozaca 
            vozac = vozaci.Find(g=>g.Voznje.Exists(gg=>gg.Id == idVoznje));
            if(vozac!=null)
            {
                vozaci.Remove(vozac);
                r = vozac.Voznje.Find(p => p.Id == idVoznje);
                vozac.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.PRIHVACENA;
                vozac.Voznje.Add(r);
                vozaci.Add(vozac);
            }

            // ISPISEMO U XML promene
            WriteToXMl(ROLE.ADMIN);
            WriteToXMl(ROLE.DRIVER);
            WriteToXMl(ROLE.USER);
            WriteAllRides();
            WriteCustomerMadeRide();

            return Ok(vozac);
        }

        [HttpGet]
        [Route("api/ahome/vozacMenjaUuToku")]
        public IHttpActionResult VozacUtoku([FromUri]string idVoznje)
        {
            User u = new Models.User();
            User dispec = new User();
            Driver vozac = new Driver();
            Ride r = new Ride();

            vozac = vozaci.Find(g => g.Voznje.Exists(gg => gg.Id == idVoznje));
            if (vozac != null)
            {
                if (vozac.Slobodan == false) { return Ok("Vozač trenutno ne može da prihvati vožnju, jer nije završio prethodnu"); }

                vozaci.Remove(vozac);
                r = vozac.Voznje.Find(p => p.Id == idVoznje);
                vozac.Slobodan = false;
                vozac.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                vozac.Voznje.Add(r);
                vozaci.Add(vozac);
            }

            r = sveVoznje.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                sveVoznje.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                sveVoznje.Add(r);
            }

            r = korisnikKreiraoVoznju.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                korisnikKreiraoVoznju.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                korisnikKreiraoVoznju.Add(r);
            }

            u = korisnici.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (u!=null)
            {
                korisnici.Remove(u);
                r = u.Voznje.Find(p => p.Id == idVoznje);
                u.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                u.Voznje.Add(r);
                korisnici.Add(u);
            }

            // Nadjemo dispecera
            dispec = admin.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (dispec!=null)
            {
                admin.Remove(dispec);
                r = dispec.Voznje.Find(p => p.Id == idVoznje);
                dispec.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                dispec.Voznje.Add(r);
                admin.Add(dispec);
            }

            //nadjemo vozaca 
            vozac = vozaci.Find(g => g.Voznje.Exists(gg => gg.Id == idVoznje));
            if (vozac != null)
            {
                vozaci.Remove(vozac);
                r = vozac.Voznje.Find(p => p.Id == idVoznje);
                vozac.Voznje.Remove(r);
                r.StatusVoznje = RideStatus.U_TOKU;
                vozac.Slobodan = false;
                vozac.Voznje.Add(r);
                vozaci.Add(vozac);
            }

            // ISPISEMO U XML promene
            WriteToXMl(ROLE.ADMIN);
            WriteToXMl(ROLE.DRIVER);
            WriteToXMl(ROLE.USER);
            WriteAllRides();
            WriteCustomerMadeRide();

            return Ok(vozac);
        }

        [HttpGet]
        [Route("api/ahome/vozacMenjaUspesnaNeuspesna")]
        public IHttpActionResult VozacUspesnaNeuspesna([FromUri]string s)
        {
            string []niz = s.Split('*');
            string tip = niz[0];
            string idVoznje = niz[1];
            idVoznje = idVoznje.Trim(' ');
            Ride pom = new Ride();

            if (tip == "USPEH") { pom.StatusVoznje = RideStatus.USPESNA; }
            else { pom.StatusVoznje = RideStatus.NEUSPESNA; }

            //
            User u = new Models.User();
            User dispec = new User();
            Driver vozac = new Driver();

            Ride r = sveVoznje.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                sveVoznje.Remove(r);
                r.StatusVoznje = pom.StatusVoznje;
                sveVoznje.Add(r);
            }

            r = korisnikKreiraoVoznju.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                korisnikKreiraoVoznju.Remove(r);
                r.StatusVoznje = pom.StatusVoznje;
                korisnikKreiraoVoznju.Add(r);
            }
            vozac = vozaci.Find(g => g.Voznje.Exists(gg => gg.Id == idVoznje));
            if (vozac != null)
            {
                vozaci.Remove(vozac);
                r = vozac.Voznje.Find(p => p.Id == idVoznje);
                vozac.Voznje.Remove(r);
                r.StatusVoznje = pom.StatusVoznje;
                vozac.Slobodan = true;
                vozac.Voznje.Add(r);
                vozaci.Add(vozac);
            }


            u = korisnici.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (u!=null)
            {
                korisnici.Remove(u);
                r = u.Voznje.Find(p => p.Id == idVoznje);
                u.Voznje.Remove(r);
                r.StatusVoznje = pom.StatusVoznje;
                u.Voznje.Add(r);
                korisnici.Add(u);
            }
            // Nadjemo dispecera
            dispec = admin.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (dispec!=null)
            {
                admin.Remove(dispec);
                r = dispec.Voznje.Find(p => p.Id == idVoznje);
                dispec.Voznje.Remove(r);
                r.StatusVoznje = pom.StatusVoznje;
                dispec.Voznje.Add(r);
                admin.Add(dispec);
            }

            //nadjemo vozaca 

            // ISPISEMO U XML promene
            WriteToXMl(ROLE.ADMIN);
            WriteToXMl(ROLE.DRIVER);
            WriteToXMl(ROLE.USER);
            WriteAllRides();
            WriteCustomerMadeRide();

            return Ok(vozac);
            //

        }

        [HttpGet]
        [Route("api/ahome/vratiSve")]
        public IHttpActionResult vratiSve()
        {
            List<Ride> celokupno = new List<Ride>();
            foreach (Ride r in sveVoznje)
            {
                celokupno.Add(r);
            }

            foreach (Ride r in korisnikKreiraoVoznju)
            {
                if (!celokupno.Contains(r))
                {
                    celokupno.Add(r);
                }
            }

            return Ok(celokupno);
        }

        [Route("api/ahome/statusSort")]
        public IHttpActionResult statusSort(VoznjeObj voznje) {
            List<Ride> voznjeRet = new List<Ride>();

            string ss = voznje.Status.ToString();
            char s = ss[0];

            if (voznje.Status >= 0)
            {
                //{KREIRANA,FORMIRANA,OBRADJENA,PRIHVACENA,OTKAZANA,NEUSPESNA,USPESNA,U_TOKU}
                RideStatus stat = RideStatus.KREIRANA;
                switch (s) {
                    case '0':
                        stat = RideStatus.KREIRANA;
                        break;
                    case '1':
                        stat = RideStatus.FORMIRANA;
                        break;
                    case '2':
                        stat = RideStatus.OBRADJENA;
                        break;
                    case '3':
                        stat = RideStatus.PRIHVACENA;
                        break;
                    case '4':
                        stat = RideStatus.OTKAZANA;
                        break;
                    case '5':
                        stat = RideStatus.NEUSPESNA;
                        break;
                    case '6':
                        stat = RideStatus.USPESNA;
                        break;
                    case '7':
                        stat = RideStatus.U_TOKU;
                        break;
                }

                foreach (Ride r in voznje.PoslateVoznje)
                {
                    if (r.StatusVoznje == stat) {
                        voznjeRet.Add(r);
                    }
                }
            }

            return Ok(voznjeRet);
        }

        [Route("api/ahome/vozacOdredisteCena")]
        public IHttpActionResult VozacOdredisteCena(Ride voznja)
        {
            if (voznja.Iznos == 0) { return Ok("Niste uneli validan iznos za cenu"); }

            voznja.Id = voznja.Id.Trim(' ');

            //
            User u = new Models.User();
            User dispec = new User();
            Driver vozac = new Driver();
            Ride r = new Ride();
            string idVoznje = voznja.Id;

            vozac = vozaci.Find(g => g.Voznje.Exists(gg => gg.Id == idVoznje));
            if (vozac != null)
            {

                vozaci.Remove(vozac);
                r = vozac.Voznje.Find(p => p.Id == idVoznje);
                vozac.Voznje.Remove(r);
                r.Odrediste = voznja.Odrediste;
                r.Iznos = voznja.Iznos;
                vozac.Voznje.Add(r);
                vozaci.Add(vozac);
            }

            r = sveVoznje.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                sveVoznje.Remove(r);
                r.Odrediste = voznja.Odrediste;
                r.Iznos = voznja.Iznos;
                sveVoznje.Add(r);
            }

            r = korisnikKreiraoVoznju.Find(f => f.Id == idVoznje);
            if (r != null)
            {
                korisnikKreiraoVoznju.Remove(r);
                r.Odrediste = voznja.Odrediste;
                r.Iznos = voznja.Iznos;
                korisnikKreiraoVoznju.Add(r);
            }

            u = korisnici.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (u != null)
            {
                korisnici.Remove(u);
                r = u.Voznje.Find(p => p.Id == idVoznje);
                u.Voznje.Remove(r);
                r.Odrediste = voznja.Odrediste;
                r.Iznos = voznja.Iznos;
                u.Voznje.Add(r);
                korisnici.Add(u);
            }

            // Nadjemo dispecera
            dispec = admin.Find(p => p.Voznje.Exists(pp => pp.Id == idVoznje));
            if (dispec != null)
            {
                admin.Remove(dispec);
                r = dispec.Voznje.Find(p => p.Id == idVoznje);
                dispec.Voznje.Remove(r);
                r.Odrediste = voznja.Odrediste;
                r.Iznos = voznja.Iznos;
                dispec.Voznje.Add(r);
                admin.Add(dispec);
            }

            // ISPISEMO U XML promene
            WriteToXMl(ROLE.ADMIN);
            WriteToXMl(ROLE.DRIVER);
            WriteToXMl(ROLE.USER);
            WriteAllRides();
            WriteCustomerMadeRide();

            return Ok(vozac);
            //
        }

        [Route("api/ahome/datesort")]
        public IHttpActionResult DateSort(VoznjeObj voznje) {
            List<Ride> voznje1 = voznje.PoslateVoznje;

            voznje1 = voznje1.OrderBy(X => X.DatumIVremePorudzbine).ToList();
            
            return Ok(voznje1);
        }
   
        [Route("api/ahome/gradeSort")]
        public IHttpActionResult GradeSort(VoznjeObj voznje)
        {
            List<Ride> voznje1 = voznje.PoslateVoznje;

            voznje1 = voznje1.OrderBy(X => X.Komentar.Ocena).ToList();

            return Ok(voznje1);
        }

        [Route("api/ahome/voznjeNaCekanju")]
        public IHttpActionResult voznjeNaCekanju(VoznjeObj voznje) {
            List<Ride> ret = new List<Ride>();

            foreach (Ride r in korisnikKreiraoVoznju)
            {
                if (r.StatusVoznje == RideStatus.KREIRANA)
                {
                    ret.Add(r);                }
            }

            foreach (Ride r in sveVoznje) {
                if (r.StatusVoznje == RideStatus.KREIRANA)
                {if(!ret.Contains(r))
                    ret.Add(r);
                }
            }

            return Ok(ret);
        }

        [Route("api/ahome/lengthSort")]
        public IHttpActionResult lengthSort(UdaljenostiObj objekat) {
            List<Ride> povratni = new List<Ride>();
            Dictionary<string, double> udaljenosti = new Dictionary<string, double>();
            Double x = Convert.ToDouble(objekat.TrentutnaLokacijaVozaca.Xcoordinate);
            Double y = Convert.ToDouble(objekat.TrentutnaLokacijaVozaca.Ycoordinate);

            foreach (Ride d in objekat.PoslateVoznje)
            {
                Double xx = Convert.ToDouble(d.LokacijaPolazna.Xcoordinate);
                Double yy = Convert.ToDouble(d.LokacijaPolazna.Ycoordinate);
                // Double r = Math.Sqrt(Math.Pow((xx - yy), 2) + Math.Pow((x - y), 2));
                Double r = Math.Sqrt(Math.Pow((xx - x), 2) + Math.Pow((yy - y), 2));
                udaljenosti.Add(d.Id, r);
            }

            udaljenosti = udaljenosti.OrderBy(o => o.Value).ToDictionary(k => k.Key, k => k.Value);

            foreach (string id in udaljenosti.Keys)
            {
                Ride p = objekat.PoslateVoznje.Find(o=>o.Id == id);
                povratni.Add(p);
            }

            //Dictionary<string, string> povratni = new Dictionary<string, string>();

            return Ok(povratni);
        }

        //AdminPretraga
        [Route("api/ahome/AdminPretraga")]
        public IHttpActionResult AdminPretraga(AdminSearchObj objekat)
        {
            List<Ride> pretrazeno = new List<Ride>();
            List<Ride> result1 = new List<Ride>();
            List<Ride> result2 = new List<Ride>();
            List<Ride> result3 = new List<Ride>();
            List<Ride> result4 = new List<Ride>();
            List<Ride> result5 = new List<Ride>();
            DateTime vremeOd = DateTime.Now;
            DateTime vremeDo = DateTime.Now;


            if (objekat.OdVreme != null)
            {
                vremeOd = DateTime.ParseExact(objekat.OdVreme, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (objekat.DoVreme != null)
            { vremeDo = DateTime.ParseExact(objekat.DoVreme, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture); }

            if (objekat.DoVreme != null && objekat.OdVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help = DateTime.Compare(r.DatumIVremePorudzbine, vremeOd);
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeDo);
                    if (help >= 0 && help1 <= 0) { pretrazeno.Add(r); }
                }
            }
            else if (objekat.DoVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeDo);
                    if (help1 <= 0)
                    {
                        pretrazeno.Add(r);
                    }
                }
            }
            else if (objekat.OdVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeOd);
                    if (help1 >= 0)
                    {
                        pretrazeno.Add(r);
                    }
                }
            }
            else
            {
                pretrazeno = objekat.voznje.PoslateVoznje;
            }

            if (objekat.DoCena != null)
            {

                int doCena = Int32.Parse(objekat.DoCena);
                if (doCena < 0) { return Ok("Cena ne sme biti negativna"); }

                foreach (Ride r in pretrazeno)
                {
                    if (r.Iznos <= doCena)
                    {
                        result1.Add(r);
                    }
                }
                pretrazeno = result1;
            }

            if (objekat.OdCena != null)
            {
                int odCena = Int32.Parse(objekat.OdCena);
                if (odCena < 0) { return Ok("Cena ne sme biti negativna"); }

                foreach (Ride r in pretrazeno)
                {
                    if (r.Iznos >= odCena)
                    {
                        result2.Add(r);
                    }
                }
                pretrazeno = result2;
            }

            if (objekat.DoOcena != null && objekat.DoOcena != "-1")
            {
                int doOcena = Int32.Parse(objekat.DoOcena);
                if (doOcena >= 0)
                {
                    foreach (Ride r in pretrazeno)
                    {
                        if (r.Komentar.Ocena <= doOcena)
                        {
                            result3.Add(r);
                        }
                    }
                }
                pretrazeno = result3;
            }

            if (objekat.OdOcena != null && objekat.OdOcena != "-1")
            {

                int odOcena = Int32.Parse(objekat.OdOcena);
                if (odOcena >= 0)
                {
                    foreach (Ride r in pretrazeno)
                    {
                        if (r.Komentar.Ocena >= odOcena)
                        {
                            result4.Add(r);
                        }
                    }
                }
                pretrazeno = result4;
            }

            RideStatus stat = RideStatus.FORMIRANA;
            if (objekat.StatusVoznje >= 0)
            {
                switch (objekat.StatusVoznje)
                {
                    case 0:
                        stat = RideStatus.KREIRANA;
                        break;
                    case 1:
                        stat = RideStatus.FORMIRANA;
                        break;
                    case 2:
                        stat = RideStatus.OBRADJENA;
                        break;
                    case 3:
                        stat = RideStatus.PRIHVACENA;
                        break;
                    case 4:
                        stat = RideStatus.OTKAZANA;
                        break;
                    case 5:
                        stat = RideStatus.NEUSPESNA;
                        break;
                    case 6:
                        stat = RideStatus.USPESNA;
                        break;
                    case 7:
                        stat = RideStatus.U_TOKU;
                        break;
                }

                foreach (Ride r in pretrazeno)
                {
                    if (r.StatusVoznje == stat)
                    {
                        result5.Add(r);
                    }
                }
                pretrazeno = result5;
            }

            // proverimo sad ime da li je uneto

            // MORA KOMPLEKSNIJA PRETRAGA
            // JER IME KOJE SE CUVA U VOZNJI ZAPRAVO PRETSTAVLJA KORISNICKO IME 
            // MORAM PO ID-U DA DOBAVIM PROSLEDJENOG KORISNIKA
            // TE DA NJEGOV ID POREDIM SA ONIM KOJI STOJI U VOZNJI 
            // ISTO VAZI I ZA VOZACA


            return Ok(pretrazeno);

        }
        [Route("api/ahome/KorisnikPretraga")]
        public IHttpActionResult KorisnikPretraga(KorisnikPretraga objekat) {

            List<Ride> pretrazeno = new List<Ride>();
            List<Ride> result1 = new List<Ride>();
            List<Ride> result2 = new List<Ride>();
            List<Ride> result3 = new List<Ride>();
            List<Ride> result4 = new List<Ride>();
            List<Ride> result5 = new List<Ride>();
            DateTime vremeOd = DateTime.Now;
            DateTime vremeDo = DateTime.Now;


            if (objekat.OdVreme != null) {
                 vremeOd = DateTime.ParseExact(objekat.OdVreme, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (objekat.DoVreme != null)
            {  vremeDo = DateTime.ParseExact(objekat.DoVreme, "yyyy-MM-ddTHH:mm", System.Globalization.CultureInfo.InvariantCulture); }

            if (objekat.DoVreme != null && objekat.OdVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help = DateTime.Compare(r.DatumIVremePorudzbine, vremeOd);
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeDo);
                    if (help >= 0 && help1 <= 0) { pretrazeno.Add(r); }
                }
            }
            else if (objekat.DoVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeDo);
                    if (help1 <= 0)
                    {
                        pretrazeno.Add(r);
                    }
                }
            }
            else if (objekat.OdVreme != null)
            {
                foreach (Ride r in objekat.voznje.PoslateVoznje)
                {
                    int help1 = DateTime.Compare(r.DatumIVremePorudzbine, vremeOd);
                    if (help1 >= 0)
                    {
                        pretrazeno.Add(r);
                    }
                }
            }
            else {
                pretrazeno = objekat.voznje.PoslateVoznje; 
            }

            if (objekat.DoCena != null)
            {

                int doCena = Int32.Parse(objekat.DoCena);
                if (doCena < 0) { return Ok("Cena ne sme biti negativna"); }

                foreach (Ride r in pretrazeno)
                {
                    if (r.Iznos <= doCena)
                    {
                        result1.Add(r);
                    }
                }
                pretrazeno = result1;
            }

            if (objekat.OdCena != null) {
                int odCena = Int32.Parse(objekat.OdCena);
                if (odCena < 0) { return Ok("Cena ne sme biti negativna"); }

                foreach (Ride r in pretrazeno)
                {
                    if (r.Iznos >= odCena)
                    {
                        result2.Add(r);
                    }
                }
                pretrazeno = result2;
            }

            if (objekat.DoOcena != null && objekat.DoOcena != "-1") {
                int doOcena = Int32.Parse(objekat.DoOcena);
                if (doOcena >= 0)
                {
                    foreach (Ride r in pretrazeno)
                    {
                        if (r.Komentar.Ocena <= doOcena)
                        {
                            result3.Add(r);
                        }
                    }
                }
                pretrazeno = result3;
            }

            if (objekat.OdOcena != null && objekat.OdOcena != "-1") {

                int odOcena = Int32.Parse(objekat.OdOcena);
                if (odOcena >= 0)
                {
                    foreach (Ride r in pretrazeno)
                    {
                        if (r.Komentar.Ocena >= odOcena)
                        {
                            result4.Add(r);
                        }
                    }
                }
                pretrazeno = result4;
            }
           
            RideStatus stat = RideStatus.FORMIRANA;
            if (objekat.StatusVoznje >= 0) {
                switch (objekat.StatusVoznje)
                {
                    case 0:
                        stat = RideStatus.KREIRANA;
                        break;
                    case 1:
                        stat = RideStatus.FORMIRANA;
                        break;
                    case 2:
                        stat = RideStatus.OBRADJENA;
                        break;
                    case 3:
                        stat = RideStatus.PRIHVACENA;
                        break;
                    case 4:
                        stat = RideStatus.OTKAZANA;
                        break;
                    case 5:
                        stat = RideStatus.NEUSPESNA;
                        break;
                    case 6:
                        stat = RideStatus.USPESNA;
                        break;
                    case 7:
                        stat = RideStatus.U_TOKU;
                        break;
                }
            
                foreach (Ride r in pretrazeno) {
                    if (r.StatusVoznje == stat) {
                        result5.Add(r);
                    }
                }
                pretrazeno = result5;
            }
            

           // foreach

            return Ok(pretrazeno);
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
            //string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\RIDES.xml";
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/RIDES.xml");

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
            //string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\" + uloga.ToString() + ".xml";
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/" + uloga.ToString() + ".xml");

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
            //string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\" + uloga.ToString() + ".xml";
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/"+ uloga.ToString()+ ".xml");
            XmlSerializer serializer;
            if (uloga == ROLE.DRIVER)
                serializer = new XmlSerializer(typeof(List<Driver>));
            else
                serializer = new XmlSerializer(typeof(List<User>));

            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    if (uloga == ROLE.ADMIN) {

                        admin = (List<User>)serializer.Deserialize(reader);
                    }  
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
            // string path = @"C:\Users\asus\Desktop\Web\Web projekat\TaksiSluzba\RIDES.xml";
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/RIDES.xml");

            XmlSerializer serializer;
                serializer = new XmlSerializer(typeof(List<Ride>));

            using (TextWriter writer = new StreamWriter(path))
            {
                    serializer.Serialize(writer, sveVoznje);
            }
        }

        private void ReadCustomerMadeRide()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/CUSTOMER_RIDES.xml");

            XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(List<Ride>));

            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    korisnikKreiraoVoznju = (List<Ride>)serializer.Deserialize(reader);
                }
            }
            catch { }
        }

        private void WriteCustomerMadeRide()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/CUSTOMER_RIDES.xml");

            XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(List<Ride>));

            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, korisnikKreiraoVoznju);
            }
        }

        private void IzmeniAdminaUVozacima(string id,string username)
        {
            List<Driver> vozaciNovi = new List<Driver>();
            List<Ride> voznjeNove = new List<Ride>();
            Driver pom = new Driver();
            foreach (Driver d in vozaci)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in d.Voznje)
                {
                    if (r.DispatcherId == id) {
                        r.Dispatcher = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = d;
                pom.Voznje = voznjeNove;
                vozaciNovi.Add(pom);
            }
            vozaci = vozaciNovi;
            WriteToXMl(ROLE.DRIVER);
        }

        private void IzmeniAdminaUKorisnicima(string id, string username) {
            List<User> korisniciNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in korisnici)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje)
                {
                    if (r.DispatcherId == id)
                    {
                        r.Dispatcher = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                korisniciNovi.Add(pom);
            }
            korisnici = korisniciNovi;
            WriteToXMl(ROLE.USER);
        }

        private void IzmeniAdminoveVoznje(string id,string username) {
            List<User> adminiNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in admin) {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje) {
                    if (r.DispatcherId == id) {
                        r.Dispatcher = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                adminiNovi.Add(pom);
            }
            admin = adminiNovi;
            WriteToXMl(ROLE.ADMIN);
        }

        private void IzmeniKorisnikKreiraoAdmina(string id,string username) {
            List<Ride> korisnikove = new List<Ride>();
            Ride r = new Ride();
            foreach (Ride rr in korisnikKreiraoVoznju) {
                if (rr.DispatcherId == id) {
                    rr.Dispatcher = username;
                } else if (rr.MusterijaId == id) {
                    rr.Musterija = username;
                } else if (rr.VozacId == id) {
                    rr.Vozac = username;
                }
                korisnikove.Add(rr);
            }

            korisnikKreiraoVoznju = korisnikove;
            WriteCustomerMadeRide();

        }

        private void IzmeniSveVoznjeAdmina(string id, string username) {
            List<Ride> NoveSve = new List<Ride>();
            Ride r = new Ride();
            foreach (Ride rr in sveVoznje)
            {
                if (rr.DispatcherId == id)
                {
                    rr.Dispatcher = username;
                }
                else if (rr.MusterijaId == id)
                {
                    rr.Musterija = username;
                }
                else if (rr.VozacId == id)
                {
                    rr.Vozac = username;
                }
                NoveSve.Add(rr);
            }
            sveVoznje = NoveSve;
            WriteAllRides();
        }

        private void IzmeniKorisnikaUKorisnicima(string id, string username) {
            List<User> korisniciNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in korisnici)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje)
                {
                    if (r.MusterijaId == id)
                    {
                        r.Musterija = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                korisniciNovi.Add(pom);
            }
            korisnici = korisniciNovi;
            WriteToXMl(ROLE.USER);
        }

        private void IzmeniKorisnikaUVozacima(string id, string username)
        {
            List<Driver> vozaciNovi = new List<Driver>();
            List<Ride> voznjeNove = new List<Ride>();
            Driver pom = new Driver();
            foreach (Driver d in vozaci)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in d.Voznje)
                {
                    if (r.MusterijaId == id)
                    {
                        r.Musterija = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = d;
                pom.Voznje = voznjeNove;
                vozaciNovi.Add(pom);
            }
            vozaci = vozaciNovi;
            WriteToXMl(ROLE.DRIVER);
        }

        private void IzmeniKorisnikaUAdminima(string id, string username) {
            List<User> adminiNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in admin)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje)
                {
                    if (r.MusterijaId == id)
                    {
                        r.Musterija = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                adminiNovi.Add(pom);
            }
            admin = adminiNovi;
            WriteToXMl(ROLE.ADMIN);
        }

        private void IzmeniVozacaUKorisnicima(string id, string username) {
            List<User> korisniciNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in korisnici)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje)
                {
                    if (r.VozacId == id)
                    {
                        r.Vozac = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                korisniciNovi.Add(pom);
            }
            korisnici = korisniciNovi;
            WriteToXMl(ROLE.USER);
        }

        private void IzmeniVozacaUVozacima(string id, string username) {
            List<Driver> vozaciNovi = new List<Driver>();
            List<Ride> voznjeNove = new List<Ride>();
            Driver pom = new Driver();
            foreach (Driver d in vozaci)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in d.Voznje)
                {
                    if (r.VozacId == id)
                    {
                        r.Vozac = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = d;
                pom.Voznje = voznjeNove;
                vozaciNovi.Add(pom);
            }
            vozaci = vozaciNovi;
            WriteToXMl(ROLE.DRIVER);
        }

        private void IzmeniVozacaUAdminima(string id, string username) {
            List<User> adminiNovi = new List<User>();
            List<Ride> voznjeNove = new List<Ride>();
            User pom = new User();
            foreach (User u in admin)
            {
                voznjeNove = new List<Ride>();
                foreach (Ride r in u.Voznje)
                {
                    if (r.VozacId == id)
                    {
                        r.Vozac = username;
                    }
                    voznjeNove.Add(r);
                }
                pom = u;
                pom.Voznje = voznjeNove;
                adminiNovi.Add(pom);
            }
            admin = adminiNovi;
            WriteToXMl(ROLE.ADMIN);
        }

    }
}
/*toSend = toSend.OrderBy(X => X.VremePorudjbine).ToList();   toSend je lista voznje  */


//svaki gumb za admina da se odradi
//function ime i šta mu je stiglo 
// function funkcija(parametarBezTipa){};


/*
 $.ajax({
                    url: "/api/main/logoutuser",
                    data: un,
                    type: "DELETE"              
                });
*/
