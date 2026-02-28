using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DrawingApp
{
    public partial class ColorPickerWindow : Window
    {
        // The selected color — read this after ShowDialog()
        public Color KivalasztottSzin { get; private set; } = Colors.Black;
        private WriteableBitmap _bitmap;
        private bool _Huzas = false;

        public ColorPickerWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => KorRajzolas();
        }

        // A színkör kirajzolása egy WriteableBitmap segítségével, majd megjelenítése egy Image controlban a Canvas-on.
        // Minden pixel színét a helyzetétől függően számolja ki: a szög határozza meg a Hue-t, a távolság a középponttól pedig a Saturation-t. A Value mindig 1 (teljes fényerő).
        // Az alfa csatorna 0 lesz a körön kívül, így ott nem lesz színválasztás.
        private void KorRajzolas()
        {
            int size = (int)KorCanvas.Width;   // 300
            int cx = size / 2; // középpont x koordinátája
            int cy = size / 2; // középpont y koordinátája
            int r = cx - 2; // sugár (kicsit kisebb, hogy ne lógjon ki a kör)

            _bitmap = new WriteableBitmap(size, size,96, 96,PixelFormats.Bgra32, null); // 32 bites színformátum (8 bit kék, 8 bit zöld, 8 bit piros, 8 bit alfa)
            int[] pixels = new int[size * size];

            for (int y = 0; y < size; y++) // minden sor
            {
                for (int x = 0; x < size; x++) // minden oszlop
                {
                    double dx = x - cx; // középponttól való távolság x irányban
                    double dy = y - cy; // középponttól való távolság y irányban
                    double tavolsag = Math.Sqrt(dx * dx + dy * dy); // távolság a középponttól

                    if (tavolsag <= r) // A körön belül
                    {
                        // Hue a szögből, Saturation a távolságból
                        double hue = (Math.Atan2(dy, dx) + Math.PI) / (2 * Math.PI) * 360.0; // 0–360° körben
                        double saturation = tavolsag / r; // 0 középen és 1 a szélén
                        Color c = HsvToRgb(hue, saturation, 1.0); // teljes fényerő
                        pixels[y * size + x] = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B; // ARGB forma
                    }
                    // A körön kívül a pixel értéke marad 0, ami azt jelenti, hogy teljesen átlátszó lesz, így nem lehet színt választani onnan
                }
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, size, size),pixels, size * 4, 0); // 4 bájt pixelenként

            var img = new System.Windows.Controls.Image
            {
                Width = size,
                Height = size,
                Source = _bitmap
            };
            KorCanvas.Children.Add(img);
        }

        // Egér események kezelése a színválasztáshoz: MouseDown, MouseMove és MouseUp.

        private void Wheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _Huzas = true; // Jelzi, hogy elkezdtük húzni az egeret
            KorCanvas.CaptureMouse();  // Rögzíti a mouse move eseményeket, még akkor is, ha az egér elhagyja a canvas területét
            Szinvalasztas(e.GetPosition(KorCanvas)); // Azonnal színválasztás a kattintás helyén
        }

        private void Wheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_Huzas && e.LeftButton == MouseButtonState.Pressed) // Csak akkor változik, ha éppen húzzuk az egeret
                Szinvalasztas(e.GetPosition(KorCanvas)); // Színválasztás a húzás helyén
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _Huzas = false; // Jelzi, hogy befejeztük a húzást
            KorCanvas.ReleaseMouseCapture(); // Feloldja a mouse capture-t, így a mouse move események már nem lesznek rögzítve
            base.OnMouseUp(e); // Fontos, hogy meghívjuk a base osztály OnMouseUp metódusát, hogy a többi eseménykezelő is működjön
        }

        private void Szinvalasztas(Point p)
        {
            int size = (int)KorCanvas.Width; // 300, a bitmap mérete
            // Korlátozza a koordinátákat a bitmap méretére, hogy ne legyenek negatív vagy túl nagy értékek
            int x = (int)Math.Clamp(p.X, 0, size - 1); 
            int y = (int)Math.Clamp(p.Y, 0, size - 1);

            // A bitmapből olvassa ki a pixel színét a megadott koordinátákon
            int[] pixel = new int[1];
            _bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0); // 4 bájt pixelenként, így a stride 4
            // A pixel érték ARGB formátumban van, így szét kell bontani a komponensekre
            int argb = pixel[0];
            byte rb = (byte)((argb >> 16) & 0xFF); // piros komponens
            byte gb = (byte)((argb >> 8) & 0xFF); // zöld komponens
            byte bb = (byte)(argb & 0xFF); // kék komponens
            byte ab = (byte)((argb >> 24) & 0xFF); // alfa komponens

            // Ha kívül vagyunk a körön, akkor az alfa 0 lesz, így nem választunk színt
            if (ab == 0) return;

            KivalasztottSzin = Color.FromRgb(rb, gb, bb); // beállítja a kiválasztott színt a színkörből
            Szinelonezet.Background = new SolidColorBrush(KivalasztottSzin); // frissíti a színelőnézetet
            HexBox.TextChanged -= HexBox_TextChanged; // ideiglenesen letiltja a HexBox szövegváltozás eseményét, hogy ne okozzon végtelen ciklust
            HexBox.Text = $"#{rb:X2}{gb:X2}{bb:X2}"; // frissíti a HexBox szövegét a kiválasztott szín hex kódjával
            HexBox.TextChanged += HexBox_TextChanged; // visszaállítja a HexBox szövegváltozás eseményét, hogy újra reagáljon a kézi hex bevitelre
        }

        // Hex

        private void HexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = HexBox.Text.TrimStart('#');
            if (text.Length != 6) return;
            try
            {
                byte r = Convert.ToByte(text[0..2], 16); 
                byte g = Convert.ToByte(text[2..4], 16); 
                byte b = Convert.ToByte(text[4..6], 16);
                KivalasztottSzin = Color.FromRgb(r, g, b);
                Szinelonezet.Background = new SolidColorBrush(KivalasztottSzin);
            }
            catch { /* helytelen hex */ }
        }

        // OK

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // jelzi, hogy sikeres választás történt
            Close();
        }

        // HSV → RGB 
        private static Color HsvToRgb(double h, double s, double v)
        {
            double c = v * s; // kroma
            double x = c * (1 - Math.Abs(h / 60 % 2 - 1)); // a második legnagyobb komponens
            double m = v - c; // a legkisebb komponens hozzáadva, hogy a szín világosabb legyen
            double r1, g1, b1; // színkód a kroma és x alapján

            if (h < 60) { r1 = c; g1 = x; b1 = 0; } // 0–60°: piros → sárga
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; } // 60–120°: sárga → zöld
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; } // 120–180°: zöld → cián
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; } // 180–240°: cián → kék
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; } // 240–300°: kék → magenta
            else { r1 = c; g1 = 0; b1 = x; } // 300–360°: magenta → piros

            return Color.FromRgb((byte)((r1 + m) * 255),(byte)((g1 + m) * 255),(byte)((b1 + m) * 255)); // visszaalakítja RGB-vé, 0–255-ös értékekre
        }
    }
}
