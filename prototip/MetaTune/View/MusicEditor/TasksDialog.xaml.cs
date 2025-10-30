using Core.Model;
using Core.Storage;
using MetaTune.View.Home;
using MetaTune.ViewModel.Home;
using PostgreSQLStorage;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using static System.Net.Mime.MediaTypeNames;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.View.MusicEditor
{
    public partial class TasksDialog : Page
    {
        private readonly IWorkStorage _workStorage;
        private readonly IAuthorStorage _authorStorage;
        private readonly ITaskStorage _taskStorage;
        private ObservableCollection<ContentItemViewModel> _newContentItems;

        private List<Core.Model.Task> Tasks {  get; set; }

        public TasksDialog()
        {
            InitializeComponent();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _taskStorage = Injector.CreateInstance<ITaskStorage>();
            _ = LoadRecentContentAsync();
        }

        private async Task LoadRecentContentAsync()
        {
            _newContentItems = new ObservableCollection<ContentItemViewModel>();

            try
            {
                AuthorFilter authorFilter = AuthorFilter.All;
                // Get all works and authors
                List<Work> allWorks = await _workStorage.GetAll();
                List<Author> allAuthors = await _authorStorage.GetAll(authorFilter);
                List<Core.Model.Task> tasks = await _taskStorage.GetAllByEditorId(MainWindow.LoggedInUser!.Id);
                tasks = tasks.Where(task => !task.Done).ToList();
                Tasks = tasks;

                allWorks = allWorks.Where(w => tasks.Any(t => t.WorkId == w.WorkId)).ToList();
                allAuthors = allAuthors.Where(a => tasks.Any(t => t.AuthorId == a.AuthorId)).ToList();

                // Debug: Show what we're loading
                int songCount = allWorks.Count(w => w.WorkType == WorkType.Song);
                int albumCount = allWorks.Count(w => w.WorkType == WorkType.Album);
                System.Diagnostics.Debug.WriteLine($"Loading: {songCount} songs, {albumCount} albums, {allAuthors.Count} authors");

                var contentList = new List<ContentItemViewModel>();

                // Strategy: Show a balanced mix of content
                // Assume newer IDs = more recently added content (common database pattern)

                // Get the most recent works (by ID, assuming sequential IDs)
                var recentWorks = allWorks;

                // Get the most recent authors (by ID)
                var recentAuthors = allAuthors;

                // Add recent works to the list
                foreach (Work work in recentWorks)
                {
                    contentList.Add(new ContentItemViewModel
                    {
                        Id = work.WorkId,
                        Title = work.WorkName,
                        Subtitle = GetWorkSubtitle(work),
                        TypeLabel = GetWorkTypeLabel(work.WorkType),
                        TypeIcon = GetWorkTypeIcon(work.WorkType),
                        TypeColor = GetWorkTypeColor(work.WorkType),
                        DateAdded = tasks.First(t => t.WorkId == work.WorkId).AssignmentDate,
                        ContentType = ContentType.Work,
                        WorkType = work.WorkType,
                        SortOrder = GetSortOrder(work.WorkId, true) // true = is work
                    });
                }

                // Add recent authors to the list
                foreach (Author author in recentAuthors)
                {
                    // Find the most recent work by this author for the subtitle
                    var authorWorks = allWorks.Where(w =>
                        w.Authors != null && w.Authors.Any(a => a.AuthorId == author.AuthorId)).ToList();

                    DateTime mostRecentDate = authorWorks.Any()
                        ? authorWorks.Max(w => w.PublishDate)
                        : DateTime.Now;

                    contentList.Add(new ContentItemViewModel
                    {
                        Id = author.AuthorId,
                        Title = author.AuthorName ?? "Unknown Author",
                        Subtitle = await GetAuthorSubtitleAsync(author, allWorks),
                        TypeLabel = "AUTOR",
                        TypeIcon = "👤",
                        TypeColor = "#FF2196F3",
                        DateAdded = tasks.First(t => t.AuthorId == author.AuthorId).AssignmentDate,
                        ContentType = ContentType.Author,
                        SortOrder = GetSortOrder(author.AuthorId, false) // false = is author
                    });
                }

                // Sort by our custom sort order (which prioritizes recent IDs)
                // This creates a nice mix of works and authors
                var sortedContent = contentList
                    .OrderByDescending(c => c.DateAdded)
                    .ToList();

                // Debug: Show what's being displayed
                foreach (var item in sortedContent)
                {
                    System.Diagnostics.Debug.WriteLine($"Displaying: {item.TypeLabel} - {item.Title} (Order: {item.SortOrder})");
                }

                foreach (var item in sortedContent)
                {
                    _newContentItems.Add(item);
                }


                // Show info message if no songs found
                if (songCount == 0 && albumCount > 0)
                {
                    MessageBox.Show($"Pronađeno: {albumCount} albuma, {allAuthors.Count} autora\n\nNapomena: Nema pojedinačnih pesama u bazi podataka.",
                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                NewContentItemsControl.ItemsSource = _newContentItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju sadržaja: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a sort order based on ID (assuming higher ID = more recent)
        // This creates an interleaved display of works and authors
        private int GetSortOrder(string id, bool isWork)
        {
            // Try to extract numeric part from ID
            // For IDs like "work_123" or "author_456", extract the number
            string numericPart = new string(id.Where(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out int numericId))
            {
                // Multiply by 10 to leave room for mixing
                // Works get even numbers, authors get odd numbers
                // This creates a nice interleaved display
                return isWork ? numericId * 10 : (numericId * 10) + 1;
            }

            // Fallback: use hash code
            return id.GetHashCode();
        }

        
        private void ContentItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is ContentItemViewModel item)
            {
                NavigateToContent(item);
            }
        }

        private async Task NavigateToContent(ContentItemViewModel item)
        {
            try
            {
                if (item.ContentType == ContentType.Work)
                {
                    if (item.WorkType == WorkType.Album)
                    {
                        var albumModel = new AlbumPageViewModel(item.Id, MainWindow.LoggedInUser);
                        await albumModel.LoadAlbum(item.Id);
                        var albumPage = new AlbumPage(albumModel);
                        NavigationService.Navigate(albumPage);
                    }
                    else if (item.WorkType == WorkType.Song)
                    {
                        var songModel = new SongPageViewModel(item.Id, MainWindow.LoggedInUser);
                        await songModel.LoadSong(item.Id);
                        var songPage = new SongPage(songModel);
                        NavigationService.Navigate(songPage);
                    }
                }
                else if (item.ContentType == ContentType.Author)
                {
                    var authorModel = new ArtistPageViewModel(item.Id, MainWindow.LoggedInUser);
                    await authorModel.LoadArtist(item.Id);
                    var authorPage = new ArtistPage(authorModel);
                    NavigationService.Navigate(authorPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper methods for work information
        private string GetWorkSubtitle(Work work)
        {
            // Handle multiple authors - Work.Authors is a List<Author>
            if (work.Authors == null || work.Authors.Count == 0)
            {
                return "Nepoznat autor";
            }

            string authorsText;
            if (work.Authors.Count == 1)
            {
                authorsText = work.Authors[0].AuthorName ?? "Nepoznat autor";
            }
            else if (work.Authors.Count == 2)
            {
                authorsText = $"{work.Authors[0].AuthorName ?? "Nepoznat"} i {work.Authors[1].AuthorName ?? "Nepoznat"}";
            }
            else
            {
                // More than 2 authors: show first author + "i drugi"
                authorsText = $"{work.Authors[0].AuthorName ?? "Nepoznat"} i drugi ({work.Authors.Count})";
            }

            // Add year from PublishDate
            string year = work.PublishDate.Year.ToString();
            return $"{authorsText} • {year}";
        }

        private string GetWorkTypeLabel(WorkType type)
        {
            return type switch
            {
                WorkType.Song => "PESMA",
                WorkType.Album => "ALBUM",
                _ => "DELO"
            };
        }

        private string GetWorkTypeIcon(WorkType type)
        {
            return type switch
            {
                WorkType.Song => "🎵",
                WorkType.Album => "💿",
                _ => "📄"
            };
        }

        private string GetWorkTypeColor(WorkType type)
        {
            return type switch
            {
                WorkType.Song => "#FFFF4081",  // Pink for songs
                WorkType.Album => "#FF7B1FA2", // Purple for albums
                _ => "#FF9E9E9E"
            };
        }

        private async Task<string> GetAuthorSubtitleAsync(Author author, List<Work> allWorks)
        {
            // Count works where this author is in the Authors list
            int workCount = allWorks.Count(w => w.Authors != null && w.Authors.Any(a => a.AuthorId == author.AuthorId));

            if (workCount == 1)
                return "1 delo";
            else if (workCount > 1 && workCount < 5)
                return $"{workCount} dela";
            else
                return $"{workCount} dela";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private async void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Da li ste sigurni da želite da završite ovaj zadatak?", "Završetak", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            var loading = new LoadingDialog()
            {
                Owner = MainWindow.Instance
            };
            loading.Show();
            try
            {
                var button = sender as Button;
                var id = button?.Tag as string;

                if (string.IsNullOrEmpty(id))
                    return;


                var task = Tasks.FirstOrDefault(t => t.WorkId == id || t.AuthorId == id);
                if (task == null) return;
                task.Done = true;
                await _taskStorage.UpdateOne(task);
                await LoadRecentContentAsync();

                //var review = await _reviewStorage.GetById(reviewId);
                //if (review == null)
                //{
                //    MessageBox.Show("Komentar nije pronađen!", "Greška",
                //        MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //// Set editor ID (UserId property maps to editor field)
                //review.UserId = _currentUser.Id; // Set editor who approved
                //review.IsEditable = true; // Mark as approved/editable

                //await _reviewStorage.UpdateOne(review);

                //// Ukloni iz liste odmah
                //var viewModel = _reviews.FirstOrDefault(r => r.ReviewId == reviewId);
                //if (viewModel != null)
                //{
                //    _reviews.Remove(viewModel);
                //    ReviewsItemsControl.ItemsSource = null;
                //    ReviewsItemsControl.ItemsSource = _reviews;
                //}

                MessageBox.Show("Zadatak završen", "Uspeh",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri završavanju zadatka: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loading.SafeClose();
            }
        }
    }

    // ViewModel for content items
    public class ContentItemViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string TypeLabel { get; set; }
        public string TypeIcon { get; set; }
        public string TypeColor { get; set; }
        public DateTime DateAdded { get; set; }
        public ContentType ContentType { get; set; }
        public WorkType WorkType { get; set; }
        public int SortOrder { get; set; }
    }

    public enum ContentType
    {
        Work,
        Author
    }
}