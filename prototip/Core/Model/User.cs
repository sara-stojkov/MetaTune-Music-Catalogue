using Core.Utils;

namespace Core.Model
{
    public class User(string id, string personId, string name, string surname, string email, string password, string role, string status, bool? contactVisible = null, bool? reviewsVisible = null, string? verificationCode = null, List<Genre> genres = null)
    {
        private readonly string id = id;
        private readonly string personId = personId;
        private string name = name;
        private string surname = surname;
        private string email = email;
        private string password = password;
        private string role = role;
        private string status = status;
        private bool? contactVisible = contactVisible;
        private bool? reviewsVisible = reviewsVisible;
        private string? verificationCode = verificationCode;
        private List<Genre>? genre = genres;

        public User() : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "", "", "", "", UserRole.BASIC, UserStatus.WAITINGVERIFICATION) { }
        public User(string name, string surname, string email, string password)
            : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), name, surname, email, password, UserRole.BASIC, UserStatus.WAITINGVERIFICATION, true, true)
        { }
        public string Id => id;
        public string PersonId => personId;
        public string Name
        {
            get => this.name;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("User's name cannot be empty or just whitespace");
                this.name = value.Trim();
            }
        }
        public string Surname
        {
            get => this.surname;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("User's surname cannot be empty or just whitespace");
                this.surname = value.Trim();
            }
        }
       
        public string Email
        {
            get => this.email;
            set
            {
                if (!Validator.IsValidEmail(value))
                    throw new ArgumentException("User's email is not valid");
                this.email = value.Trim();
            }
        }
        public string Password
        {
            get => this.password;
            set
            {
                //if (!Validator.IsValidPassword(value))
                //    throw new ArgumentException("User's password cannot be empty or just whitespace");
                this.password = value.Trim();
            }
        }

        public bool? IsContactVisible
        {
            get => contactVisible;
            set => contactVisible = value;
        } 
        public bool? AreReviewsVisible
        {
            get => reviewsVisible;
            set => reviewsVisible = value;
        }

        public List<Genre> Genres
        {
            get => genre;
            set => genre = value;
        }

        public string Role => role;
        public string Status
        {
            get => status;
            set => status = value;
        }
        public string? VerificationCode
        {
            get => verificationCode;
            set => verificationCode = value;
        }
        public void HashPassword() { password = PasswordManager.HashPassword(password); }

    }
}
