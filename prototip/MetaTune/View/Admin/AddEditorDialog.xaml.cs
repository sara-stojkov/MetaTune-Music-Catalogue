

using System.Windows;

namespace MetaTune.View.Admin
{
    public partial class AddEditorDialog : Window
    {
        public AddEditorDialog()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected genres
            var selectedGenres = GenreListBox.SelectedItems;
            DialogResult = true;
            Close();
        }
    }
}
