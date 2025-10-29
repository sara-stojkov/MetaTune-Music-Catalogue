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
    public class SongPageViewModel : INotifyPropertyChanged
    {
        private readonly IWorkStorage _workStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;
        private readonly User _currentUser;

        private Work? _song;
        private string _songName = string.Empty;
        private string _artistNames = string.Empty;
        private DateTime _publishDate;
        private string _songDescription = string.Empty;
        private string _lyrics = string.Empty;
        private decimal? _averageRating;
        private ObservableCollection<Review> _userReviews = new();
        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;

        public SongPageViewModel(
            IWorkStorage workStorage,
            IRatingStorage ratingStorage,
            IReviewStorage reviewStorage,
            User currentUser)
        {
            _workStorage = workStorage;
            _ratingStorage = ratingStorage;
            _reviewStorage = reviewStorage;
            _currentUser = currentUser;

            LeaveRatingCommand = new AsyncRelayCommand(LeaveRating);
            LeaveReviewCommand = new AsyncRelayCommand(LeaveReview);
        }

        public async System.Threading.Tasks.Task LoadSong(string songId)
        {
            try
            {
                _song = await _workStorage.GetById(songId);

                if (_song == null)
                    return;

                SongName = _song.WorkName;
                PublishDate = _song.PublishDate;
                SongDescription = _song.WorkDescription ?? string.Empty;
                Lyrics = _song.Src ?? string.Empty;

                // Get artist names from authors
                if (_song.Authors != null && _song.Authors.Any())
                {
                    ArtistNames = string.Join(", ", _song.Authors.Select(a => a.AuthorName ?? "Unknown"));
                }

                // Load ratings and calculate average
                var ratings = await _ratingStorage.GetAllByWorkId(songId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Load reviews
                var reviews = await _reviewStorage.GetAllByWorkId(songId);
                UserReviews = new ObservableCollection<Review>(reviews.Where(r => !r.IsEditable));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading song: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LeaveRating()
        {
            if (_song == null || _newRatingValue < 1 || _newRatingValue > 5)
                return;

            try
            {
                var rating = new Rating(
                    ratingId: Guid.NewGuid().ToString(),
                    value: _newRatingValue,
                    ratingDate: DateTime.Now,
                    userId: _currentUser.Id,
                    workId: _song.WorkId
                );

                await _ratingStorage.CreateOne(rating);

                // Reload ratings to update average
                var ratings = await _ratingStorage.GetAllByWorkId(_song.WorkId);
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
            if (_song == null || string.IsNullOrWhiteSpace(_newReviewContent))
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
                    workId: _song.WorkId
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

        public string SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                OnPropertyChanged();
            }
        }

        public string ArtistNames
        {
            get => _artistNames;
            set
            {
                _artistNames = value;
                OnPropertyChanged();
            }
        }

        public string SongNameWithArtists => string.IsNullOrEmpty(ArtistNames)
            ? SongName
            : $"{SongName} - {ArtistNames}";

        public DateTime PublishDate
        {
            get => _publishDate;
            set
            {
                _publishDate = value;
                OnPropertyChanged();
            }
        }

        public string SongDescription
        {
            get => _songDescription;
            set
            {
                _songDescription = value;
                OnPropertyChanged();
            }
        }

        public string Lyrics
        {
            get => _lyrics;
            set
            {
                _lyrics = value;
                OnPropertyChanged();
            }
        }

        public string DescriptionAndLyrics
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(SongDescription))
                    parts.Add(SongDescription);
                if (!string.IsNullOrWhiteSpace(Lyrics))
                    parts.Add(Lyrics);
                return string.Join("\n\n", parts);
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