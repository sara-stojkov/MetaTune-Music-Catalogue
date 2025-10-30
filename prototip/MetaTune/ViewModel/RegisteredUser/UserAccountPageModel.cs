using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Model;
using Core.Storage;
using MetaTune.View;

namespace MetaTune.ViewModel.RegisteredUser
{
    public class UserAccountPageModel
    {
        private readonly IUserStorage userStorage;
        public UserAccountPageModel(User user) 
        {
            User = user;
            Name = user.Name;
            Surname = user.Surname;
            Email = user.Email;
            Password = string.Empty;
            IsContactVisible = user.IsContactVisible ?? true;
            AreReviewsVisible = user.AreReviewsVisible ?? true;

            UpdatePersonalDataCommand = new RelayCommand(UpdatePersonalData);
            UpdateVisibilityCommand = new RelayCommand(UpdateVisibility);

            userStorage = Injector.CreateInstance<IUserStorage>();
        }
        public User User;
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsContactVisible { get; set; }
        public bool NotIsContactVisible
        {
            get => !IsContactVisible;
            set => IsContactVisible = !value;
        }
        public bool AreReviewsVisible { get; set; }
        public bool NotAreReviewsVisible
        {
            get => !AreReviewsVisible;
            set => AreReviewsVisible = !value;
        }

        public ICommand UpdatePersonalDataCommand {  get; }
        public ICommand UpdateVisibilityCommand { get; }

        public async void UpdatePersonalData(object parameter)
        {
            var oldName = User.Name;
            var oldSurname = User.Surname;
            var oldEmail = User.Email;
            var oldPassword = User.Password;
            var loading = new LoadingDialog()
            {
                Owner = MainWindow.Instance
            };
            loading.Show();
            try
            {
                User.Name = Name;
                User.Surname = Surname;
                User.Email = Email;
                if (Password.Length > 0)
                {
                    User.Password = Password;
                    User.HashPassword();
                }
                await userStorage.UpdateOne(User);
                MessageBox.Show("Uspešno ažurirani podaci");
            }
            catch (Exception ex)
            {
                User.Name = oldName;
                User.Surname = oldSurname;
                User.Email = oldEmail;
                User.Password = oldPassword;
                Password = string.Empty;
                Name = oldName;
                Surname = oldSurname;
                Email = oldEmail;
                MessageBox.Show("Došlo je do greške: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loading.SafeClose();
            }
        }

        public async void UpdateVisibility(object parameter)
        {

            bool oldContact = User.IsContactVisible ?? true;
            bool oldReviews = User.AreReviewsVisible ?? true;
            var loading = new LoadingDialog()
            {
                Owner = MainWindow.Instance
            };
            loading.Show();
            try
            {
                User.IsContactVisible = IsContactVisible;
                User.AreReviewsVisible = AreReviewsVisible;
                await userStorage.UpdateOne(User);
                MessageBox.Show("Uspešno ažurirani podaci");
            }
            catch (Exception ex)
            {
                User.IsContactVisible = oldContact;
                User.AreReviewsVisible = oldReviews;
                IsContactVisible = oldContact;
                AreReviewsVisible = oldReviews;
                MessageBox.Show("Došlo je do greške: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loading.SafeClose();
            }
        }

    }
}
