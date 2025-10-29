using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Model;
using Core.Storage;
using MetaTune.View.Home;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.View
{
    public partial class HomePage : Page
    {
        private readonly IWorkStorage _workStorage;
        private readonly IAuthorStorage _authorStorage;
        private ObservableCollection<ContentItemViewModel> _newContentItems;

        public HomePage()
        {
            InitializeComponent();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            SetupEventHandlers();
            _ = LoadRecentContentAsync();
        }

        private void SetupEventHandlers()
        {
            // Handle Enter key in search box
            SearchTextBox.KeyDown += SearchTextBox_KeyDown;
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

                // Debug: Show what we're loading
                int songCount = allWorks.Count(w => w.WorkType == WorkType.Song);
                int albumCount = allWorks.Count(w => w.WorkType == WorkType.Album);
                System.Diagnostics.Debug.WriteLine($"Loading: {songCount} songs, {albumCount} albums, {allAuthors.Count} authors");

                var contentList = new List<ContentItemViewModel>();

                // Strategy: Show a balanced mix of content
                // Assume newer IDs = more recently added content (common database pattern)

                // Get the most recent works (by ID, assuming sequential IDs)
                var recentWorks = allWorks
                    .OrderByDescending(w => w.WorkId)
                    .Take(12)
                    .ToList();

                // Get the most recent authors (by ID)
                var recentAuthors = allAuthors
                    .OrderByDescending(a => a.AuthorId)
                    .Take(6)
                    .ToList();

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
                        DateAdded = work.PublishDate,
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
                        DateAdded = mostRecentDate,
                        ContentType = ContentType.Author,
                        SortOrder = GetSortOrder(author.AuthorId, false) // false = is author
                    });
                }

                // Sort by our custom sort order (which prioritizes recent IDs)
                // This creates a nice mix of works and authors
                var sortedContent = contentList
                    .OrderByDescending(c => c.SortOrder)
                    .Take(18)
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

                NewContentItemsControl.ItemsSource = _newContentItems;

                // Show info message if no songs found
                if (songCount == 0 && albumCount > 0)
                {
                    MessageBox.Show($"Pronađeno: {albumCount} albuma, {allAuthors.Count} autora\n\nNapomena: Nema pojedinačnih pesama u bazi podataka.",
                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Add click event handlers to each item
                NewContentItemsControl.Loaded += (s, e) =>
                {
                    AddClickHandlersToItems();
                };
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

        private void AddClickHandlersToItems()
        {
            // Find all Border elements in the ItemsControl and add click handlers
            var itemsPresenter = FindVisualChild<ItemsPresenter>(NewContentItemsControl);
            if (itemsPresenter == null) return;

            for (int i = 0; i < _newContentItems.Count; i++)
            {
                var container = NewContentItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    var border = FindVisualChild<Border>(container);
                    if (border != null)
                    {
                        border.MouseLeftButtonUp += ContentItem_Click;
                    }
                }
            }
        }

        private void ContentItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is ContentItemViewModel item)
            {
                NavigateToContent(item);
            }
        }

        private void NavigateToContent(ContentItemViewModel item)
        {
            try
            {
                if (item.ContentType == ContentType.Work)
                {
                    if (item.WorkType == WorkType.Album)
                    {
                        var albumPage = new AlbumPage();
                        NavigationService?.Navigate(albumPage);
                    }
                    else if (item.WorkType == WorkType.Song)
                    {
                        var songPage = new SongPage();
                        NavigationService?.Navigate(songPage);
                    }
                }
                else if (item.ContentType == ContentType.Author)
                {
                    var authorDetailsPage = new ArtistPage();
                    NavigationService?.Navigate(authorDetailsPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string searchQuery = SearchTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Unesite pojam za pretragu.", "Pretraga",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var searchPage = new SearchHomePage(searchQuery);
                NavigationService?.Navigate(searchPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri pretrazi: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authWindow = new AuthFrame();
                authWindow.NavigateTo(new Auth.LoginPage());
                authWindow.Owner = Application.Current.MainWindow;
                authWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri otvaranju prozora za prijavu: {ex.Message}",
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

        // Helper method to find visual children
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
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