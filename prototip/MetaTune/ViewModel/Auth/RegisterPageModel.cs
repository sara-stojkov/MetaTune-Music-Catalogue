using MetaTune.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Storage;
using Core.Model;
using Core.Controller;
using MetaTune.View;
using MetaTune.View.Auth;
using Core.Services.EmailService;

namespace MetaTune.ViewModel.Auth
{
    public class RegisterPageModel : INotifyPropertyChanged
    {
        public ICommand RegisterCommand { get; }
        public event Action? ClearPasswordRequested;
        public RegisterPageModel()
        {
            RegisterCommand = new RelayCommand(Register);

            IUserStorage userStorage = Injector.CreateInstance<IUserStorage>();
            IGenreStorage genreStorage = Injector.CreateInstance<IGenreStorage>();
            IEmailService emailService = Injector.CreateInstance<IEmailService>();
            userController = new(userStorage, genreStorage, emailService);
        }

        private async void Register(object parameter)
        {
            var loading = new LoadingDialog()
            {
                Owner = MainWindow.Instance
            };
            loading.Show();
            try
            {
                User user = new(Name, Surname, Email, Password);
                user.HashPassword();
                await userController.Register(user);
                MainWindow.Instance.Navigate(new VerificationPage(user));
            }
                catch (Exception ex)
                {
                    Password = string.Empty;
                    ClearPasswordRequested?.Invoke();
            MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    loading.SafeClose();
                }
            }

        private readonly UserController userController;
        private string _email = "";
        private string _password = "";
        private string _name = "";
        private string _surname = "";

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name));}
        }
        public string Surname
        {
            get => _surname;
            set { _surname = value; OnPropertyChanged(nameof(Surname)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
