using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Model;
using MetaTune.ViewModel.Home;

namespace MetaTune.View.Home
{
    public partial class SongPage : Page
    {
        private SongPageViewModel _viewModel;

        public SongPage()
        {
            InitializeComponent();
        }

        public SongPage(SongPageViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void Artist_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                if (border?.Tag is string authorId)
                {
                    var artistModel = new ArtistPageViewModel(authorId, MainWindow.LoggedInUser);
                    await artistModel.LoadArtist(authorId);
                    var artistPage = new ArtistPage(artistModel);
                    NavigationService?.Navigate(artistPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji na izvođača: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AlbumBorder_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_viewModel?.Album != null)
                {
                    var albumModel = new AlbumPageViewModel(_viewModel.Album.WorkId, MainWindow.LoggedInUser);
                    await albumModel.LoadAlbum(_viewModel.Album.WorkId);
                    var albumPage = new AlbumPage(albumModel);
                    NavigationService?.Navigate(albumPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji na album: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}