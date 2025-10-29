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
using System.Windows.Shapes;

namespace MetaTune.View
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Prevent user from closing manually
            if (!_allowClose)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private bool _allowClose = false;

        public void SafeClose()
        {
            _allowClose = true;
            Close();
        }
    }
}
