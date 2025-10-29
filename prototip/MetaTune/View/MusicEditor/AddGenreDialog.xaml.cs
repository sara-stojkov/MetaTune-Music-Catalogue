using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Core.Model;
using Core.Storage;
using Task = System.Threading.Tasks.Task;

namespace MetaTune.View.MusicEditor
{
    public partial class AddGenreDialog : Window
    {
        private readonly IGenreStorage _genreStorage;

        public AddGenreDialog()
        {
            InitializeComponent();
            _genreStorage = Injector.CreateInstance<IGenreStorage>();
            Loaded += async (s, e) => await LoadGenres();
        }

        private async Task LoadGenres()
        {
            var genres = await _genreStorage.GetAll();

            GenreComboBox.Items.Clear();

            var noneItem = new ComboBoxItem { Content = "Nema nadređenog", Tag = null };
            GenreComboBox.Items.Add(noneItem);

            foreach (var genre in genres)
            {
                var item = new ComboBoxItem { Content = genre.Name, Tag = genre.Id };
                GenreComboBox.Items.Add(item);
            }

            GenreComboBox.SelectedIndex = 0;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var genreName = GenreNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(genreName))
            {
                MessageBox.Show("Morate uneti naziv žanra!", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = GenreComboBox.SelectedItem as ComboBoxItem;
            var parentGenreId = selectedItem?.Tag as string;

            var newGenre = new Genre(genreName)
            {
                ParentGenreId = parentGenreId ?? string.Empty
            };

            await _genreStorage.CreateOne(newGenre);

            MessageBox.Show("Žanr uspešno dodat!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}