using MetaTune.View.Auth;
using System;
using System.Windows;

namespace MetaTune.Helpers
{
    internal static class Logout
    {
        public static void Execute()
        {
            Window topLevelWindow = Application.Current.MainWindow
                ?? throw new Exception("Unable to logout");
            MainWindow mainWindow = topLevelWindow as MainWindow
                ?? throw new Exception("MainWindow is not of type MainWindow.");
            mainWindow.Title = "Login | MetaTune";
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.ResizeMode = ResizeMode.NoResize;
            mainWindow.Height = 820;
            mainWindow.Width = 420;
            mainWindow.Navigate(new LoginPage());
        }
    }
}
