

using System.Windows;
using System.Windows.Controls;

namespace MetaTune.View.Home
{
    public partial class SearchHomePage : Page
    {
        string searchQuery;
        public SearchHomePage(string searchQuery)
        {
            MessageBox.Show(searchQuery);
        }
    }
}
