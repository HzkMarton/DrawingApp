using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            PauseMenuUserControl.ResumeClicked += OnResumeClicked;
            PauseMenuUserControl.BackToMainMenuClicked += OnBackToMainMenuClicked;
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
        // MainMenu → Canvas
        private void OnStartDrawingClicked(object sender, EventArgs e)
        {
            MainMenuOverlay.Visibility = Visibility.Collapsed;
            ToolContainer.Visibility = Visibility.Visible;
            DrawingArea.Visibility = Visibility.Visible;
        }

        // PauseMenu → Canvas
        private void OnResumeClicked(object sender, EventArgs e)
        {
            MainMenuOverlay.Visibility = Visibility.Collapsed;
        }

        // PauseMenu → MainMenu
        private void OnBackToMainMenuClicked(object sender, EventArgs e)
        {
            MainWindow.ink.Strokes.Clear();
            PauseMenuUserControl.Visibility = Visibility.Collapsed;
            MainMenuUserControl.Visibility = Visibility.Visible;
            MainMenuOverlay.Visibility = Visibility.Visible;
            ToolContainer.Visibility = Visibility.Collapsed;
            DrawingArea.Visibility = Visibility.Collapsed;
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
        // Escape
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (MainMenuOverlay.Visibility == Visibility.Visible)
                {
                    if (PauseMenuUserControl.Visibility == Visibility.Visible)
                        OnResumeClicked(this, EventArgs.Empty);
                }
                else
                {
                    // Show
                    MainMenuUserControl.Visibility = Visibility.Collapsed;
                    PauseMenuUserControl.Visibility = Visibility.Visible;
                    MainMenuOverlay.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
