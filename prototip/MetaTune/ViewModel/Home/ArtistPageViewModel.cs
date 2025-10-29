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
    public class ArtistPageViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;
        private readonly User _currentUser;

        private Author? _author;
        private string _artistName = string.Empty;
        private string _artistBiography = string.Empty;
        private decimal? _averageRating;
        private ObservableCollection<Review> _userReviews = new();
        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;

        public ArtistPageViewModel(
            IAuthorStorage authorStorage,
            IRatingStorage ratingStorage,
            IReviewStorage reviewStorage,
            User currentUser)
        {
            _authorStorage = authorStorage;
            _ratingStorage = ratingStorage;
            _reviewStorage = reviewStorage;
            _currentUser = currentUser;

            LeaveRatingCommand = new AsyncRelayCommand(LeaveRating);
            LeaveReviewCommand = new AsyncRelayCommand(LeaveReview);
        }

        public async System.Threading.Tasks.Task LoadArtist(string authorId)
        {
            try
            {
                _author = await _authorStorage.GetById(authorId);

                if (_author == null)
                    return;

                ArtistName = _author.AuthorName ?? string.Empty;
                ArtistBiography = _author.Biography ?? string.Empty;

                // Load ratings and calculate average
                var ratings = await _ratingStorage.GetAllByAuthorId(authorId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Load reviews
                var reviews = await _reviewStorage.GetAllByAuthorId(authorId);
                UserReviews = new ObservableCollection<Review>(reviews.Where(r => !r.IsEditable));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading artist: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LeaveRating()
        {
            if (_author == null || _newRatingValue < 1 || _newRatingValue > 5)
                return;

            try
            {
                var rating = new Rating(
                    ratingId: Guid.NewGuid().ToString(),
                    value: _newRatingValue,
                    ratingDate: DateTime.Now,
                    userId: _currentUser.Id,
                    authorId: _author.AuthorId
                );

                await _ratingStorage.CreateOne(rating);

                // Reload ratings to update average
                var ratings = await _ratingStorage.GetAllByAuthorId(_author.AuthorId);
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
            if (_author == null || string.IsNullOrWhiteSpace(_newReviewContent))
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
                    authorId: _author.AuthorId
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

        public string ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
                OnPropertyChanged();
            }
        }

        public string ArtistBiography
        {
            get => _artistBiography;
            set
            {
                _artistBiography = value;
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