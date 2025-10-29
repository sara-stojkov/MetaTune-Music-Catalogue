using Core.Model;
namespace Core.DTO
{
    public class RegisterDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly DateOfBirth { get; set; }

        public RegisterDTO()
        {
            Name = Surname = Email = Password = PhoneNumber = string.Empty;
        }
    }
}
