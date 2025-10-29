using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace MetaTune
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // prvo hookaš handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);

            try
            {
                MetaTune.MainWindow.Instance.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // sad će svaki exception iz UI thread-a doći ovde umjesto da WPF zatvori stranicu
            MessageBox.Show("Unhandled exception: " + e.Exception.Message + "\n\n" + e.Exception.StackTrace,
                            "Unhandled Exception",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

            // Ako želiš da app ne puca, postavi Handled = true
            e.Handled = true;
        }
    }

}
