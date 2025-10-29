using Core.Model;
using MetaTune.View.Auth;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Loaded += (_, _) => ChangeTitleBarColor();
            Loaded += (_, _) => Navigate(new LoginPage());
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
        private void ChangeTitleBarColor()
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            // Attribute 35 = DWMWA_CAPTION_COLOR
            int attribute = 35;
            uint color = 0; // ARGB (here blue, same as Windows accent)
            DwmSetWindowAttribute(hwnd, attribute, ref color, sizeof(uint));

            // Optional: set text color (DWMWA_TEXT_COLOR = 36)
            uint textColor = 0xFFFFFFFF; // white text
            DwmSetWindowAttribute(hwnd, 36, ref textColor, sizeof(uint));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref uint attrValue, int attrSize);

    }
}