using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrawingApp
{
    internal class Eszkozok : Border
    {
        public enum Tipus { Radir, Szin, Kiemelo, Toll}
        private Tipus _tipus;
        public static Dictionary<Tipus, string> svgIcons;
        public static List<Eszkozok> ToolContainer = new List<Eszkozok>();
        public Eszkozok(Tipus tipus)
        {
            this._tipus = tipus;
            SetIcon(svgIcons[this._tipus]);
            this.Width = 30;
            this.Height = 30;
            this.Margin = new System.Windows.Thickness(0, 20, 0, 0);
            if(this._tipus == Tipus.Szin)this.CornerRadius = new System.Windows.CornerRadius(20);
            this.MouseEnter += Eszkozok_MouseEnter;
            this.MouseLeave += Eszkozok_MouseLeave;
            this.MouseDown += Eszkozok_MouseDown;
            ToolContainer.Add(this);
        }

        private void Eszkozok_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (this._tipus)
            {
                case Tipus.Radir:
                    MainWindow.ink.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case Tipus.Toll:
                    MainWindow.ink.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case Tipus.Szin:
                    MainWindow.colors.Visibility = System.Windows.Visibility.Visible;
                    MainWindow.ink.EditingMode = InkCanvasEditingMode.Ink;
                    break;
            }
            
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
            BrushConverter converter = new BrushConverter();
            Brush szin = (Brush)converter.ConvertFromString("#B3B9C8");
            GeometryDrawing icon = new GeometryDrawing(szin, new Pen(szin, 1), geometry);
            DrawingBrush brush = new DrawingBrush(icon)
            {
                Stretch = Stretch.Uniform
            };
            this.Background = brush;
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
