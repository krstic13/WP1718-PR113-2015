using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaksiSluzba.Models
{
    public enum ROLE { USER, DRIVER, ADMIN}

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string JMBG { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public ROLE Uloga { get; set; }
        public Ride Voznja { get; set; }

        public User(string username, string password, string name, string lastname, string gender, string jmbg, string phonenumber,string email, ROLE uloga, Ride voznja)
        {
            UserName = username;
            Password = password;
            Name = name;
            LastName = lastname;
            Gender = gender;
            JMBG = jmbg;
            PhoneNumber = phonenumber;
            Email = email;
            Uloga = uloga;
            Voznja = new Ride(voznja);

        }

        public User(User korisnik)
        {
            this.Email = korisnik.Email;
            this.Gender = korisnik.Gender;
            this.JMBG = korisnik.JMBG;
            this.LastName = korisnik.LastName;
            this.Name = korisnik.Name;
            this.Password = korisnik.Password;
            this.PhoneNumber = korisnik.PhoneNumber;
            this.Uloga = korisnik.Uloga;
            this.UserName = korisnik.UserName;
            this.Voznja = korisnik.Voznja;
        }

        public User()
        {
            UserName = "";
            Password = "";
            Name = "";
            LastName = "";
            Gender = "Z";
            JMBG = "";
            PhoneNumber = "";
            Email = "";
            Uloga = ROLE.USER;
            Voznja = new Ride();
        }

    }
}