using Core.Model;
using Core.Storage;
using PostgreSQLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.View.MusicEditor
{
    public partial class EditorHomePage : Page
    {
        private readonly Core.Model.User _currentUser;
        private readonly IReviewStorage _reviewStorage;
        private readonly IWorkStorage _workStorage;
        private readonly IAuthorStorage _authorStorage;
        private List<ReviewViewModel> _reviews;

        public EditorHomePage()
        {
            InitializeComponent();

            _currentUser = MainWindow.LoggedInUser;
            _reviewStorage = Injector.CreateInstance<IReviewStorage>();
            _workStorage = Injector.CreateInstance<IWorkStorage>();
            _authorStorage = Injector.CreateInstance<IAuthorStorage>();
            _reviews = new List<ReviewViewModel>();

            Loaded += async (s, e) => await LoadPendingReviews();
        }

        private async Task LoadPendingReviews()
        {
            try
            {
                var pendingReviews = await _reviewStorage.GetAllPending();
                _reviews.Clear();

                foreach (var review in pendingReviews)
                {
                    string title = "Nepoznato";

                    if (review.WorkId != null)
                    {
                        var work = await _workStorage.GetById(review.WorkId);
                        title = work?.WorkName ?? "Nepoznato delo";
                    }
                    else if (review.AuthorId != null)
                    {
                        var author = await _authorStorage.GetById(review.AuthorId);
                        title = author?.AuthorName ?? "Nepoznat izvođač";
                    }

                    var viewModel = new ReviewViewModel
                    {
                        ReviewId = review.ReviewId,
                        Content = review.Content,
                        ReviewDate = review.ReviewDate,
                        WorkTitle = title
                    };

                    _reviews.Add(viewModel);
                }

                ReviewsItemsControl.ItemsSource = _reviews;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri učitavanju komentara: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void ViewTasks_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new TasksDialog());
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var reviewId = button?.Tag as string;

                if (string.IsNullOrEmpty(reviewId))
                    return;

                var review = await _reviewStorage.GetById(reviewId);
                if (review == null)
                {
                    MessageBox.Show("Komentar nije pronađen!", "Greška",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Set editor ID (UserId property maps to editor field)
                review.UserId = _currentUser.Id; // Set editor who approved
                review.IsEditable = true; // Mark as approved/editable

                await _reviewStorage.UpdateOne(review);

                // Ukloni iz liste odmah
                var viewModel = _reviews.FirstOrDefault(r => r.ReviewId == reviewId);
                if (viewModel != null)
                {
                    _reviews.Remove(viewModel);
                    ReviewsItemsControl.ItemsSource = null;
                    ReviewsItemsControl.ItemsSource = _reviews;
                }

                MessageBox.Show("Komentar je uspešno odobren!", "Uspeh",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri odobravanju komentara: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var reviewId = button?.Tag as string;

                if (string.IsNullOrEmpty(reviewId))
                    return;

                var result = MessageBox.Show(
                    "Da li ste sigurni da želite da obrišete ovaj komentar?",
                    "Potvrda brisanja",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                await _reviewStorage.DeleteById(reviewId);

                // Ukloni iz liste odmah
                var viewModel = _reviews.FirstOrDefault(r => r.ReviewId == reviewId);
                if (viewModel != null)
                {
                    _reviews.Remove(viewModel);
                    ReviewsItemsControl.ItemsSource = null;
                    ReviewsItemsControl.ItemsSource = _reviews;
                }

                MessageBox.Show("Komentar je uspešno obrisan!", "Uspeh",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri brisanju komentara: {ex.Message}", "Greška",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddGenreDialog();
            dialog.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.LoggedInUser = null;
            NavigationService?.Navigate(new HomePage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAlbumDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSongDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }
    }

    public class ReviewViewModel
    {
        public string ReviewId { get; set; }
        public string Content { get; set; }
        public DateTime ReviewDate { get; set; }
        public string WorkTitle { get; set; }
    }
}