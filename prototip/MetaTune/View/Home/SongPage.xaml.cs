using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MetaTune.ViewModel.Home;

namespace MetaTune.View.Home
{
    public partial class SongPage : Page
    {
        public SongPage()
        {
            InitializeComponent();
        }

        public SongPage(SongPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        //private void PlayButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AudioPlayer.Play();
        //}

        //private void PauseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AudioPlayer.Pause();
        //}

        //private void StopButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AudioPlayer.Stop();
        //}

        //private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (AudioPlayer != null)
        //    {
        //        AudioPlayer.Volume = VolumeSlider.Value;
        //    }
        //}
    }
}
