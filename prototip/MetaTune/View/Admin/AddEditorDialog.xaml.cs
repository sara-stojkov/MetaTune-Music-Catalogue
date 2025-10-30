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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddEditorDialogViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private void GenreListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AddEditorDialogViewModel vm)
            {
                vm.SelectedGenres.Clear();
                foreach (Genre g in ((ListBox)sender).SelectedItems.Cast<Genre>())
                    vm.SelectedGenres.Add(g);
            }
        }
    }
}
