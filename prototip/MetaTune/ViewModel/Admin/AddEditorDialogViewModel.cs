using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Model;
using Core.Storage;
using Core.Controller;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.ViewModel.Admin
{
    public class AddEditorDialogViewModel : INotifyPropertyChanged
    {
        private readonly IUserStorage _userStorage;
        private readonly IGenreStorage _genreStorage;

        private readonly bool _isEdit;
        private readonly User _existingUser;

        public AddEditorDialogViewModel(bool isEdit = false, User user = null)
        {
            _userStorage = Injector.CreateInstance<IUserStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();

            _isEdit = isEdit;
            _existingUser = user;

            RegisterCommand = new RelayCommand(async (_) => await RegisterOrUpdateEditorAsync());
            LoadGenresCommand = new RelayCommand(async (_) => await LoadGenresAsync());

            Genres = new ObservableCollection<Genre>();
            SelectedGenres = new ObservableCollection<Genre>();

            _ = LoadGenresAsync();

            if (_isEdit && _existingUser != null)
            {
                FirstName = _existingUser.Name;
                LastName = _existingUser.Surname;
                Email = _existingUser.Email;
                Password = "";

                async void LoadSelectedGenres()
                {
                    var genres = await _genreStorage.GetEditorsGenres(_existingUser.Id);
                    foreach (var g in genres)
                        SelectedGenres.Add(g);
                    _existingUser.Genres = genres;
                    MessageBox.Show($"{genres.Count}");
                }
                LoadSelectedGenres();

                ButtonText = "Sačuvaj izmene";
            }
            else
            {
                ButtonText = "Registruj urednika";
            }
        }

        private string _buttonText;
        public string ButtonText
        {
            get => _buttonText;
            set { _buttonText = value; OnPropertyChanged(nameof(ButtonText)); }
        }

        private string _firstName = "";
        private string _lastName = "";
        private string _email = "";
        private string _password = "";

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

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

        public ObservableCollection<Genre> Genres { get; set; }
        public ObservableCollection<Genre> SelectedGenres { get; set; }

        public ICommand RegisterCommand { get; }
        public ICommand LoadGenresCommand { get; }


        private async Task LoadGenresAsync()
        {
            try
            {
                Genres.Clear();
                var genresFromDb = await _genreStorage.GetAll();
                foreach (var g in genresFromDb)
                    Genres.Add(g);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju žanrova: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RegisterOrUpdateEditorAsync()
        {
            try
            {
                if (_isEdit)
                {
                    Password = _existingUser.Password;
                }
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    MessageBox.Show("Unesite ime urednika.", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(LastName))
                {
                    MessageBox.Show("Unesite prezime urednika.", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Core.Utils.Validator.IsValidEmail(Email))
                {
                    MessageBox.Show("Unesite ispravan email.", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Core.Utils.Validator.IsValidPassword(Password))
                {
                    MessageBox.Show("Lozinka nije validna.", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedGenres == null || SelectedGenres.Count == 0)
                {
                    MessageBox.Show("Odaberite bar jedan žanr.", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEdit && _existingUser != null)
                {
                    _existingUser.Name = FirstName.Trim();
                    _existingUser.Surname = LastName.Trim();
                    _existingUser.Email = Email.Trim();

                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        _existingUser.Password = Password.Trim();
                        _existingUser.HashPassword();
                    }

                    _existingUser.Genres = SelectedGenres.ToList();

                    await _userStorage.UpdateOne(_existingUser);
                    MessageBox.Show("Urednik je uspešno ažuriran!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var editor = new User(
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString(),
                        FirstName.Trim(),
                        LastName.Trim(),
                        Email.Trim(),
                        Password.Trim(),
                        UserRole.EDITOR,
                        UserStatus.ACTIVE
                    )
                    {
                        Genres = SelectedGenres.ToList(),
                    };

                    editor.HashPassword();
                    await _userStorage.CreateOne(editor);

                    MessageBox.Show("Urednik je uspešno dodat!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);

                    FirstName = "";
                    LastName = "";
                    Email = "";
                    Password = "";
                    SelectedGenres.Clear();
                }

                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
