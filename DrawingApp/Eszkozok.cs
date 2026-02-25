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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DrawingApp
{
    internal class Eszkozok : Border
    {
        public enum Tipus { Radir, Szin, Kiemelo, Toll, None}

        public Tipus _tipus;
        private bool _selected;
        private SolidColorBrush _selfColor;
        private Tipus _linkedTo;
        public DrawingAttributes ToolAttributes;
        public static Dictionary<Tipus, string> svgIcons;
        public static List<Eszkozok> ToolContainer = new List<Eszkozok>();
        
        public Eszkozok(Tipus tipus, SolidColorBrush selfColor, Tipus link)
        {
            this._tipus = tipus;
            this._selfColor = selfColor;
            this._linkedTo = link;
            this.Width = 30;
            this.Height = 30;
            this.Cursor = Cursors.Hand;
            this.ToolAttributes = new DrawingAttributes();
            this._selected = false;
            if (this._tipus == Tipus.Szin)
            {
                this.CornerRadius = new System.Windows.CornerRadius(20);
                this.Margin = new System.Windows.Thickness(20, 0, 0, 0);
                if(selfColor == Brushes.Black && link == Tipus.Toll) this._selected = true;
                else if (selfColor == Brushes.Red && link == Tipus.Kiemelo) this._selected = true;
            }
            else
            {
                this.Margin = new System.Windows.Thickness(0, 20, 0, 0);
            }
            
            if (this._tipus == Tipus.Kiemelo)
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
                this.GrowReset(false);
                this._selected = true;
            }
            this.MouseEnter += Eszkozok_MouseEnter;
            this.MouseLeave += Eszkozok_MouseLeave;
            this.MouseDown += Eszkozok_MouseDown;
            SetIcon(svgIcons[this._tipus]);
            ToolContainer.Add(this);
        }

        private void Eszkozok_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.ink.EditingMode = InkCanvasEditingMode.Ink;
            foreach (Eszkozok f in ToolContainer)
            {
                if(this._tipus == Tipus.Szin && f._tipus == Tipus.Szin && f._linkedTo == this._linkedTo)f._selected = false;
                else if (this._tipus != Tipus.Szin && f._tipus != Tipus.Szin) f._selected = false;
              
            }
            this._selected = true;
            foreach (Eszkozok f in ToolContainer) f.GrowReset(true);
            switch (this._tipus)
            {
                case Tipus.Radir:
                    MainWindow.ink.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case Tipus.Szin:
                    ToolContainer.Where(x => x._tipus == this._linkedTo).First().ToolAttributes.Color = this._selfColor.Color;
                    break;
                case Tipus.Toll:
                case Tipus.Kiemelo:
                    MainWindow.ink.DefaultDrawingAttributes = this.ToolAttributes;
                    CreateToolBar();
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
            DoubleAnimation @grow = new DoubleAnimation()
            {
                From = 30,
                To = 40,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            DoubleAnimation @shrink = new DoubleAnimation()
            {
                From = 40,
                To = 30,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            if (isReset && !this._selected)
            {
                if (this.Width != 30)
                {
                    this.BeginAnimation(WidthProperty, shrink);
                    this.BeginAnimation(HeightProperty, shrink);
                }
            }
            else
            {
                if (!this._selected)
                {
                    this.BeginAnimation(WidthProperty, grow);
                    this.BeginAnimation(HeightProperty, grow);
                }
            }
        }

        private void SetIcon(string svg)
        {
            if(this._tipus == Tipus.Szin)
            {
                this.Background = this._selfColor;
                return;
            }
            Geometry geometry = Geometry.Parse(svg);
            Brush szin = this._selfColor;
            GeometryDrawing icon = new GeometryDrawing(szin, new Pen(szin, 1), geometry);
            DrawingBrush brush = new DrawingBrush(icon)
            {
                Stretch = Stretch.Uniform
            };
            this.Background = brush;
        }
        StackPanel sp = new StackPanel();
        public void CreateToolBar()
        {
            sp.Children.Clear();
            sp.Orientation = Orientation.Horizontal;
            foreach (Eszkozok e in ToolContainer)
            {
                if (e._linkedTo == this._tipus)
                {
                    sp.Children.Add(e);
                    if (e._selected)
                    {
                        e.Width = 40;
                        e.Height = 40;
                    }
                }
            }
            MainWindow.options.Child = sp;
            
        }

        public static void Initialize()
        {
            StreamReader sr = new StreamReader("assets/icons.json");
            svgIcons = JsonSerializer.Deserialize<Dictionary<Tipus, string>>(sr.ReadToEnd());
            if (svgIcons == null) throw new Exception("Nem sikerült az ikonok betöltése");
            Tipus[] tipusok = {Tipus.Toll, Tipus.Kiemelo, Tipus.Radir };
            foreach (Tipus t in tipusok) new Eszkozok(t, MainWindow.Foreground, Tipus.None);
            foreach (SolidColorBrush sb in new SolidColorBrush[] { Brushes.Red, Brushes.Orange, Brushes.Green }) new Eszkozok(Tipus.Szin, sb, Tipus.Kiemelo);
            foreach (SolidColorBrush sb in new SolidColorBrush[] { Brushes.Black, Brushes.Blue, Brushes.Red }) new Eszkozok(Tipus.Szin, sb, Tipus.Toll);
        }
    }
}
