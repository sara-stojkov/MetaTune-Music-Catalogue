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

namespace MetaTune.ViewModel.Auth
{
    public class LoginPageModel : INotifyPropertyChanged
    {
        public ICommand LoginCommand { get; }
        public event Action? ClearPasswordRequested;
        public LoginPageModel()
        {
            LoginCommand = new RelayCommand(Login);

            IUserStorage userStorage = Injector.CreateInstance<IUserStorage>();
            IGenreStorage genreStorage = Injector.CreateInstance<IGenreStorage>();
            userController = new(userStorage, genreStorage);
        }

        private async void Login(object parameter)
        {
            try
            {
                User user = await userController.Login(_email, _password)
                    ?? throw new Exception("Prijava neuspešna");
                MessageBox.Show($"Dobrodošli {user.Name} {user.Surname}");
            }
            catch (Exception ex)
            {
                Password = string.Empty;
                ClearPasswordRequested?.Invoke();
                MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private readonly UserController userController;
        private string _email = "";
        private string _password = "";

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}