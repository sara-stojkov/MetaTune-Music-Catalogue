using Core.Model;
using Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        private decimal? _averageRating;
        private ObservableCollection<Review> _userReviews = new();
        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;

        public AlbumPageViewModel(
            IWorkStorage workStorage,
            IRatingStorage ratingStorage,
            IReviewStorage reviewStorage,
            User currentUser)
        {
            _workStorage = workStorage;
            _ratingStorage = ratingStorage;
            _reviewStorage = reviewStorage;
            _currentUser = currentUser;

            LeaveRatingCommand = new RelayCommand(async () => await LeaveRating());
            LeaveReviewCommand = new RelayCommand(async () => await LeaveReview());
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

                // Load songs
                var songs = await _workStorage.GetAllByAlbumId(albumId);
                Songs = new ObservableCollection<Work>(songs);

                // Load ratings and calculate average
                var ratings = await _ratingStorage.GetAllByWorkId(albumId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Load reviews
                var reviews = await _reviewStorage.GetAllByWorkId(albumId);
                UserReviews = new ObservableCollection<Review>(reviews.Where(r => !r.IsEditable));
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading album: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LeaveRating()
        {
            if (_album == null || _newRatingValue < 1 || _newRatingValue > 5)
                return;

            try
            {
                var rating = new Rating(
                    ratingId: Guid.NewGuid().ToString(),
                    value: _newRatingValue,
                    ratingDate: DateTime.Now,
                    userId: _currentUser.Id,
                    workId: _album.WorkId
                );

                await _ratingStorage.CreateOne(rating);

                // Reload ratings to update average
                var ratings = await _ratingStorage.GetAllByWorkId(_album.WorkId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                NewRatingValue = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leaving rating: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LeaveReview()
        {
            if (_album == null || string.IsNullOrWhiteSpace(_newReviewContent))
                return;

            try
            {
                var review = new Review(
                    reviewId: Guid.NewGuid().ToString(),
                    content: _newReviewContent,
                    reviewDate: DateTime.Now,
                    isEditable: false,
                    editor: null,
                    userId: _currentUser.Id,
                    workId: _album.WorkId
                );

                await _reviewStorage.CreateOne(review);

                // Add to collection
                UserReviews.Add(review);

                NewReviewContent = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leaving review: {ex.Message}");
            }
        }

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

        public decimal? AverageRating
        {
            get => _averageRating;
            set
            {
                _averageRating = value;
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

        public ICommand LeaveRatingCommand { get; }
        public ICommand LeaveReviewCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}