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

namespace MetaTune.ViewModel.Admin
{
    internal class AddAlbumViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IGenreStorage _genreStorage;
        private readonly IUserStorage _userStorage;
        private readonly IWorkStorage _workStorage;
        private readonly ITaskStorage _taskStorage;

        private string? _albumName;
        private string? _description;
        private DateTime? _publishDate;
        private Work? _selectedAlbum;
        private Genre? _selectedGenre;
        private User? _selectedEditor;

        private ObservableCollection<Genre> _genres = new();
        private ObservableCollection<Author> _authors = new();
        private ObservableCollection<User> _editors = new();
        private ObservableCollection<Work> _albums = new();

        public ObservableCollection<Author> SelectedAuthors { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddAlbumViewModel()
        {
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            _userStorage = Injector.CreateInstance<IUserStorage>();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _taskStorage = Injector.CreateInstance<ITaskStorage>();

            SelectedAuthors = new ObservableCollection<Author>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);

            _ = LoadGenresAsync();
            _ = LoadAuthorsAsync();
            _ = LoadEditorsAsync();
            _ = LoadAlbumsAsync();
        }

        public string? AlbumName
        {
            get => _albumName;
            set
            {
                _albumName = value;
                OnPropertyChanged(nameof(AlbumName));
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
            if (string.IsNullOrWhiteSpace(AlbumName))
            {
                MessageBox.Show("Ime albuma je obavezno.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var album = new Work(
                Guid.NewGuid().ToString(),
                AlbumName!.Trim(),
                (DateTime)PublishDate,
                WorkType.Album,
                SelectedGenre.Id,
                SelectedAuthors.ToList(),
                Description?.Trim(),
                null,
                null
            );

            await _workStorage.CreateOne(album);

            if (SelectedEditor != null)
            {
                var task = new Core.Model.Task(
                    Guid.NewGuid().ToString(),
                    DateTime.Now,
                    false,
                    SelectedEditor.Id,
                    album.WorkId,
                    null
                );
                await _taskStorage.CreateOne(task);
            }

            MessageBox.Show("Album je uspešno dodat!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSave =>
            !string.IsNullOrWhiteSpace(AlbumName);

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
