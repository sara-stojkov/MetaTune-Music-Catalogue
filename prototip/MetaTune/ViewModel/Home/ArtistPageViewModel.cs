using Core.Model;
using Core.Storage;
using PostgreSQLStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MetaTune.ViewModel.Home
{
    public class ArtistPageViewModel : INotifyPropertyChanged
    {
        private readonly IAuthorStorage _authorStorage;
        private readonly IRatingStorage _ratingStorage;
        private readonly IReviewStorage _reviewStorage;
        private readonly IWorkStorage _workStorage;
        private readonly IGenreStorage _genreStorage;
        private readonly IMemberStorage _memberStorage;
        private readonly User _currentUser;

        private Author? _author;
        private string _artistName = string.Empty;
        private string _artistBiography = string.Empty;
        private string _genreName = string.Empty;
        private decimal? _averageRating;

        // Editor review with rating (highlighted)
        private ReviewWithRating? _editorReviewWithRating;

        // User ratings (not editor)
        private ObservableCollection<Rating> _userRatings = new();

        // User reviews (not editor)
        private ObservableCollection<Review> _userReviews = new();

        // Songs
        private ObservableCollection<Work> _songs = new();

        // Bands/Groups information
        private ObservableCollection<BandMembership> _bandMemberships = new();
        private ObservableCollection<BandMember> _bandMembers = new();
        private bool _isGroup = false;

        private decimal _newRatingValue = 0;
        private string _newReviewContent = string.Empty;

        public ArtistPageViewModel(
            string authorId,
            User currentUser)
        {
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _ratingStorage = Injector.CreateInstance<IRatingStorage>();
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            _memberStorage = Injector.CreateInstance<IMemberStorage>();
            _currentUser = currentUser;

            LeaveRatingCommand = new AsyncRelayCommand(LeaveRating);
            LeaveReviewCommand = new AsyncRelayCommand(LeaveReview);
            NavigateToSongCommand = new RelayCommand(async (param) => await NavigateToSong(param));
            NavigateToArtistCommand = new RelayCommand(async (param) => await NavigateToArtist(param));
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

                // Load songs by this artist
                var works = await _workStorage.GetAllByAuthorId(authorId);
                var songs = works.Where(w => w.WorkType == WorkType.Song).ToList();
                Songs = new ObservableCollection<Work>(songs);

                // Get genre from first song (assuming all songs have same genre)
                if (songs.Any())
                {
                    var genre = await _genreStorage.GetById(songs.First().GenreId);
                    GenreName = genre?.Name ?? string.Empty;
                }

                // Load ratings
                var ratings = await _ratingStorage.GetAllByAuthorId(authorId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Load all reviews
                var allReviews = await _reviewStorage.GetAllByAuthorId(authorId);

                // Get THE editor review (should be only one primary editor review)
                var editorReview = allReviews.FirstOrDefault(r => r.IsEditable);
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
                var userReviewsList = allReviews.Where(r => !r.IsEditable).ToList();
                UserReviews = new ObservableCollection<Review>(userReviewsList);

                // Get user ratings (not editor, excluding the editor's rating)
                var editorUserId = editorReview?.UserId;
                var userRatingsList = ratings.Where(r => r.UserId != editorUserId).ToList();
                UserRatings = new ObservableCollection<Rating>(userRatingsList);

                // Load bands/groups information
                await LoadBandsAndMembers(authorId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading artist: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadBandsAndMembers(string authorId)
        {
            try
            {
                // Check if this artist is a member of any bands
                var memberships = await _memberStorage.GetAllMembersByMemberId(authorId);

                if (memberships != null && memberships.Any())
                {
                    var bandMembershipList = new List<BandMembership>();

                    foreach (var membership in memberships)
                    {
                        var band = await _authorStorage.GetById(membership.GroupId);
                        if (band != null)
                        {
                            bandMembershipList.Add(new BandMembership
                            {
                                BandId = band.AuthorId,
                                BandName = band.AuthorName ?? "Nepoznat bend",
                                JoinDate = membership.JoinDate,
                                LeaveDate = membership.LeaveDate,
                                IsCurrent = membership.LeaveDate == null
                            });
                        }
                    }

                    BandMemberships = new ObservableCollection<BandMembership>(
                        bandMembershipList.OrderByDescending(b => b.IsCurrent)
                                         .ThenByDescending(b => b.JoinDate));
                }

                // Check if this artist is a band (has members)
                var members = await _memberStorage.GetAllMembersPresentByAuthorId(authorId);
                var allMembers = await _memberStorage.GetAllMembersAllTimeByAuthorId(authorId);

                if (allMembers != null && allMembers.Any())
                {
                    IsGroup = true;
                    var bandMemberList = new List<BandMember>();

                    foreach (var member in allMembers)
                    {
                        var memberAuthor = await _authorStorage.GetById(member.MemberId);
                        if (memberAuthor != null)
                        {
                            bandMemberList.Add(new BandMember
                            {
                                MemberId = memberAuthor.AuthorId,
                                MemberName = memberAuthor.AuthorName ?? "Nepoznat član",
                                JoinDate = member.JoinDate,
                                LeaveDate = member.LeaveDate,
                                IsCurrent = member.LeaveDate == null
                            });
                        }
                    }

                    BandMembers = new ObservableCollection<BandMember>(
                        bandMemberList.OrderByDescending(m => m.IsCurrent)
                                     .ThenByDescending(m => m.JoinDate));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bands and members: {ex.Message}");
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

                    // Navigation will be handled in the View (code-behind)
                    // This just prepares the data
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
            if (parameter is BandMember bandMember)
            {
                try
                {
                    var artistViewModel = new ArtistPageViewModel(bandMember.MemberId, _currentUser);
                    await artistViewModel.LoadArtist(bandMember.MemberId);
                    // Navigation will be handled in the View (code-behind)
                    // This just prepares the data
                    ArtistNavigationRequested?.Invoke(this, artistViewModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error navigating to artist: {ex.Message}");
                }
            }
            else if (parameter is BandMembership bandMembership)
            {
                try
                {
                    var artistViewModel = new ArtistPageViewModel(bandMembership.BandId, _currentUser);
                    await artistViewModel.LoadArtist(bandMembership.BandId);
                    // Navigation will be handled in the View (code-behind)
                    // This just prepares the data
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
            if (_currentUser == null)
            {
                MessageBox.Show("Morate biti prijavljeni da biste ostavili ocjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

                // Reload ratings to update average and list
                var ratings = await _ratingStorage.GetAllByAuthorId(_author.AuthorId);
                if (ratings.Any())
                {
                    AverageRating = ratings.Average(r => r.Value);
                }

                // Update user ratings list (excluding editor's rating)
                var editorUserId = EditorReviewWithRating?.Review?.UserId;
                var userRatingsList = ratings.Where(r => r.UserId != editorUserId).ToList();
                UserRatings = new ObservableCollection<Rating>(userRatingsList);

                NewRatingValue = 0;
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
                MessageBox.Show("Morate biti prijavljeni da biste ostavili ocjenu.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        // Properties
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

        public string GenreName
        {
            get => _genreName;
            set
            {
                _genreName = value;
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

        public ObservableCollection<Work> Songs
        {
            get => _songs;
            set
            {
                _songs = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BandMembership> BandMemberships
        {
            get => _bandMemberships;
            set
            {
                _bandMemberships = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BandMember> BandMembers
        {
            get => _bandMembers;
            set
            {
                _bandMembers = value;
                OnPropertyChanged();
            }
        }

        public bool IsGroup
        {
            get => _isGroup;
            set
            {
                _isGroup = value;
                OnPropertyChanged();
            }
        }

        public bool HasBandMemberships => BandMemberships?.Count > 0;
        public bool HasBandMembers => BandMembers?.Count > 0;

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

        public event EventHandler<SongPageViewModel>? SongNavigationRequested;

        public event EventHandler<ArtistPageViewModel>? ArtistNavigationRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class to combine Review with its Rating
    public class ReviewWithRating : INotifyPropertyChanged
    {
        private Review _review;
        private decimal? _rating;

        public Review Review
        {
            get => _review;
            set
            {
                _review = value;
                OnPropertyChanged();
            }
        }

        public decimal? Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for band membership information
    public class BandMembership : INotifyPropertyChanged
    {
        private string _bandId;
        private string _bandName;
        private DateTime _joinDate;
        private DateTime? _leaveDate;
        private bool _isCurrent;

        public string BandId
        {
            get => _bandId;
            set
            {
                _bandId = value;
                OnPropertyChanged();
            }
        }

        public string BandName
        {
            get => _bandName;
            set
            {
                _bandName = value;
                OnPropertyChanged();
            }
        }

        public DateTime JoinDate
        {
            get => _joinDate;
            set
            {
                _joinDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? LeaveDate
        {
            get => _leaveDate;
            set
            {
                _leaveDate = value;
                OnPropertyChanged();
            }
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                _isCurrent = value;
                OnPropertyChanged();
            }
        }

        public string MembershipPeriod
        {
            get
            {
                if (IsCurrent)
                    return $"{JoinDate.Year} - Danas";
                else
                    return $"{JoinDate.Year} - {LeaveDate?.Year}";
            }
        }

        public string StatusText => IsCurrent ? "Aktivan član" : "Bivši član";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for band member information
    public class BandMember : INotifyPropertyChanged
    {
        private string _memberId;
        private string _memberName;
        private DateTime _joinDate;
        private DateTime? _leaveDate;
        private bool _isCurrent;

        public string MemberId
        {
            get => _memberId;
            set
            {
                _memberId = value;
                OnPropertyChanged();
            }
        }

        public string MemberName
        {
            get => _memberName;
            set
            {
                _memberName = value;
                OnPropertyChanged();
            }
        }

        public DateTime JoinDate
        {
            get => _joinDate;
            set
            {
                _joinDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? LeaveDate
        {
            get => _leaveDate;
            set
            {
                _leaveDate = value;
                OnPropertyChanged();
            }
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                _isCurrent = value;
                OnPropertyChanged();
            }
        }

        public string MembershipPeriod
        {
            get
            {
                if (IsCurrent)
                    return $"{JoinDate.Year} - Danas";
                else
                    return $"{JoinDate.Year} - {LeaveDate?.Year}";
            }
        }

        public string StatusText => IsCurrent ? "Aktivan" : "Bivši";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}