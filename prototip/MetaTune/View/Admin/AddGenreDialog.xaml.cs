using System.Windows;

namespace MetaTune.View.Admin
{
    public partial class AddGenreDialog : Window
    {
            public AddGenreDialog()
            {
                InitializeComponent();
            }

            private void ConfirmButton_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = true;
                Close();
            }
        }
    }
