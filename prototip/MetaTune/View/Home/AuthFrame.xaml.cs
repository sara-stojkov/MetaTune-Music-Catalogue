using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MetaTune.View.Home
{
    public partial class AuthFrame : Window
    {
        public AuthFrame()
        {
            InitializeComponent();
            Closed += (_, _) => MainWindow.Instance.Focus();
        }

        // Helper to load a given Page (LoginPage, RegisterPage, etc.)
        public void NavigateTo(Page page)
        {
            AuthFrameContent.Navigate(page);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

    }
}
