using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model
{
    public class Person(string personId, string personName, string personSurname)
    {
        private readonly string personId = personId;
        private string personName = personName;
        private string personSurname = personSurname;

        public string PersonId => personId;
        public string PersonName
        {
            get => this.personName;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("User's name cannot be empty or just whitespace");
                this.personName = value.Trim();
            }
        }
        public string PersonSurname
        {
            get => this.personSurname;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("User's surname cannot be empty or just whitespace");
                this.personSurname = value.Trim();
            }
        }
    }
}
