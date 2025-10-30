using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MetaTune.ViewModel.Home;

namespace MetaTune.View.Home
{
    public partial class ArtistPage : Page
    {
        public ArtistPage()
        {
            InitializeComponent();
        }

        public ArtistPage(ArtistPageViewModel viewModel) : this()
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
                System.Windows.MessageBox.Show($"Greška pri navigaciji ka pesmi: {ex.Message}",
                    "Greška", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show($"Greška pri navigaciji ka izvođaču: {ex.Message}",
                    "Greška", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
