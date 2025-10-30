using MetaTune.ViewModel.Home;
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

namespace MetaTune.View.Home
{
    public partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        public AlbumPage(AlbumPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.SongNavigationRequested += OnSongNavigationRequested;
            viewModel.ArtistNavigationRequested += OnArtistNavigationRequested;
        }

        private void OnSongNavigationRequested(object sender, SongPageViewModel songViewModel)
        {
            try
            {
                var songPage = new SongPage(songViewModel);
                NavigationService?.Navigate(songPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji ka pesmi: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnArtistNavigationRequested(object sender, ArtistPageViewModel artistViewModel)
        {
            try
            {
                var artistPage = new ArtistPage(artistViewModel);
                NavigationService?.Navigate(artistPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji ka izvođaču: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
