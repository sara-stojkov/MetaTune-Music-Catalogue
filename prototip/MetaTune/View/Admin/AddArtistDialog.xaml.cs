using MetaTune.ViewModel.Admin;
using PostgreSQLStorage;
using System.Windows;

namespace MetaTune.View.Admin
{
    public partial class AddArtistDialog:Window
    {
        public AddArtistDialog()
        {
            InitializeComponent();
            DataContext = new AddArtistViewModel();
        }

    }
}
