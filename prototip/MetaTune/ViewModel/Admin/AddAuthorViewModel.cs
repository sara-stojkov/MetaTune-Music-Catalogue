using Core.Model;
using Core.Storage;
using PostgreSQLStorage;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MetaTune.ViewModel.Admin
{
    public class AddArtistViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IGenreStorage _genreStorage;

        private string? _artistName;
        private string? _artistSurname;
        private string? _biography;
        private Genre? _selectedGenre;
        private bool _isIndividualArtist = true;
        private bool _isMusicGroup;
        private ObservableCollection<Genre> _genres = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddArtistViewModel()
        {
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);

            // Automatsko učitavanje žanrova
            _ = LoadGenresAsync();
        }

        public string? ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
                OnPropertyChanged(nameof(ArtistName));
            }
        }

        public string? ArtistSurname
        {
            get => _artistSurname;
            set
            {
                _artistSurname = value;
                OnPropertyChanged(nameof(ArtistSurname));
            }
        }

        public string? Biography
        {
            get => _biography;
            set
            {
                _biography = value;
                OnPropertyChanged(nameof(Biography));
            }
        }

        public bool IsIndividualArtist
        {
            get => _isIndividualArtist;
            set
            {
                _isIndividualArtist = value;
                if (value) IsMusicGroup = false;
                OnPropertyChanged(nameof(IsIndividualArtist));
            }
        }

        public bool IsMusicGroup
        {
            get => _isMusicGroup;
            set
            {
                _isMusicGroup = value;
                if (value) IsIndividualArtist = false;
                OnPropertyChanged(nameof(IsMusicGroup));
            }
        }

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set
            {
                _genres = value;
                OnPropertyChanged(nameof(Genres));
            }
        }

        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                _selectedGenre = value;
                OnPropertyChanged(nameof(SelectedGenre));
            }
        }

        public ICommand SaveCommand { get; }

        // Učitavanje žanrova iz baze
        private async System.Threading.Tasks.Task LoadGenresAsync()
        {
            try
            {
                var allGenres = await _genreStorage.GetAll();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Genres.Clear();
                    foreach (var genre in allGenres)
                        Genres.Add(genre);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju žanrova: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Validacija i čuvanje izvođača
        public async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(ArtistName))
            {
                MessageBox.Show("Ime izvođača je obavezno.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var author = new Author(
                Guid.NewGuid().ToString(),
                $"{ArtistName} {ArtistSurname}".Trim(),
                Biography,
                null
            );

            await _authorStorage.CreateOne(author);

            MessageBox.Show("Izvođač uspešno dodat!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSave =>
            !string.IsNullOrWhiteSpace(ArtistName);

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
