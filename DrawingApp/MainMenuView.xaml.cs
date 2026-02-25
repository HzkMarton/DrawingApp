using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DrawingApp
{
    public partial class MainMenuView : UserControl
    {
        // Ez az esemény jelzi, hogy a "Start Drawing" gombra kattintottak, és a rajzoló felület megjeleníthető.
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

        public void Betoltes_Click(object sender, RoutedEventArgs e)
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
        public void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        MainWindow.ink.Strokes.Save(fs);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt a fájl mentésekor: " + ex.Message,"Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Beallitasok_Click(object sender, RoutedEventArgs e)
        {
            Button? settingsButton = sender as Button;
            if (settingsButton == null) return;

            ContextMenu Beallitasok = new ContextMenu();
            MenuItem temaMenu = new MenuItem { Header = "Téma" };
            MenuItem blackTheme = new MenuItem { Header = "Fekete téma" };
            blackTheme.Click += (s, args) => ChangeTheme("fekete");
            temaMenu.Items.Add(blackTheme);

            MenuItem whiteTheme = new MenuItem { Header = "Fehér téma" };
            whiteTheme.Click += (s, args) => ChangeTheme("fehér");
            temaMenu.Items.Add(whiteTheme);

            MenuItem blueTheme = new MenuItem { Header = "Kék téma (Alapértelmezett)" };
            blueTheme.Click += (s, args) => ChangeTheme("kék");
            temaMenu.Items.Add(blueTheme);

            MenuItem info = new MenuItem { Header = "Információ" };
            info.Click += Info_Click;
            Beallitasok.Items.Add(temaMenu);
            Beallitasok.Items.Add(info);

            Beallitasok.PlacementTarget = settingsButton;
            Beallitasok.IsOpen = true;
        }
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rajzoló alkalmazás\nKészítette: RPT Industries\n2024", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private static void ChangeTheme(string themeName)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetTheme(themeName);
            }
        }
        public void Fomenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenuView mainMenu = new MainMenuView();
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Content = mainMenu;
                mainWindow.Show();
            }
        }
    }
}
