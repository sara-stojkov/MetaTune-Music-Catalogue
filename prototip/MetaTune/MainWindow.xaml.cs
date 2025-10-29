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
using Core.Model;
using MetaTune.View.Auth;

namespace MetaTune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow? _instance = null;
        public static User? LoggedInUser { get; set; } = null; 
        private MainWindow()
        {
            InitializeComponent();
            Navigate(new LoginPage());
        }

        public void Navigate(Page page)
        {
            MainFrame.Navigate(page);
        }

        public static MainWindow Instance {
            get {
                _instance ??= new MainWindow();
                return _instance!;
            }
        } 
    } 
}