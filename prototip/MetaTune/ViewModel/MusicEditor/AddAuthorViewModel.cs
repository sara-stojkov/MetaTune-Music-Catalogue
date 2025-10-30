using Core.Model;
using Core.Storage;
using PostgreSQLStorage;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetaTune.ViewModel.MusicEditor
{
    public class AddArtistViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IGenreStorage _genreStorage;
        private readonly IUserStorage _userStorage;
        private readonly IMemberStorage _memberStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;

        private string? _artistName;
        private string? _artistSurname;
        private string? _biography;
        private DateTime? _joinDate;
        private Genre? _selectedGenre;
        private Author? _selectedGroup;
        private User? _selectedEditor;
        private bool _isIndividualArtist = true;
        private bool _isMusicGroup;
        private ObservableCollection<Genre> _genres = new();
        private ObservableCollection<Author> _groups = new();
        private ObservableCollection<User> _editors = new();
        private string? _review;
        private int? _selectedRating;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddArtistViewModel()
        {
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            _userStorage = Injector.CreateInstance<IUserStorage>();
            _memberStorage = Injector.CreateInstance<IMemberStorage>();
            _ratingStorage = Injector.CreateInstance<IRatingStorage>();
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);

            _ = LoadGenresAsync();
            _ = LoadGroupsAsync();
            _ = LoadEditorsAsync();
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

        public string? Review
        {
            get => _review;
            set
            {
                _review = value;
                OnPropertyChanged(nameof(Review));
            }
        }

        public int? SelectedRating
        {
            get => _selectedRating;
            set
            {
                _selectedRating = value;
                OnPropertyChanged(nameof(SelectedRating));
            }
        }

        public DateTime? JoinDate
        {
            get => _joinDate;
            set
            {
                _joinDate = value;
                OnPropertyChanged(nameof(JoinDate));
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

        public ObservableCollection<Author> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                OnPropertyChanged(nameof(Groups));
            }
        }

        public Author? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
            }
        }

        public ObservableCollection<User> Editors
        {
            get => _editors;
            set
            {
                _editors = value;
                OnPropertyChanged(nameof(Editors));
            }
        }

        public User? SelectedEditor
        {
            get => _selectedEditor;
            set
            {
                _selectedEditor = value;
                OnPropertyChanged(nameof(SelectedEditor));
            }
        }

        public ICommand SaveCommand { get; }

        private async System.Threading.Tasks.Task LoadGroupsAsync()
        {
            try
            {
                var allGroups = await _authorStorage.GetAll(AuthorFilter.Group);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Groups.Clear();
                    foreach (var group in allGroups)
                        Groups.Add(group);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju grupa: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        // Učitavanje žanrova iz baze
        private async System.Threading.Tasks.Task LoadEditorsAsync()
        {
            try
            {
                var allEditors = await _userStorage.GetAllByRole("EDITOR");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Editors.Clear();
                    foreach (var editor in allEditors)
                        Editors.Add(editor);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju editora: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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

            Person person = null;
            Member member = new Member(DateTime.Now, null, "", "") { };

            String authorId = Guid.NewGuid().ToString();

            if (IsIndividualArtist)
            {
                person = new Person(
                    Guid.NewGuid().ToString(),
                    ArtistName.Trim(),
                    ArtistSurname?.Trim()
                );
            }

            var author = new Author(
                authorId,
                $"{ArtistName} {ArtistSurname}".Trim(),
                Biography,
                person == null ? null : person.PersonId
            );

            if (IsMusicGroup) {
                await _authorStorage.CreateOne(author, null);
            }
            else
            {
                await _authorStorage.CreateOne(author, person);
            }

            if (IsIndividualArtist && SelectedGroup != null)
            {
                member = new Member(
                    (DateTime)JoinDate,
                    null,
                    SelectedGroup.AuthorId,
                    authorId
                );
                await _memberStorage.CreateOne(member);
            }


            if (SelectedRating != null)
            {
                var rating = new Rating
                (
                    Guid.NewGuid().ToString(),
                    (int)SelectedRating,
                    DateTime.Now,
                    MainWindow.LoggedInUser.Id,
                    null,
                    authorId
                );
                await _ratingStorage.CreateOne(rating);
            }

            if (!string.IsNullOrWhiteSpace(Review))
            {
                var review = new Review
                (
                    Guid.NewGuid().ToString(),
                    Review!.Trim(),
                    DateTime.Now,
                    false,
                    null,
                    MainWindow.LoggedInUser.Id,
                    null,
                    authorId,
                    MainWindow.LoggedInUser.Name + " " + MainWindow.LoggedInUser.Surname
                );
                await _reviewStorage.CreateOne(review);
            }

            MessageBox.Show("Izvođač uspešno dodat!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSave =>
            !string.IsNullOrWhiteSpace(ArtistName);

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
