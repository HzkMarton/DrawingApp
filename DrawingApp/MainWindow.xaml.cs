using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawingApp
{
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

            // ✅ NEW: subscribe to the UserControl's event
            MainMenuUserControl.StartDrawingClicked += OnStartDrawingClicked;
        }

        private void Initialize()
        {
            Eszkozok.Initialize();
            foreach (Eszkozok e in Eszkozok.ToolContainer)
            {
                if (e._tipus == Eszkozok.Tipus.Szin) continue;
                this.toolBar.Children.Add(e);
            }
            Eszkozok.ToolContainer
                     .Where(x => x._tipus == Eszkozok.Tipus.Toll)
                     .First()
                     .CreateToolBar();
        }

        // ✅ NEW: hide the overlay, reveal the drawing UI
        private void OnStartDrawingClicked(object sender, EventArgs e)
        {
            MainMenuOverlay.Visibility = Visibility.Collapsed;
            ToolContainer.Visibility = Visibility.Visible;
            DrawingArea.Visibility = Visibility.Visible;
        }
        public void SetTheme(string themeName)
        {
            var (bg, fg) = themeName.ToLower() switch
            {
                "fekete" => (Colors.Black, Colors.White),
                "fehér" => (Colors.White, Colors.Black),
                _ => (Color.FromRgb(21, 26, 40), Color.FromRgb(179, 185, 200))
            };
            Background = new SolidColorBrush(bg);
            Foreground = new SolidColorBrush(fg);
            this.MainContainer.Background = Background;
            this.ToolContainer.Background = Background;
            this.MainMenuOverlay.Background = Background;
        }

    }
}
