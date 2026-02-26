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
            MainMenuUserControl.StartDrawingClicked += OnStartDrawingClicked;
        }

        private void Initialize()
        {
            Eszkozok.Initialize();
            Eszkozok.Tipus[] toSkip = { Eszkozok.Tipus.Shrink, Eszkozok.Tipus.Grow, Eszkozok.Tipus.Szin, Eszkozok.Tipus.Paletta };
            foreach (Eszkozok e in Eszkozok.ToolContainer)
            {
                if (toSkip.Contains(e._tipus)) continue;
                this.toolBar.Children.Add(e);
            }
            Eszkozok.ToolContainer.Where(x => x._tipus == Eszkozok.Tipus.Toll).First().CreateToolBar();
        }
        private void OnStartDrawingClicked(object sender, EventArgs e)
        {
            MainMenuOverlay.Visibility = Visibility.Collapsed;
            ToolContainer.Visibility = Visibility.Visible;
            DrawingArea.Visibility = Visibility.Visible;
        }
        public void SetTheme(string themeName)
        {
            Color bg;
            Color fg;
            switch (themeName.ToLower())
            {
                case "fekete":
                    bg = Colors.Black;
                    fg = Colors.White;
                    break;
                case "fehér":
                    bg = Colors.White;
                    fg = Colors.Black;
                    break;
                default:
                    bg = Color.FromRgb(21, 26, 40);
                    fg = Color.FromRgb(179, 185, 200);
                    break;
            }
            Background = new SolidColorBrush(bg);
            Foreground = new SolidColorBrush(fg);
            this.MainContainer.Background = Background;
            this.ToolContainer.Background = Background;
            this.MainMenuOverlay.Background = Background;
        }

    }
}
