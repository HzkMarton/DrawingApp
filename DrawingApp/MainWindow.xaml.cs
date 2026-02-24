using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DrawingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static InkCanvas ink;
        public static Border options;
        public static SolidColorBrush Background = new SolidColorBrush(Color.FromRgb(21, 26, 40));
        public static SolidColorBrush Foreground = new SolidColorBrush(Color.FromRgb(179, 185, 200));
        public MainWindow()
        {
            InitializeComponent();
            ink = this.canvas;
            options = this.Menu;
            this.ToolContainer.Background = Background;
            this.MainContainer.Background = Background;
            Initialize();
            
        }
        private void Initialize()
        {
            Eszkozok.Initialize();
            foreach (Eszkozok e in Eszkozok.ToolContainer)
            {
                if (e._tipus == Eszkozok.Tipus.Szin) continue;
                this.toolBar.Children.Add(e);
            }
        }
    }
}