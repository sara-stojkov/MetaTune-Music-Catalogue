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
    public partial class VerificationPage : Page
    {
        public VerificationPage()
        {
            InitializeComponent();
        }

        private void SignUpHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Title = "Register | MetaTune";
            NavigationService?.Navigate(new RegisterPage());
        }

        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}