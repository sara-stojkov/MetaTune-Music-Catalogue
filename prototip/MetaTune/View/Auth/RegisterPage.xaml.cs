using MetaTune.ViewModel.Auth;
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

namespace MetaTune.View.Auth
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            var model = new RegisterPageModel();
            DataContext = model;
            model.ClearPasswordRequested += OnClearPasswordRequested;
            MainWindow.Instance.Title = "Registracija | Meta Tune";
        }

        private void SignUpHyperlink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }

        private void OnClearPasswordRequested()
        {
            PasswordBox.Password = string.Empty;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterPageModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
        private void PasswordBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { /* no-op */ }
        private void PasswordBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { /* no-op */ }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}