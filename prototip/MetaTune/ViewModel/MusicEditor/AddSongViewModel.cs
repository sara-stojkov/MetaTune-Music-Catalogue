using Core.Model;
using Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MetaTune.ViewModel.MusicEditor
{
    internal class AddSongViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IGenreStorage _genreStorage;
        private readonly IUserStorage _userStorage;
        private readonly IWorkStorage _workStorage;
        private readonly ITaskStorage _taskStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;

        private string? _songName;
        private string? _description;
        private DateTime? _publishDate;
        private Work? _selectedAlbum;
        private Genre? _selectedGenre;
        private User? _selectedEditor;
        private string? _review;
        private int? _selectedRating;

        private ObservableCollection<Genre> _genres = new();
        private ObservableCollection<Author> _authors = new();
        private ObservableCollection<User> _editors = new();
        private ObservableCollection<Work> _albums = new();

        public ObservableCollection<Author> SelectedAuthors { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddSongViewModel()
        {
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            _userStorage = Injector.CreateInstance<IUserStorage>();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _taskStorage = Injector.CreateInstance<ITaskStorage>();
            _ratingStorage = Injector.CreateInstance<IRatingStorage>();
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();

            SelectedAuthors = new ObservableCollection<Author>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);

            _ = LoadGenresAsync();
            _ = LoadAuthorsAsync();
            _ = LoadEditorsAsync();
            _ = LoadAlbumsAsync();
        }

        public string? SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                OnPropertyChanged(nameof(SongName));
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
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

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set
            {
                _authors = value;
                OnPropertyChanged(nameof(Authors));
            }
        }

        public DateTime? PublishDate
        {
            get => _publishDate;
            set
            {
                _publishDate = value;
                OnPropertyChanged(nameof(PublishDate));
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

        public ObservableCollection<Work> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged(nameof(Albums));
            }
        }

        public Work? SelectedAlbum
        {
            get => _selectedAlbum;
            set
            {
                _selectedAlbum = value;
                OnPropertyChanged(nameof(SelectedAlbum));
            }
        }

        public ICommand SaveCommand { get; }

        private async System.Threading.Tasks.Task LoadAlbumsAsync()
        {
            try
            {
                var allAlbums = await _workStorage.GetAll();
                allAlbums = allAlbums.Where(w => w.WorkType == WorkType.Album).ToList();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Albums.Clear();
                    foreach (var album in allAlbums)
                        Albums.Add(album);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju autora: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadAuthorsAsync()
        {
            try
            {
                var allAuthors = await _authorStorage.GetAll(AuthorFilter.All);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Authors.Clear();
                    foreach (var author in allAuthors)
                        Authors.Add(author);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju autora: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // Učitavanje editora iz baze
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
            if (string.IsNullOrWhiteSpace(SongName))
            {
                MessageBox.Show("Ime pesme je obavezno.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var song = new Work(
                Guid.NewGuid().ToString(),
                SongName!.Trim(),
                (DateTime)PublishDate,
                WorkType.Song,
                SelectedGenre.Id,
                SelectedAuthors.ToList(),
                Description?.Trim(),
                null,
                SelectedAlbum?.WorkId
            );

            await _workStorage.CreateOne(song);

            if (SelectedRating != null)
            {
                var rating = new Rating
                (
                    Guid.NewGuid().ToString(),
                    (int)SelectedRating,
                    DateTime.Now,
                    MainWindow.LoggedInUser.Id,
                    song.WorkId,
                    null
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
                    song.WorkId
                );
                await _reviewStorage.CreateOne(review);
            }

            MessageBox.Show("Pesma uspešno dodata!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSave =>
            !string.IsNullOrWhiteSpace(SongName);

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
