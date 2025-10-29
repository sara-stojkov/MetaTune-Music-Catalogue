using Core.Storage;
using MetaTune.ViewModel.Admin;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetaTune.View.Admin
{
    /// <summary>
    /// Interaction logic for AdminHomePage.xaml
    /// </summary>
    public partial class AdminHomePage : Page
    {
        private readonly IUserStorage _userStorage;

        public AdminHomePage()
        {
            InitializeComponent();
            _userStorage = Injector.CreateInstance<IUserStorage>();
            DataContext = new AdminHomeViewModel(_userStorage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addEditor = new AddEditorDialog();
            addEditor.Owner = Application.Current.MainWindow;
            addEditor.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var addArtist = new AddArtistDialog();
            addArtist.Owner = Application.Current.MainWindow;
            addArtist.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var addSong = new AddSongDialog();
            addSong.Owner = Application.Current.MainWindow;
            addSong.ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var addGenre = new AddGenreDialog();
            addGenre.Owner = Application.Current.MainWindow;
            addGenre.ShowDialog();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var addAlbum = new AddAlbumDialog();
            addAlbum.Owner = Application.Current.MainWindow;
            addAlbum.ShowDialog();
        }
    }
}
