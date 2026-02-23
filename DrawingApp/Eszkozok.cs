using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DrawingApp
{
    internal class Eszkozok : Border
    {
        public enum Tipus { Radir, Szin, Kiemelo, Toll}
        private Tipus _tipus;
        public DrawingAttributes ToolAttributes;
        public static Dictionary<Tipus, string> svgIcons;
        public static List<Eszkozok> ToolContainer = new List<Eszkozok>();
        public Tipus Type { get { return _tipus; } }
        public bool Selected { get; set; }
        public Eszkozok(Tipus tipus)
        {
            this._tipus = tipus;
            SetIcon(svgIcons[this._tipus]);
            this.Width = 30;
            this.Height = 30;
            this.Margin = new System.Windows.Thickness(0, 20, 0, 0);
            if(this._tipus == Tipus.Szin)this.CornerRadius = new System.Windows.CornerRadius(20);
            this.ToolAttributes = new DrawingAttributes();
            this.Selected = false;
            if(this._tipus == Tipus.Kiemelo)
            {
                this.ToolAttributes.IsHighlighter = true;
                this.ToolAttributes.StylusTip = StylusTip.Rectangle;
                this.ToolAttributes.Width = 30;
                this.ToolAttributes.Height = 20;
                this.ToolAttributes.Color = Colors.Red;
            }
            else if(this._tipus == Tipus.Toll)
            {
                this.ToolAttributes.IsHighlighter = false;
                this.ToolAttributes.StylusTip = StylusTip.Ellipse;
                this.ToolAttributes.Width = 3;
                this.ToolAttributes.Height = 3;
                MainWindow.ink.DefaultDrawingAttributes = this.ToolAttributes;
                this.Selected = true;
            }
            this.MouseEnter += Eszkozok_MouseEnter;
            this.MouseLeave += Eszkozok_MouseLeave;
            this.MouseDown += Eszkozok_MouseDown;
            ToolContainer.Add(this);
        }

        private void Eszkozok_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.ink.EditingMode = InkCanvasEditingMode.Ink;
            foreach(Eszkozok f in  ToolContainer) f.Selected = false;
            this.Selected = true;
            switch (this._tipus)
            {
                case Tipus.Radir:
                    MainWindow.ink.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case Tipus.Szin:
                    MainWindow.colors.Visibility = System.Windows.Visibility.Visible;
                    TranslateTransform translate = new TranslateTransform();
                    MainWindow.colors.RenderTransform = translate;
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = -200,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(1),
                        EasingFunction = new CubicEase {EasingMode = EasingMode.EaseOut}
                    };
                    translate.BeginAnimation(TranslateTransform.XProperty, animation);
                    break;
                case Tipus.Toll:
                case Tipus.Kiemelo:
                    MainWindow.ink.DefaultDrawingAttributes = this.ToolAttributes;
                    break;
            }
            Szinek.SelectedColor = new SolidColorBrush(this.ToolAttributes.Color);
            Eszkozok.ToolContainer.Where(x => x.Type == Eszkozok.Tipus.Szin).First().Update();

        }

        private void Eszkozok_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.GrowReset(true);
        }

        private void Eszkozok_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.GrowReset(false);
        }
        private void GrowReset(bool isReset)
        {
            if (isReset)
            {
                this.Width = 30;
                this.Height = 30;
            }
            else
            {
                this.Width = 40;
                this.Height = 40;
            }
        }
        private void SetIcon(string svg)
        {
            if(this._tipus == Tipus.Szin)
            {
                this.Background = Brushes.Black;
                return;
            }
            Geometry geometry = Geometry.Parse(svg);
            Brush szin = MainWindow.Foreground;
            GeometryDrawing icon = new GeometryDrawing(szin, new Pen(szin, 1), geometry);
            DrawingBrush brush = new DrawingBrush(icon)
            {
                Stretch = Stretch.Uniform
            };
            this.Background = brush;
        }
        public void Update()
        {
            switch (this._tipus)
            {
                case Tipus.Szin:
                    this.Background = Szinek.SelectedColor;
                    break;
            }
        }
        public static void Initialize()
        {
            StreamReader sr = new StreamReader("assets/icons.json");
            svgIcons = JsonSerializer.Deserialize<Dictionary<Tipus, string>>(sr.ReadToEnd());
            if (svgIcons == null) throw new Exception("Nem sikerült az ikonok betöltése");
            Tipus[] tipusok = { Tipus.Szin,Tipus.Toll, Tipus.Kiemelo, Tipus.Radir };
            foreach (Tipus t in tipusok) new Eszkozok(t);
        }
    }
}
