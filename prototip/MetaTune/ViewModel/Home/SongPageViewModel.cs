using Core.Model;
using Core.Storage;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MetaTune.ViewModel.Home
{
    public class SongPageViewModel : INotifyPropertyChanged
    {
        private readonly IWorkStorage _workStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;
        private readonly IContributorStorage _contributorStorage;
        private readonly IPersonStorage _personStorage;
        private readonly User _currentUser;

        private Work? _song;
        private Work? _album;
        private string _songName = string.Empty;
        private string _artistNames = string.Empty;
        private string _albumName = string.Empty;
        private DateTime _publishDate;
        private string _songDescription = string.Empty;
        private string _lyrics = string.Empty;
        private decimal? _averageRating;
        private ReviewWithRating? _editorReviewWithRating;
        private ObservableCollection<Review> _userReviews = new();
        private ObservableCollection<Author> _artists = new();
        private ObservableCollection<ContributorViewModel> _contributors = new();
        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;
        private bool _isPlaying = false;
        private double _currentPosition = 0;
        private double _totalDuration = 180;
        private decimal? _userRating;

        public SongPageViewModel(string songId, User currentUser)
        {
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _ratingStorage = Injector.CreateInstance<IRatingStorage>();
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();
            _contributorStorage = Injector.CreateInstance<IContributorStorage>();
            _personStorage = Injector.CreateInstance<IPersonStorage>();
            _currentUser = currentUser;

            LeaveRatingCommand = new AsyncRelayCommand(LeaveRating);
            LeaveReviewCommand = new AsyncRelayCommand(LeaveReview);
            PlayPauseCommand = new RelayCommand(_ => TogglePlayPause());
            StopCommand = new RelayCommand(_ => Stop());
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

                // Load artists
                if (_song.Authors != null && _song.Authors.Any())
                {
                    Artists = new ObservableCollection<Author>(_song.Authors);
                    ArtistNames = string.Join(", ", _song.Authors.Select(a => a.AuthorName ?? "Unknown"));
                }

                // Load album if exists
                if (!string.IsNullOrEmpty(_song.AlbumId))
                {
                    _album = await _workStorage.GetById(_song.AlbumId);
                    if (_album != null)
                    {
                        AlbumName = _album.WorkName;
                    }
                }

                // Load contributors (production team)
                var allContributors = await _contributorStorage.GetAllByWorkId(songId);
                var contributorViewModels = new List<ContributorViewModel>();

                foreach (var contributor in allContributors)
                {
                    var person = await _personStorage.GetById(contributor.PersonId);
                    if (person != null)
                    {
                        contributorViewModels.Add(new ContributorViewModel
                        {
                            PersonId = person.PersonId,
                            PersonName = $"{person.PersonName} {person.PersonSurname}",
                            ContributionType = GetContributionTypeDisplayName(contributor.ContributionType)
                        });
                    }
                }

                Contributors = new ObservableCollection<ContributorViewModel>(
                    contributorViewModels.OrderBy(c => c.ContributionType)
                );

                // Load ratings and calculate average
                var ratings = await _ratingStorage.GetAllByWorkId(songId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Check if user already rated
                var existingUserRating = ratings.FirstOrDefault(r => r.UserId == _currentUser?.Id);
                if (existingUserRating != null)
                {
                    UserRating = existingUserRating.Value;
                    NewRatingValue = existingUserRating.Value;
                }

                // Load all reviews
                var allReviews = await _reviewStorage.GetAllApprovedByWorkId(songId);

                // Get THE editor review using dedicated method
                var editorReview = await _reviewStorage.GetEditorReviewByWorkId(songId);
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
                UserReviews = new ObservableCollection<Review>(
                    allReviews.OrderByDescending(r => r.ReviewDate)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading song: {ex.Message}");
            }
        }

        private string GetContributionTypeDisplayName(string contributionType)
        {
            return contributionType switch
            {
                "ARRANGER" => "Aranžer",
                "PRODUCER" => "Producent",
                "SOUND_ENGINEER" => "Tonski inženjer",
                "WRITER" => "Tekstopisac",
                _ => contributionType
            };
        }

        private void TogglePlayPause()
        {
            IsPlaying = !IsPlaying;
        }

        private void Stop()
        {
            IsPlaying = false;
            CurrentPosition = 0;
        }

        private async System.Threading.Tasks.Task LeaveRating()
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

            if (_song == null || _newRatingValue < 1 || _newRatingValue > 5)
                return;

            try
            {
                // Check if user already has a rating
                var ratings = await _ratingStorage.GetAllByWorkId(_song.WorkId);
                var existingRating = ratings.FirstOrDefault(r => r.UserId == _currentUser.Id);

                if (existingRating != null)
                {
                    // Update existing rating
                    existingRating.Value = _newRatingValue;
                    existingRating.RatingDate = DateTime.Now;
                    await _ratingStorage.UpdateOne(existingRating);
                }
                else
                {
                    // Create new rating
                    var rating = new Rating(
                        ratingId: Guid.NewGuid().ToString(),
                        value: _newRatingValue,
                        ratingDate: DateTime.Now,
                        userId: _currentUser.Id,
                        workId: _song.WorkId
                    );

                    await _ratingStorage.CreateOne(rating);
                }

                await LoadSong(_song.WorkId);

                UserRating = _newRatingValue;
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

            if (_song == null || string.IsNullOrWhiteSpace(_newReviewContent))
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
                    workId: _song.WorkId
                );

                await _reviewStorage.CreateOne(review);

                MessageBox.Show("Hvala što ste ostavili recenziju!", "Recenzija poslata", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadSong(_song.WorkId);

                NewReviewContent = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error leaving review: {ex.Message}");
            }
        }

        // Properties
        public Work? Song => _song;
        public Work? Album => _album;
        public bool HasAlbum => _album != null;

        public string SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SongNameWithArtists));
            }
        }

        public string ArtistNames
        {
            get => _artistNames;
            set
            {
                _artistNames = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SongNameWithArtists));
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
                OnPropertyChanged(nameof(HasDescription));
            }
        }

        public string Lyrics
        {
            get => _lyrics;
            set
            {
                _lyrics = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLyrics));
            }
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(SongDescription);
        public bool HasLyrics => !string.IsNullOrWhiteSpace(Lyrics);

        public decimal? AverageRating
        {
            get => _averageRating;
            set
            {
                _averageRating = value;
                OnPropertyChanged();
            }
        }

        public decimal? UserRating
        {
            get => _userRating;
            set
            {
                _userRating = value;
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

        public ObservableCollection<Review> UserReviews
        {
            get => _userReviews;
            set
            {
                _userReviews = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Author> Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasArtists));
            }
        }

        public bool HasArtists => Artists != null && Artists.Any();

        public ObservableCollection<ContributorViewModel> Contributors
        {
            get => _contributors;
            set
            {
                _contributors = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasContributors));
            }
        }

        public bool HasContributors => Contributors != null && Contributors.Any();

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

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayPauseButtonText));
            }
        }

        public string PlayPauseButtonText => IsPlaying ? "⏸ Pauza" : "▶ Pusti";

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPositionFormatted));
            }
        }

        public double TotalDuration
        {
            get => _totalDuration;
            set
            {
                _totalDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalDurationFormatted));
            }
        }

        public string CurrentPositionFormatted => TimeSpan.FromSeconds(CurrentPosition).ToString(@"mm\:ss");
        public string TotalDurationFormatted => TimeSpan.FromSeconds(TotalDuration).ToString(@"mm\:ss");

        public ICommand LeaveRatingCommand { get; }
        public ICommand LeaveReviewCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for contributors
    public class ContributorViewModel : INotifyPropertyChanged
    {
        private string _personId;
        private string _personName;
        private string _contributionType;

        public string PersonId
        {
            get => _personId;
            set
            {
                _personId = value;
                OnPropertyChanged();
            }
        }

        public string PersonName
        {
            get => _personName;
            set
            {
                _personName = value;
                OnPropertyChanged();
            }
        }

        public string ContributionType
        {
            get => _contributionType;
            set
            {
                _contributionType = value;
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