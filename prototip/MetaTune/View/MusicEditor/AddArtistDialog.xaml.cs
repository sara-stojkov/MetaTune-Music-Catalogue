using MetaTune.ViewModel.MusicEditor;
using PostgreSQLStorage;
using System.Windows;

namespace MetaTune.View.MusicEditor
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
