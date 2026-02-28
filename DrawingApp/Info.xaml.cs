using System.Windows;
using System.Windows.Input;

namespace DrawingApp
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        public Info()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnInfo)
            {
                btnInfo.IsEnabled = false;
                btnHelp.IsEnabled = true;
                Info1.Visibility = Visibility.Visible;
                Sugo.Visibility = Visibility.Collapsed;

            }
            else if (sender == btnHelp)
            {
                btnInfo.IsEnabled = true;
                btnHelp.IsEnabled = false;
                Info1.Visibility = Visibility.Collapsed;
                Sugo.Visibility = Visibility.Visible;
            }
        }
        private void Link_Megnyitas(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
        private void Ok_Click(object sender, MouseButtonEventArgs e) => this.Close();
    }
}
