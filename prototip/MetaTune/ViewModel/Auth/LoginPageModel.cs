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
using System.Windows.Navigation;
using MetaTune.View.Home;

namespace MetaTune.ViewModel.Auth
{
    public class LoginPageModel : INotifyPropertyChanged
    {
        public ICommand LoginCommand { get; }
        public event Action? ClearPasswordRequested;
        public LoginPageModel(NavigationService navigationService, Window win)
        {
            LoginCommand = new RelayCommand(Login);

            IUserStorage userStorage = Injector.CreateInstance<IUserStorage>();
            IGenreStorage genreStorage = Injector.CreateInstance<IGenreStorage>();
            IEmailService emailService = Injector.CreateInstance<IEmailService>();
            userController = new(userStorage, genreStorage, emailService);

            NavigationService = navigationService;
            Win = win;
        }

        private Window Win;
        // Add a property to hold the NavigationService instance
        private NavigationService? _navigationService;
        public NavigationService? NavigationService
        {
            get => _navigationService;
            set => _navigationService = value;
        }

        private async void Login(object parameter)
        {
            var loading = new LoadingDialog()
            {
                Owner = MainWindow.Instance
            };
            loading.Show();
            try
            {
                User user = await userController.Login(_email, _password)
                    ?? throw new Exception("Prijava neuspešna");
                if (user.Status == UserStatus.WAITINGVERIFICATION)
                {
                    if (Win is AuthFrame af)
                    {
                        af.NavigateTo(new VerificationPage(user, Win));
                        return;
                    }
                    NavigationService?.Navigate(new VerificationPage(user, Win));
                    return;
                }
                MainWindow.LoggedInUser = user;
                MainWindow.Instance.Navigate(new HomePage());
                Win.Close();
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