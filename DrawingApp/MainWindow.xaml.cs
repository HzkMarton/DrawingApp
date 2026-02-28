using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        public static InkCanvas ink;
        public static Border options;
        public static SolidColorBrush Background = new SolidColorBrush(Color.FromRgb(21, 26, 40));
        public static SolidColorBrush Foreground = new SolidColorBrush(Color.FromRgb(179, 185, 200));
        // Undo és Redo műveletekhez vonalak tárolása Stack-ben (utolsó vonal kerül előre, dettó ugyanaz, mint egy List)
        private Stack<System.Windows.Ink.Stroke> _undoStack = new(); 
        private Stack<System.Windows.Ink.Stroke> _redoStack = new();
        public MainWindow()
        {
            InitializeComponent();
            ink = this.canvas;
            options = this.Menu;
            this.ToolContainer.Background = Background;
            this.MainContainer.Background = Background;
            Initialize();
            // Undo/Redo vonalak követése
            ink.Strokes.StrokesChanged += Vonal_Valtoztatas;
        }
        private void Vonal_Valtoztatas(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            foreach (var stroke in e.Added)
            {
                _undoStack.Push(stroke); // új vonal hozzáadása az undo stackhez
                _redoStack.Clear();   // új rajz esetén a redo stack törlése
            }
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
            Eszkozok.ToolContainer
                     .Where(x => x._tipus == Eszkozok.Tipus.Toll)
                     .First()
                     .CreateToolBar();
        }

        // Navbar
        private void UjCanvas_Click(object sender, RoutedEventArgs e)
        {
            ink.Strokes.Clear();
            var picker = new ColorPickerWindow { Owner = Application.Current.MainWindow };
            if (picker.ShowDialog() == true)
            {
                Color chosen = picker.KivalasztottSzin;
                MainWindow.ink.DefaultDrawingAttributes.Color = chosen;
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
                ink.Strokes = new StrokeCollection(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message,"Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            { Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                using FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                ink.Strokes.Save(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message,"Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count == 0) return;
            var stroke = _undoStack.Pop();
            ink.Strokes.StrokesChanged -= Vonal_Valtoztatas;   // megelőzi a többszöri meghívást
            ink.Strokes.Remove(stroke); // eltávolítja a vonalat
            ink.Strokes.StrokesChanged += Vonal_Valtoztatas; // visszaállítja a változáskövetést
            _redoStack.Push(stroke); // hozzáadja a redo stackhez, hogy lehessen redo-zni
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count == 0) return;
            var stroke = _redoStack.Pop();
            ink.Strokes.StrokesChanged -= Vonal_Valtoztatas;   // megelőzi a többszöri meghívást
            ink.Strokes.Add(stroke); // újra hozzáadja a vonalat
            ink.Strokes.StrokesChanged += Vonal_Valtoztatas; // visszaállítja a változáskövetést
            _undoStack.Push(stroke); // visszahelyezi az undo stackre, hogy újra lehessen undo-zni
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rajzoló alkalmazás\nKészítette: RPT Industries\n2025",
                               "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TemaFekete_Click(object sender, RoutedEventArgs e) => TemaValtas("fekete");
        private void TemaFeher_Click(object sender, RoutedEventArgs e) => TemaValtas("fehér");
        private void TemaKek_Click(object sender, RoutedEventArgs e) => TemaValtas("kék");

        public void TemaValtas(string themeName)
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
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Z → Visszavonás
            if (e.Key == Key.Z && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Undo_Click(sender, e);
                e.Handled = true;
            }

            // Ctrl + Y → Újra
            if (e.Key == Key.Y && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Redo_Click(sender, e);
                e.Handled = true;
            }

            // Ctrl + S → Mentés
            if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Mentes_Click(sender, e);
                e.Handled = true;
            }
        }

    }
}
