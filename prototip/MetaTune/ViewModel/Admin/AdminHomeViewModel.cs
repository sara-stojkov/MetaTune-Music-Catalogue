using Core.Model;
using Core.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using MetaTune.View.Admin;
using System.Windows;

namespace MetaTune.ViewModel.Admin
{
    public class AdminHomeViewModel : INotifyPropertyChanged
    {
        private readonly IUserStorage _userStorage;

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

        public AdminHomeViewModel(IUserStorage userStorage)
        {
            _userStorage = userStorage;

            DeleteCommand = new RelayCommand(async _ => await DeleteEditor(), _ => SelectedEditor != null);
            EditCommand = new RelayCommand(async _ => await EditEditor(), _ => SelectedEditor != null);

            _ = LoadEditors();
        }

        private async Task LoadEditors()
        {
            var editors = await _userStorage.GetAllByRole("EDITOR");
            Editors.Clear();
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

        public string DisplayGenres => User.Genres != null ? string.Join(", ", User.Genres.Select(g => g.Name)) : "Nema";

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
