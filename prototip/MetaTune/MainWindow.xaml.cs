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

namespace MetaTune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer _player = new MediaPlayer();
        private bool paused = false;
        public MainWindow()
        {
            InitializeComponent();
            PlayBackgroundMusic();
        }
        private void PlayBackgroundMusic()
        {
            var uri = new Uri("assets/ambient.mp3", UriKind.Relative);
            _player.Open(uri);
            _player.MediaEnded += (s, e) =>
            {
                _player.Position = TimeSpan.Zero; // Loop
                _player.Play();
            };
            _player.MediaOpened += (s, e) =>
            {
                _player.Play();
                paused = false;
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (paused)
            {
                _player.Play();
                paused = false;
                playPause.Content = "Pause";
            }
            else
            {
                _player.Pause();
                paused = true;
                playPause.Content = "Play";
            }
        }
    }
}