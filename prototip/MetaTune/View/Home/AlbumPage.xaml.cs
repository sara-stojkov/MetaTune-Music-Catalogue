using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MetaTune.ViewModel.Home;

public partial class AlbumPage : Page
{
	public AlbumPage()
	{
		InitializeComponent();
	}

	public AlbumPage(AlbumPageViewModel viewModel) : this()
	{
		DataContext = viewModel;
	}
}