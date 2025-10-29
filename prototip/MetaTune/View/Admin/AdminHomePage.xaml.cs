using System.Windows;
using System.Windows.Controls;

namespace MetaTune.View.Admin
{
    public partial class AdminHomePage : Page
    {
        public AdminHomePage()
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