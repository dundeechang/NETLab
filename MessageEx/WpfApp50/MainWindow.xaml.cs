using System.Windows;

namespace WpfApp50
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "Hello");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxEx.Show(this, "Hello");
        }
    }
}
