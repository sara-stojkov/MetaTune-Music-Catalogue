using Core.Model;
using Core.Storage;
using MetaTune.View.Admin;
using PostgreSQLStorage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.ViewModel.Admin
{
    public class AdminHomeViewModel : INotifyPropertyChanged
    {
        private readonly IUserStorage _userStorage;
        private readonly IGenreStorage _genreStorage;

        public ObservableCollection<EditorItem> Editors { get; set; } = new();

        private EditorItem _selectedEditor;
        public EditorItem SelectedEditor
        {
            get => _selectedEditor;
            set
            {
                _selectedEditor = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand AddEditorCommand { get; }
        public ICommand AddArtistCommand { get; }
        public ICommand AddSongCommand { get; }
        public ICommand AddGenreCommand { get; }
        public ICommand AddAlbumCommand { get; }


        public AdminHomeViewModel(IUserStorage userStorage)
        {
            _userStorage = userStorage;
            _genreStorage = Injector.CreateInstance<IGenreStorage>();

            DeleteCommand = new RelayCommand(async _ => await DeleteEditor(), _ => SelectedEditor != null);
            EditCommand = new RelayCommand(async _ => await EditEditor(), _ => SelectedEditor != null);
            AddEditorCommand = new RelayCommand(async _ => await AddEditor());
            AddArtistCommand = new RelayCommand(async _ => await AddArtist());
            AddSongCommand = new RelayCommand(async _ => await AddSong());
            AddGenreCommand = new RelayCommand(async _ => await AddGenre());
            AddAlbumCommand = new RelayCommand(async _ => await AddAlbum());


            _ = LoadEditors();
        }

        private async Task LoadEditors()
        {
            var editors = await _userStorage.GetAllByRole("EDITOR");
            Editors.Clear();
            foreach (var e in editors)
            {
               var g =  await _genreStorage.GetEditorsGenres(e.Id);
               e.Genres = g;
            }
            foreach (var e in editors)
                Editors.Add(new EditorItem(e));
        }

        private async Task DeleteEditor()
        {
            if (SelectedEditor == null)
                return;
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Da li ste sigurni da želite da obrišete urednika {SelectedEditor.Name} {SelectedEditor.Surname}?",
                    "Potvrda brisanja",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            await _userStorage.DeleteById(SelectedEditor.User.Id);
            Editors.Remove(SelectedEditor);
        }

        private async Task AddEditor()
        {
            var addEditor = new AddEditorDialog();
            addEditor.Owner = Application.Current.MainWindow;
            if (addEditor.ShowDialog() == true)
                await LoadEditors();
        }

        private async Task AddArtist()
        {
            var addArtist = new AddArtistDialog();
            addArtist.Owner = Application.Current.MainWindow;
            addArtist.ShowDialog();
        }

        private async Task AddSong()
        {
            var addSong = new AddSongDialog();
            addSong.Owner = Application.Current.MainWindow;
            addSong.ShowDialog();
        }

        private async Task AddGenre()
        {
            var addGenre = new AddGenreDialog();
            addGenre.Owner = Application.Current.MainWindow;
            addGenre.ShowDialog();
        }

        private async Task AddAlbum()
        {
            var addAlbum = new AddAlbumDialog();
            addAlbum.Owner = Application.Current.MainWindow;
            addAlbum.ShowDialog();
        }


        private async Task EditEditor()
        {
            if (SelectedEditor == null)
                return;

            var editWindow = new AddEditorDialog(true, SelectedEditor.User);
            if (editWindow.ShowDialog() == true)
            {
                await LoadEditors();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public class EditorItem : INotifyPropertyChanged
    {
        public User User { get; }

        public EditorItem(User user)
        {
            User = user;
            ContentCount = 0;
        }

        public string Name => User.Name;
        public string Surname => User.Surname;
        public string Email => User.Email;

        public string DisplayGenres
        {
            get { 
                if (User.Genres == null || !User.Genres.Any())
                    return "Nema";
                return string.Join(", ", User.Genres.SelectMany(g => g.Flat).Select(g => g.Name));

            }
        }

        private int _contentCount;
        public int ContentCount
        {
            get => _contentCount;
            set
            {
                if (_contentCount != value)
                {
                    _contentCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public async void Execute(object parameter) => await _executeAsync(parameter);
    }
}
