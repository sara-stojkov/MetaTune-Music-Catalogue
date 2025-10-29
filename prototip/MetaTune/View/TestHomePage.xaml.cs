using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetaTune.View
{
    /// <summary>
    /// Interaction logic for TestHomePage.xaml
    /// </summary>
    public partial class TestHomePage : Page
    {
        public TestHomePage()
        {
            InitializeComponent();
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            Display.Content = $"{MainWindow.LoggedInUser.Name} {MainWindow.LoggedInUser.Surname}";
        }
    }
}
