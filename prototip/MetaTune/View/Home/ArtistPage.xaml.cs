using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MetaTune.ViewModel.Home;

namespace MetaTune.View.Home
{
    public partial class ArtistPage : Page
    {
        public ArtistPage()
        {
            InitializeComponent();
        }

        public ArtistPage(ArtistPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
