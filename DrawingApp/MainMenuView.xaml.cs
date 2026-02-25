using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DrawingApp
{
    public partial class MainMenuView : UserControl
    {
        // Event that MainWindow subscribes to when "New Canvas" is clicked
        public event EventHandler? StartDrawingClicked;

        public MainMenuView()
        {
            InitializeComponent();
        }

        private void UjCanvas_Click(object sender, RoutedEventArgs e)
        {
            StartDrawingClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        MainWindow.ink.Strokes = new System.Windows.Ink.StrokeCollection(fs);
                    }
                    StartDrawingClicked?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt a fájl betöltésekor: " + ex.Message,
                                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Beallitasok_Click(object sender, RoutedEventArgs e)
        {
            Button? settingsButton = sender as Button;
            if (settingsButton == null) return;

            ContextMenu themeMenu = new ContextMenu();

            MenuItem blackTheme = new MenuItem { Header = "Fekete téma" };
            blackTheme.Click += (s, args) => ChangeTheme("fekete");
            themeMenu.Items.Add(blackTheme);

            MenuItem whiteTheme = new MenuItem { Header = "Fehér téma" };
            whiteTheme.Click += (s, args) => ChangeTheme("fehér");
            themeMenu.Items.Add(whiteTheme);

            MenuItem blueTheme = new MenuItem { Header = "Kék téma (Alapértelmezett)" };
            blueTheme.Click += (s, args) => ChangeTheme("kék");
            themeMenu.Items.Add(blueTheme);

            themeMenu.PlacementTarget = settingsButton;
            themeMenu.IsOpen = true;
        }

        private void ChangeTheme(string themeName)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetTheme(themeName);
            }
        }
    }
}
