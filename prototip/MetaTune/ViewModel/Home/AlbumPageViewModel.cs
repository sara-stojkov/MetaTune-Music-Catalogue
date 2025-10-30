using Core.Model;
using Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MetaTune.ViewModel.Home
{
    public class AlbumPageViewModel : INotifyPropertyChanged
    {
        private readonly IWorkStorage _workStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;
        private readonly User _currentUser;

        private Work? _album;
        private string _albumName = string.Empty;
        private DateTime _publishDate;
        private string _albumDescription = string.Empty;
        private ObservableCollection<Work> _songs = new();
        private ObservableCollection<AlbumArtist> _artists = new();
        private decimal? _averageRating;
        private ReviewWithRating? _editorReviewWithRating;
        private ObservableCollection<Rating> _userRatings = new();
        private ObservableCollection<Review> _userReviews = new();
        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;

        public AlbumPageViewModel(string albumId, User currentUser)
        {
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _ratingStorage = Injector.CreateInstance<IRatingStorage>();
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();
            _currentUser = currentUser;

            LeaveRatingCommand = new AsyncRelayCommand(LeaveRating);
            LeaveReviewCommand = new AsyncRelayCommand(LeaveReview);
            NavigateToSongCommand = new RelayCommand(async (param) => await NavigateToSong(param));
            NavigateToArtistCommand = new RelayCommand(async (param) => await NavigateToArtist(param));
        }

        public async System.Threading.Tasks.Task LoadAlbum(string albumId)
        {
            try
            {
                _album = await _workStorage.GetById(albumId);

                if (_album == null)
                    return;

                AlbumName = _album.WorkName;
                PublishDate = _album.PublishDate;
                AlbumDescription = _album.WorkDescription ?? string.Empty;

                // Load artists/authors
                if (_album.Authors != null && _album.Authors.Any())
                {
                    var artistList = new List<AlbumArtist>();
                    foreach (var author in _album.Authors)
                    {
                        artistList.Add(new AlbumArtist
                        {
                            ArtistId = author.AuthorId,
                            ArtistName = author.AuthorName ?? "Nepoznat izvođač"
                        });
                    }
                    Artists = new ObservableCollection<AlbumArtist>(artistList);
                }

                // Load songs
                var songs = await _workStorage.GetAllByAlbumId(albumId);
                Songs = new ObservableCollection<Work>(songs);

                // Load ratings and calculate average
                var ratings = await _ratingStorage.GetAllByWorkId(albumId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Load all reviews
                var allReviews = await _reviewStorage.GetAllApprovedByWorkId(albumId);

                // Get THE editor review using dedicated method
                var editorReview = await _reviewStorage.GetEditorReviewByWorkId(albumId);
                if (editorReview != null)
                {
                    var editorRating = ratings.FirstOrDefault(r => r.UserId == editorReview.UserId);
                    EditorReviewWithRating = new ReviewWithRating
                    {
                        Review = editorReview,
                        Rating = editorRating?.Value
                    };
                }

                // Get user reviews (not editor)
                var userReviewsList = allReviews;
                UserReviews = new ObservableCollection<Review>(userReviewsList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading album: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task NavigateToSong(object parameter)
        {
            if (parameter is Work song)
            {
                try
                {
                    var songViewModel = new SongPageViewModel(song.WorkId, _currentUser);
                    await songViewModel.LoadSong(song.WorkId);
                    SongNavigationRequested?.Invoke(this, songViewModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error navigating to song: {ex.Message}");
                }
            }
        }

        private async System.Threading.Tasks.Task NavigateToArtist(object parameter)
        {
            if (parameter is AlbumArtist artist)
            {
                try
                {
                    var artistViewModel = new ArtistPageViewModel(artist.ArtistId, _currentUser);
                    await artistViewModel.LoadArtist(artist.ArtistId);
                    ArtistNavigationRequested?.Invoke(this, artistViewModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error navigating to artist: {ex.Message}");
                }
            }
        }

        private async System.Threading.Tasks.Task LeaveRating()
        {
            if (_album == null || _newRatingValue < 1 || _newRatingValue > 5)
                return;

            try
            {
                if (_currentUser == null)
                {
                    MessageBox.Show("Morate biti prijavljeni da biste ostavili ocenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!(_currentUser.Role == UserRole.BASIC || _currentUser.Role == UserRole.EDITOR))
                {
                    MessageBox.Show($"Korisnici sa role {_currentUser.Role} ne mogu ostavljati ocene.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var rating = new Rating(
                    ratingId: Guid.NewGuid().ToString(),
                    value: _newRatingValue,
                    ratingDate: DateTime.Now,
                    userId: _currentUser.Id,
                    workId: _album.WorkId
                );

                await _ratingStorage.CreateOne(rating);

                // Reload ratings
                var ratings = await _ratingStorage.GetAllByWorkId(_album.WorkId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Update user ratings list
                var editorUserId = EditorReviewWithRating?.Review?.UserId;
                var userRatingsList = ratings.Where(r => r.UserId != editorUserId).ToList();
                UserRatings = new ObservableCollection<Rating>(userRatingsList);

                NewRatingValue = 0;

                await LoadAlbum(_album.WorkId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leaving rating: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LeaveReview()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Morate biti prijavljeni da biste ostavili recenziju.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(_currentUser.Role == UserRole.BASIC || _currentUser.Role == UserRole.EDITOR))
            {
                MessageBox.Show($"Korisnici sa role {_currentUser.Role} ne mogu ostavljati recenzije.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_album == null || string.IsNullOrWhiteSpace(_newReviewContent))
                return;

            try
            {
                var review = new Review(
                    reviewId: Guid.NewGuid().ToString(),
                    content: _newReviewContent,
                    reviewDate: DateTime.Now,
                    isEditable: false,
                    editor: _currentUser.Role == UserRole.EDITOR ? _currentUser.Id : null,
                    userId: _currentUser.Id,
                    workId: _album.WorkId
                );

                await _reviewStorage.CreateOne(review);

                MessageBox.Show("Hvala što ste ostavili recenziju!", "Recenzija poslata", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadAlbum(_album.WorkId);

                NewReviewContent = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leaving review: {ex.Message}");
            }
        }

        // Properties
        public string AlbumName
        {
            get => _albumName;
            set
            {
                _albumName = value;
                OnPropertyChanged();
            }
        }

        public DateTime PublishDate
        {
            get => _publishDate;
            set
            {
                _publishDate = value;
                OnPropertyChanged();
            }
        }

        public string PublishYear => PublishDate.Year.ToString();

        public string AlbumDescription
        {
            get => _albumDescription;
            set
            {
                _albumDescription = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Work> Songs
        {
            get => _songs;
            set
            {
                _songs = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AlbumArtist> Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                OnPropertyChanged();
            }
        }

        public string ArtistsText
        {
            get
            {
                if (Artists == null || Artists.Count == 0)
                    return "Nepoznat izvođač";

                if (Artists.Count == 1)
                    return Artists[0].ArtistName;

                if (Artists.Count == 2)
                    return $"{Artists[0].ArtistName} i {Artists[1].ArtistName}";

                return $"{Artists[0].ArtistName} i drugi ({Artists.Count})";
            }
        }

        public decimal? AverageRating
        {
            get => _averageRating;
            set
            {
                _averageRating = value;
                OnPropertyChanged();
            }
        }

        public ReviewWithRating? EditorReviewWithRating
        {
            get => _editorReviewWithRating;
            set
            {
                _editorReviewWithRating = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Rating> UserRatings
        {
            get => _userRatings;
            set
            {
                _userRatings = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Review> UserReviews
        {
            get => _userReviews;
            set
            {
                _userReviews = value;
                OnPropertyChanged();
            }
        }

        public decimal NewRatingValue
        {
            get => _newRatingValue;
            set
            {
                _newRatingValue = value;
                OnPropertyChanged();
            }
        }

        public string NewReviewContent
        {
            get => _newReviewContent;
            set
            {
                _newReviewContent = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LeaveRatingCommand { get; }
        public ICommand LeaveReviewCommand { get; }
        public ICommand NavigateToSongCommand { get; }
        public ICommand NavigateToArtistCommand { get; }

        // Events for navigation
        public event EventHandler<SongPageViewModel>? SongNavigationRequested;
        public event EventHandler<ArtistPageViewModel>? ArtistNavigationRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for album artists
    public class AlbumArtist : INotifyPropertyChanged
    {
        private string _artistId;
        private string _artistName;

        public string ArtistId
        {
            get => _artistId;
            set
            {
                _artistId = value;
                OnPropertyChanged();
            }
        }

        public string ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}