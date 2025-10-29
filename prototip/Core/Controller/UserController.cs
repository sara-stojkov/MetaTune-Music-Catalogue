using Core.Model;
using Core.Storage;
using Core.Utils;

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
        IUserStorage userStorage
        )
    {
        private readonly IUserStorage _userStorage = userStorage;

        public async Task<User> Login(string email, string password)
        {
            var user = await _userStorage.GetByEmail(email)
                ?? throw new UserNotFoundException(email);
            if (!PasswordManager.ArePasswordsEqual(password, user.Password))
                throw new InvalidPasswordException();
            if (user.Role == UserRole.EDITOR)
            {
                user.Genres = await _userStorage.GetEditorsGenres(user.Id);
            }
            return user;
        }
    }
}
