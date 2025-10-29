using Core.Controller;
using Core.Model;
using Core.Services.EmailService;
using Core.Storage;
using MetaTune.Helpers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetaTune.View.Auth
{
    public partial class VerificationPage : Page
    {
        public readonly Core.Model.User user;
        private readonly UserController userController;
        private readonly TextBox[] inputs;
        public VerificationPage(Core.Model.User user)
        {
            this.user = user;
            IUserStorage userStorage = Injector.CreateInstance<IUserStorage>();
            IGenreStorage genreStorage = Injector.CreateInstance<IGenreStorage>();
            IEmailService emailService = Injector.CreateInstance<IEmailService>();
            userController = new(userStorage, genreStorage, emailService);
            InitializeComponent();
            inputs = new TextBox[] {
                txtCode1, txtCode2,
                txtCode3, txtCode4,
                txtCode5, txtCode6
            };
            for (int i = 0; i < inputs.Length; i++)
            {
                var ind = i;
                inputs[i].KeyUp += (s, e) => input_KeyUp(ind, e);
                inputs[i].TextChanged += (s, e) => input_Change(ind, e);
            }
            inputs[0].Focus();
            MainWindow.Instance.Title = "Verifikacija | Meta Tune";
        }

        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            Verify();
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            sentEmail.Text = VerificationHelper.HideEmail(user.Email);
        }

        private void input_KeyUp(int index, KeyEventArgs e)
        {
            if (index > 0 && inputs[index].Text == string.Empty && e.Key == Key.Back)
            {
                inputs[index-1].Text = string.Empty;
                inputs[index-1].Focus();
            }
            if (index > 0 && e.Key == Key.Left)
            {
                inputs[index - 1].Focus();
            }
            if (index < inputs.Length - 1 && e.Key == Key.Right)
            {
                inputs[index + 1].Focus();
            }
        }
        private void input_Change(int index, TextChangedEventArgs e)
        {
            if (inputs[index].Text.Length > 0 && !char.IsNumber(inputs[index].Text[0]))
            {
                inputs[index].Text = string.Empty;
                return;
            }
            if (inputs[index].Text.Length > 0 && index < inputs.Length - 1)
            {
                inputs[index + 1].Focus();
            }
        }
        private async void Verify()
        {
            var loading = new LoadingDialog() { Owner = MainWindow.Instance };
            loading.Show();
            try
            {
                var builder = new StringBuilder();
                foreach (var input in inputs)
                {
                    input.IsEnabled = false;
                    if (input.Text.Length == 0) throw new Exception("Morate uneti sve cifre");
                    builder.Append(input.Text);
                }
                var verified = await userController.Verify(user, builder.ToString());
                if (!verified) throw new Exception("Pogrešan kod. Pokušajte ponovo");
                MainWindow.LoggedInUser = user;
                MainWindow.Instance.Navigate(new TestHomePage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                foreach (var input in inputs)
                {
                    input.IsEnabled = true;
                }
                loading.SafeClose();
            }
        }

        private async void resend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await userController.SendVerificationCode(user);
                MessageBox.Show("Verifikacioni kod poslat. Proveri Vaš mejl.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}