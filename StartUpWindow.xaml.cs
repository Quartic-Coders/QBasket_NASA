/// Start Up Window Class
/// Start up class gets the elements required
///     for starting QBasket; in particular
///     the projection system. Values are
///     used by Main Window.
/// Window Elements:
/// WMS Service Title (set by Main)
/// Latency (Uin) - hard-coded latency options
/// Projection (Uin) - hard-coded projection options
/// Continue Btn - oopens
/// Exit Btn
/// 
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace QBasket_demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartUpWindow : Window
    {

        // Initialize method
        public StartUpWindow()
        {
            InitializeComponent();
        }

        // Window View model
        public class Startup_VM : INotifyPropertyChanged
        {
            // Field variables
            string latency;
            string projection;

            // Methods
            // Constructor Declaration of Class 
            public Startup_VM(string latency, string projection)
            {
                this.latency = latency;
                this.projection = projection;
            }   // end Constructor

            public string getLatency()
            { return latency; }
            public string getProjection()
            { return projection; }

            /// Register callback handler for property changes
            public event PropertyChangedEventHandler PropertyChanged;

            // Standard event parser - call property's PropertyChangedEventArgs
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.
                      Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

        }   // end StartUpVM


        // Close window and continue program
        private void ContinueButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Close window and exit program
        private void QuitButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}