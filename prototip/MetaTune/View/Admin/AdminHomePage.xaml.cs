using Core.Storage;
using MetaTune.ViewModel.Admin;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetaTune.View.Admin
{
    /// <summary>
    /// Interaction logic for AdminHomePage.xaml
    /// </summary>
    public partial class AdminHomePage : Page
    {
        private readonly IUserStorage _userStorage;

        public AdminHomePage()
        {
            InitializeComponent();
            _userStorage = Injector.CreateInstance<IUserStorage>();
            DataContext = new AdminHomeViewModel(_userStorage);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement logout logic
            MainWindow.LoggedInUser = null;
            NavigationService?.Navigate(new HomePage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());
        }
    }
}