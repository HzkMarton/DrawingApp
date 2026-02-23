using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DrawingApp
{
    internal class Szinek : Border
    {
        private SolidColorBrush _szin;
        private bool _selected;
        public static List<Szinek> ColorContainer = new List<Szinek>();
        public static SolidColorBrush SelectedColor = Brushes.Black;
        public Szinek(SolidColorBrush sz)
        {
            this.Width = 30;
            this.Height = 30;
            this.Margin = new System.Windows.Thickness(0, 10, 0, 0);
            this.CornerRadius = new System.Windows.CornerRadius(10);
            this._szin = sz;
            this.Background = this._szin;
            ColorContainer.Add(this);
            this.Cursor = Cursors.Hand;
            this.MouseDown += Szinek_MouseDown;
            this.MouseEnter += Szinek_MouseEnter;
            this.MouseLeave += Szinek_MouseLeave;
        }

        private void Szinek_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!this._selected)
            {
                this.GrowReset(true);
            }
        }

        private void Szinek_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.GrowReset(false);
        }

        private void Szinek_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.ink.DefaultDrawingAttributes.Color = this._szin.Color;
            this._selected = true;
            foreach (Szinek s in ColorContainer)
            {
                if (s == this) continue;
                s._selected = false;
                s.GrowReset(true);
            }
            this.GrowReset(false);
            TranslateTransform translate = new TranslateTransform();
            MainWindow.colors.RenderTransform = translate;
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -200,
                Duration = TimeSpan.FromSeconds(2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            translate.BeginAnimation(TranslateTransform.XProperty, animation);
            SelectedColor = this._szin;
            Eszkozok.ToolContainer.Where(x => x.Type == Eszkozok.Tipus.Szin).First().Update();
            Eszkozok.ToolContainer.Where(x => x.Selected).First().ToolAttributes.Color = this._szin.Color;
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
        public static void Initialize()
        {
            SolidColorBrush[] brushes = new SolidColorBrush[] { Brushes.Black, Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LawnGreen, Brushes.Blue, Brushes.Purple };
            foreach (SolidColorBrush b in brushes) new Szinek(b);
        }
        
        
    }
}
