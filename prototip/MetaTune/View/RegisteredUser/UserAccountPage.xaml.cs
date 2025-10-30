using MetaTune.ViewModel.Auth;
using System.Windows;
using System.Windows.Controls;
using MetaTune.ViewModel.RegisteredUser;

namespace MetaTune.View.RegisteredUser
{
    public partial class UserAccountPage:Page
    {
        public UserAccountPage()
        {
            InitializeComponent();
            DataContext = new UserAccountPageModel(MainWindow.LoggedInUser!);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show("Uskoro...");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow.LoggedInUser = null;
            MainWindow.Instance.Navigate(new HomePage());
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserAccountPageModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
    }
}
