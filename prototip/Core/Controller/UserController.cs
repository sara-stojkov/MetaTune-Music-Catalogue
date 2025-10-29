using Core.Model;
using Core.Services.EmailService;
using Core.Storage;
using Core.Utils;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace Core.Controller
{
    // Custom exception for user not found
    public class UserNotFoundException : ApplicationException
    {
        public UserNotFoundException(string email)
            : base($"Korisnik sa mejlom {email} ne postoji") { }
    }

    // Custom exception for invalid password
    public class InvalidPasswordException : ApplicationException
    {
        public InvalidPasswordException()
            : base("Pogrešna lozinka") { }
    }

    public class UserController(
        IUserStorage userStorage,
        IGenreStorage genreStorage,
        IEmailService emailService
        )
    {
        private readonly IUserStorage _userStorage = userStorage;
        private readonly IGenreStorage _genreStorage = genreStorage;
        private readonly IEmailService _emailService = emailService;

        public async Task<User> Login(string email, string password)
        {
            var user = await _userStorage.GetByEmail(email)
                ?? throw new UserNotFoundException(email);
            switch (user.Status)
            {
                case UserStatus.BANNED:
                    throw new Exception("Vaš nalog je trajno uklonjen");
                case UserStatus.DEACTIVATED:
                    throw new Exception("Vaš nalog je deaktiviran");
            }
            if (!PasswordManager.ArePasswordsEqual(password, user.Password))
                throw new InvalidPasswordException();
            if (user.Role == UserRole.EDITOR)
            {
                user.Genres = await _genreStorage.GetEditorsGenres(user.Id);
            }
            return user;
        }

        public async System.Threading.Tasks.Task SendVerificationCode(User user)
        {
            var builder = new StringBuilder();
            var random = new Random();
            for(int i = 0; i < 6; i++) 
            {
                var digit = random.Next() % 10;
                builder.Append(digit);
            }
            user.VerificationCode = builder.ToString();
            await _userStorage.Update(user);
            var email = new Email($"{user.Name} {user.Surname}", user.Email, "Verification Code",
                $"Poštovani {user.Name} {user.Surname},\n" +
                $"Vaš verifikacioni kod je: {user.VerificationCode}"
                );
            _emailService.Send(email);
        }

        public async System.Threading.Tasks.Task Register(User user)
        {
            await _userStorage.Create(user);
            await SendVerificationCode(user);
        }
        public async Task<bool> Verify(User user, string code)
        {
            var dbCode = await _userStorage.GetVerificationCode(user.Id);
            if (dbCode == null) return false;
            if (dbCode != code) return false;
            user.VerificationCode = null;
            user.Status = UserStatus.ACTIVE;
            await _userStorage.Update(user);
            return true;
        }
    }
}
