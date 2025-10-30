using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MetaTune.ViewModel.Admin;
using Core.Model;

namespace MetaTune.View.Admin
{
    public partial class AddEditorDialog : Window
    {
        public AddEditorDialog() : this(false, null)
        {
        }

        public AddEditorDialog(bool isEdit, Core.Model.User existingUser)
        {
            InitializeComponent();


            DataContext = new AddEditorDialogViewModel(isEdit, existingUser);
        }

        private void GenreListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AddEditorDialogViewModel vm && !vm.firstLoad)
            {
                vm.SelectedGenres.Clear();
                foreach (Genre g in ((ListBox)sender).SelectedItems)
                    vm.SelectedGenres.Add(g);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Izmena podataka urednika", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
            if (DataContext is AddEditorDialogViewModel vm && vm.SelectedGenres.Any())
            {
                foreach (var genre in vm.SelectedGenres)
                {
                    var match = vm.Genres.FirstOrDefault(g => g.Id == genre.Id);
                    if (match != null)
                        GenreListBox.SelectedItems.Add(match);
                }
                vm.firstLoad = false;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEditorDialogViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }
    }
}
