using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Model;
using Core.Storage;
using MetaTune.View.Home;
using MetaTune.ViewModel.Home;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.View
{
    public partial class SearchHomePage : Page
    {
        private readonly IWorkStorage _workStorage;
        private readonly IAuthorStorage _authorStorage;
        private readonly IGenreStorage _genreStorage;
        private ObservableCollection<SearchResultViewModel> _searchResults;
        private string _currentSearchQuery;
        private List<Work> _allWorks;
        private List<Author> _allAuthors;
        private List<Genre> _allGenres;

        public SearchHomePage(string searchQuery = "")
        {
            InitializeComponent();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            _currentSearchQuery = searchQuery;
            SearchTextBox.Text = searchQuery;

            _ = LoadDataAsync();
            SetupEventHandlers();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _searchResults = new ObservableCollection<SearchResultViewModel>();
                AuthorFilter authorFilter = AuthorFilter.All;
                _allWorks = await _workStorage.GetAll();
                _allAuthors = await _authorStorage.GetAll(authorFilter);
                _allGenres = await _genreStorage.GetAll();

                // Populate genre ComboBox
                PopulateGenreComboBox();

                // Perform initial search if there's a query
                if (!string.IsNullOrEmpty(_currentSearchQuery))
                {
                    PerformSearch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju podataka: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateGenreComboBox()
        {
            GenreComboBox.Items.Clear();

            // Add "All genres" option
            var allGenresItem = new ComboBoxItem
            {
                Content = "Svi žanrovi",
                Tag = null
            };
            GenreComboBox.Items.Add(allGenresItem);

            // Add all genres from database
            foreach (var genre in _allGenres.OrderBy(g => g.Name))
            {
                var item = new ComboBoxItem
                {
                    Content = genre.Name,
                    Tag = genre.Id
                };
                GenreComboBox.Items.Add(item);
            }

            // Select "All genres" by default
            GenreComboBox.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            // Handle Enter key in search box
            SearchTextBox.KeyDown += SearchTextBox_KeyDown;

            // Handle filter change events
            GenreComboBox.SelectionChanged += Filter_Changed;
            ArtistCheckBox.Checked += Filter_Changed;
            ArtistCheckBox.Unchecked += Filter_Changed;
            SongCheckBox.Checked += Filter_Changed;
            SongCheckBox.Unchecked += Filter_Changed;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _currentSearchQuery = SearchTextBox.Text?.Trim();
                PerformSearch();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _currentSearchQuery = SearchTextBox.Text?.Trim();
            PerformSearch();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                if (_allWorks == null || _allAuthors == null)
                {
                    // Data not loaded yet
                    return;
                }

                _searchResults.Clear();

                if (string.IsNullOrEmpty(_currentSearchQuery))
                {
                    ResultsTitle.Text = "Unesite pojam za pretragu";
                    SearchResultsItemsControl.ItemsSource = _searchResults;
                    return;
                }

                // Get filter values
                bool includeArtists = ArtistCheckBox.IsChecked == true;
                bool includeSongs = SongCheckBox.IsChecked == true;

                // Get selected genre ID from ComboBox Tag
                string selectedGenreId = (GenreComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
                bool filterByGenre = selectedGenreId != null;

                int yearFrom = ParseYear(YearFromTextBox.Text, 1900);
                int yearTo = ParseYear(YearToTextBox.Text, DateTime.Now.Year);

                var results = new List<SearchResultViewModel>();

                // Search in authors
                if (includeArtists)
                {
                    var authorResults = SearchAuthors(_currentSearchQuery);
                    results.AddRange(authorResults);
                }

                // Search in works
                if (includeSongs)
                {
                    var workResults = SearchWorks(_currentSearchQuery, filterByGenre ? selectedGenreId : null, yearFrom, yearTo);
                    results.AddRange(workResults);
                }

                // Sort by relevance (exact matches first, then contains)
                var sortedResults = results
                    .OrderByDescending(r => CalculateRelevanceScore(r, _currentSearchQuery))
                    .ThenBy(r => r.Title)
                    .ToList();

                foreach (var result in sortedResults)
                {
                    _searchResults.Add(result);
                }

                // Update results title
                int totalCount = _searchResults.Count;
                ResultsTitle.Text = totalCount switch
                {
                    0 => $"Nema rezultata za \"{_currentSearchQuery}\"",
                    1 => $"Pronađen 1 rezultat za \"{_currentSearchQuery}\"",
                    _ when totalCount < 5 => $"Pronađena {totalCount} rezultata za \"{_currentSearchQuery}\"",
                    _ => $"Pronađeno {totalCount} rezultata za \"{_currentSearchQuery}\""
                };

                SearchResultsItemsControl.ItemsSource = _searchResults;

                // Add click handlers
                SearchResultsItemsControl.Loaded += (s, e) =>
                {
                    AddClickHandlersToItems();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri pretrazi: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<SearchResultViewModel> SearchAuthors(string query)
        {
            var results = new List<SearchResultViewModel>();
            query = query.ToLower();

            foreach (var author in _allAuthors)
            {
                if (author.AuthorName != null && author.AuthorName.ToLower().Contains(query))
                {
                    results.Add(new SearchResultViewModel
                    {
                        Id = author.AuthorId,
                        Title = author.AuthorName ?? "Nepoznat autor",
                        Subtitle = GetAuthorSubtitle(author),
                        TypeLabel = "AUTOR",
                        TypeIcon = "👤",
                        TypeColor = "#FF2196F3",
                        AdditionalInfo = "",
                        ContentType = ContentType.Author
                    });
                }
            }

            return results;
        }

        private List<SearchResultViewModel> SearchWorks(string query, string genreId, int yearFrom, int yearTo)
        {
            var results = new List<SearchResultViewModel>();
            query = query.ToLower();

            foreach (var work in _allWorks)
            {
                // Check if title matches
                bool titleMatches = work.WorkName.ToLower().Contains(query);

                // Check if any author name matches
                bool authorMatches = false;
                if (work.Authors != null && work.Authors.Count > 0)
                {
                    authorMatches = work.Authors.Any(a =>
                        a.AuthorName != null && a.AuthorName.ToLower().Contains(query));
                }

                if (titleMatches || authorMatches)
                {
                    // Apply genre filter
                    if (genreId != null && work.GenreId != genreId)
                        continue;

                    // Apply year filter
                    int workYear = work.PublishDate.Year;
                    if (workYear < yearFrom || workYear > yearTo)
                        continue;

                    results.Add(new SearchResultViewModel
                    {
                        Id = work.WorkId,
                        Title = work.WorkName,
                        Subtitle = GetWorkSubtitle(work),
                        TypeLabel = GetWorkTypeLabel(work.WorkType),
                        TypeIcon = GetWorkTypeIcon(work.WorkType),
                        TypeColor = GetWorkTypeColor(work.WorkType),
                        AdditionalInfo = work.PublishDate.Year.ToString(),
                        ContentType = ContentType.Work,
                        WorkType = work.WorkType
                    });
                }
            }

            return results;
        }

        private int CalculateRelevanceScore(SearchResultViewModel result, string query)
        {
            int score = 0;
            string lowerQuery = query.ToLower();
            string lowerTitle = result.Title.ToLower();

            // Exact match
            if (lowerTitle == lowerQuery)
                score += 1000;
            // Starts with query
            else if (lowerTitle.StartsWith(lowerQuery))
                score += 500;
            // Contains query
            else if (lowerTitle.Contains(lowerQuery))
                score += 100;

            // Prefer shorter titles (more likely to be relevant)
            score += Math.Max(0, 100 - result.Title.Length);

            return score;
        }

        private int ParseYear(string yearText, int defaultValue)
        {
            if (int.TryParse(yearText, out int year))
            {
                if (year >= 1000 && year <= 9999)
                    return year;
            }
            return defaultValue;
        }

        private void AddClickHandlersToItems()
        {
            var itemsPresenter = FindVisualChild<ItemsPresenter>(SearchResultsItemsControl);
            if (itemsPresenter == null) return;

            for (int i = 0; i < _searchResults.Count; i++)
            {
                var container = SearchResultsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    var border = FindVisualChild<Border>(container);
                    if (border != null)
                    {
                        border.MouseLeftButtonUp += SearchResultItem_Click;
                    }
                }
            }
        }

        private void SearchResultItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is SearchResultViewModel item)
            {
                NavigateToContent(item);
            }
        }

        private async void NavigateToContent(SearchResultViewModel item)
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
                        NavigationService?.Navigate(albumPage);
                    }
                    else if (item.WorkType == WorkType.Song)
                    {
                        var songModel = new SongPageViewModel(item.Id, MainWindow.LoggedInUser);
                        await songModel.LoadSong(item.Id);
                        var songPage = new SongPage(songModel);
                        NavigationService?.Navigate(songPage);
                    }
                }
                else if (item.ContentType == ContentType.Author)
                {
                    var authorModel = new ArtistPageViewModel(item.Id, MainWindow.LoggedInUser);
                    await authorModel.LoadArtist(item.Id);
                    var authorPage = new ArtistPage(authorModel);
                    NavigationService?.Navigate(authorPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri navigaciji: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authWindow = new AuthFrame();
                authWindow.NavigateTo(new Auth.LoginPage(authWindow));
                authWindow.Owner = Application.Current.MainWindow;
                authWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri otvaranju prozora za prijavu: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper methods
        private string GetWorkSubtitle(Work work)
        {
            // Handle multiple authors
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
                authorsText = $"{work.Authors[0].AuthorName ?? "Nepoznat"} i drugi ({work.Authors.Count})";
            }

            return authorsText;
        }

        private string GetAuthorSubtitle(Author author)
        {
            int workCount = _allWorks.Count(w => w.Authors != null && w.Authors.Any(a => a.AuthorId == author.AuthorId));

            if (workCount == 1)
                return "1 delo";
            else if (workCount > 1 && workCount < 5)
                return $"{workCount} dela";
            else
                return $"{workCount} dela";
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
                WorkType.Song => "#FFFF4081",
                WorkType.Album => "#FF7B1FA2",
                _ => "#FF9E9E9E"
            };
        }

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri vraćanju nazad: {ex.Message}",
                    "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset filters to default values
            GenreComboBox.SelectedIndex = 0;
            ArtistCheckBox.IsChecked = true;
            SongCheckBox.IsChecked = true;
            YearFromTextBox.Text = "1900";
            YearToTextBox.Text = "2025";
        }

        private void NavigateToContent_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is SearchResultViewModel item)
            {
                NavigateToContent(item);
            }
        }
    }

    // ViewModel for search results
    public class SearchResultViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string TypeLabel { get; set; }
        public string TypeIcon { get; set; }
        public string TypeColor { get; set; }
        public string AdditionalInfo { get; set; }
        public ContentType ContentType { get; set; }
        public WorkType WorkType { get; set; }
    }
}