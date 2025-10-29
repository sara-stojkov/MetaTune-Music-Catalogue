using System.Windows;
using System.Windows.Controls;

namespace MetaTune.View.MusicEditor
{
    public partial class EditorHomePage : Page
    {
        public EditorHomePage()
        {
            InitializeComponent();
        }

        // Event handler for the "Add New Genre" button click
        private void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddGenreDialog();
            dialog.ShowDialog();
        }
    }
}