
using Core.Model;
using MetaTune.ViewModel.MusicEditor;
using System.Windows;
using System.Windows.Controls;

namespace MetaTune.View.MusicEditor
{
    public partial class AddSongDialog:Window
    {
        public AddSongDialog()
        {
            InitializeComponent();

            DataContext = new AddSongViewModel();
        }

        private void AuthorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AddSongViewModel vm)
            {
                vm.SelectedAuthors.Clear();
                foreach (Author a in ((ListBox)sender).SelectedItems)
                    vm.SelectedAuthors.Add(a);
            }
        }
    }
}
