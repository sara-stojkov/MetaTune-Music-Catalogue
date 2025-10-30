
using Core.Model;
using MetaTune.ViewModel.Admin;
using System.Windows;
using System.Windows.Controls;

namespace MetaTune.View.MusicEditor
{
    public partial class AddAlbumDialog:Window
    {
        public AddAlbumDialog()
        {
            InitializeComponent();

            DataContext = new AddAlbumViewModel();
        }

        private void AuthorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AddAlbumViewModel vm)
            {
                vm.SelectedAuthors.Clear();
                foreach (Author a in ((ListBox)sender).SelectedItems)
                    vm.SelectedAuthors.Add(a);
            }
        }
    }
}
