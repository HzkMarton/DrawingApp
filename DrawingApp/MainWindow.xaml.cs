using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace DrawingApp
{
    public partial class MainWindow : Window
    {
        public static InkCanvas ink;
        public static Border options;
        // Undo és Redo műveletekhez vonalak tárolása Stack-ben (utolsó vonal kerül előre, dettó ugyanaz, mint egy List)
        private Stack<System.Windows.Ink.Stroke> _undoStack = new(); 
        private Stack<System.Windows.Ink.Stroke> _redoStack = new();
        public MainWindow()
        {
            InitializeComponent();
            ink = this.canvas;
            options = this.Menu;
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
            MessageBoxResult messageBoxResult = MessageBox.Show("A NEM mentett módosítások el fognak VESZNI, biztosan folytatja?", "Figyelem!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(messageBoxResult == MessageBoxResult.Yes)ink.Strokes.Clear();
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
                FilePath = dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message,"Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        static string FilePath = "";
        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            { Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*", FileName = "Untitled.isf" };
            if (FilePath == "")
            {
                if (dlg.ShowDialog() != true) return;
                try
                {
                    using FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                    ink.Strokes.Save(fs);
                    FilePath = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba a mentéskor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                using FileStream fs = new FileStream(FilePath, FileMode.Create);
                ink.Strokes.Save(fs);
            }
        }
        private void MentesMaskent_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            { Filter = "Ink files (*.isf)|*.isf|Minden fájl (*.*)|*.*", FileName = "Untitled.isf" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                using FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                ink.Strokes.Save(fs);
                FilePath = dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Export(object sender , RoutedEventArgs e)
        {
            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);            
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                VisualBrush vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }
            rtb.Render(dv);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            SaveFileDialog dlg = new SaveFileDialog
            { Filter = "PNG Image (*.png)|*.png|Minden fájl (*.*)|*.*", FileName = "Untitled.png" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                using FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                encoder.Save(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBoxResult messageBoxResult = MessageBox.Show("A NEM mentett módosítások el fognak VESZNI, biztosan folytatja?", "Figyelem!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes) Application.Current.Shutdown();
        }
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Info infoWindow = new Info(); 
            infoWindow.ShowDialog();
        }

        private void TemaFekete_Click(object sender, RoutedEventArgs e) => TemaValtas("fekete");
        private void TemaFeher_Click(object sender, RoutedEventArgs e) => TemaValtas("fehér");
        private void TemaKek_Click(object sender, RoutedEventArgs e) => TemaValtas("kék");

        public void TemaValtas(string themeName)
        {
            switch (themeName.ToLower())
            {
                case "fekete":
                    Application.Current.Resources["BackGround"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#131314"));
                    Application.Current.Resources["ForeGround"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3E3E3"));
                    Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1F20"));
                    break;
                case "fehér":
                    Application.Current.Resources["BackGround"] = new SolidColorBrush(Colors.White);
                    Application.Current.Resources["ForeGround"] = new SolidColorBrush(Colors.Black);
                    Application.Current.Resources["BorderColor"] = new SolidColorBrush(Colors.GhostWhite);
                    break;
                default:
                    Application.Current.Resources["BackGround"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#151A28"));
                    Application.Current.Resources["ForeGround"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3B9C8"));
                    Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C2135"));
                    break;
            }
            Eszkozok.ReColor();
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

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowStyle == WindowStyle.None)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }
            
        }
    }
}
