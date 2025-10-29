using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetaTune.ViewModel.Auth;

namespace MetaTune.View.Auth
{
    public partial class LoginPage : Page
    {
        public LoginPage(Window win)
        {
            InitializeComponent();
            LoginPageModel model = new(NavigationService, win);
            DataContext = model;
            model.ClearPasswordRequested += OnClearPasswordRequested;
            WelcomeLabel.Text = "Dobrodošli";
            win.Title = "Prijava | Meta Tune";
            this.win = win;
        }
        private readonly Window win;

        private void SignUpHyperlink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage(win));
        }

        private void OnClearPasswordRequested()
        {
            PasswordBox.Password = string.Empty;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginPageModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
    }
}