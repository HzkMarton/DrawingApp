using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace DrawingApp
{
    public partial class PauseMenu : UserControl
    {
        public event EventHandler? ResumeClicked;
        public event EventHandler? BackToMainMenuClicked;

        public PauseMenu() 
        { 
            InitializeComponent(); 
        }

        private void Folytatas_Click(object sender, RoutedEventArgs e)
        {
            ResumeClicked?.Invoke(this, EventArgs.Empty);
        }

        private void Fomenu_Click(object sender, RoutedEventArgs e)
        {
            BackToMainMenuClicked?.Invoke(this, EventArgs.Empty);
        }
        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            { Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                using FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                MainWindow.ink.Strokes.Save(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message, "Hiba",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            { Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                using FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
                MainWindow.ink.Strokes = new StrokeCollection(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message, "Hiba",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Reuse MainMenuView's public Beallitasok_Click — same logic, no duplication
        private void Beallitasok_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.MainMenuUserControl.Beallitasok_Click(sender, e);
        }
    }
}
