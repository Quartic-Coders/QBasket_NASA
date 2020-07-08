using System.Windows;

namespace QBasket_demo
{
    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
        }


        // Default window closing for F4 and titlebar closing
        private void QBasketPBWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
        }   // end QBasketFormatWin_Closing
    }
}
