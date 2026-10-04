using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace X_menu_app.Views
{
    public partial class AddProductWindow : Window
    {
        public string? ProductName { get; private set; }
        public int ProductQuantity { get; private set; } = 1;
        public decimal ProductPrice { get; private set; } = 0.0m;
        public bool ProductIsAvailable { get; private set; } = true;
        public string? ProductImagePath { get; private set; }
        private string? _tempOriginalImage;

        public string? ProductOriginalImagePath => _tempOriginalImage;

        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                ProductImagePath = dlg.FileName;
                ImagePathText.Text = ProductImagePath;
                try
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage(new Uri(ProductImagePath));
                    PreviewImage.Source = bmp;
                }
                catch
                {
                    PreviewImage.Source = null;
                }
            }
        }

        private void PhotoOption_Checked(object sender, RoutedEventArgs e)
        {
            // Defensive null-checks: the Checked event may fire during initialization
            if (NoPhotoRadio == null || BrowseButton == null || ImagePathText == null || PreviewImage == null)
                return;

            if (NoPhotoRadio.IsChecked == true)
            {
                BrowseButton.IsEnabled = false;
                ImagePathText.Text = string.Empty;
                PreviewImage.Source = null;
                ProductImagePath = null;
            }
            else
            {
                BrowseButton.IsEnabled = true;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // wrap logic to surface any unexpected errors without crashing

            ProductName = NameBox.Text?.Trim();

            if (!int.TryParse(QuantityBox.Text, out var qty) || qty < 0)
            {
                MessageBox.Show("Veuillez entrer une quantité valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                // Try with current culture
                if (!decimal.TryParse(PriceBox.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out price))
                {
                    MessageBox.Show("Veuillez entrer un prix valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ProductQuantity = qty;
            ProductPrice = price;
            ProductIsAvailable = AvailableBox?.IsChecked == true;

            // If user chose to include photo, copy and generate thumbnail into app data folder
            if ((WithPhotoRadio?.IsChecked == true) && !string.IsNullOrWhiteSpace(ProductImagePath))
            {
                try
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string imagesDir = Path.Combine(appData, "X_menu_app", "Images");
                    Directory.CreateDirectory(imagesDir);

                    // Copy original for reference
                    string originalExt = Path.GetExtension(ProductImagePath);
                    string originalName = Guid.NewGuid().ToString() + originalExt;
                    string destOriginal = Path.Combine(imagesDir, originalName);
                    File.Copy(ProductImagePath, destOriginal, true);

                    // Create thumbnail with fixed size (160x120) while preserving aspect ratio
                    int thumbW = 160;
                    int thumbH = 120;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(destOriginal);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    double scale = Math.Min((double)thumbW / bitmap.PixelWidth, (double)thumbH / bitmap.PixelHeight);
                    if (scale > 1) scale = 1; // don't upscale

                    var transform = new ScaleTransform(scale, scale);
                    var tb = new TransformedBitmap(bitmap, transform);

                    string thumbName = Guid.NewGuid().ToString() + ".png";
                    string destThumb = Path.Combine(imagesDir, thumbName);

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(tb));
                    using (var fs = new FileStream(destThumb, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    // Store both original and thumbnail paths
                    ProductImagePath = destThumb;
                    ImagePathText.Text = ProductImagePath;
                    // expose original path via temporary field on window to be consumed by caller
                    _tempOriginalImage = destOriginal;
                }
                catch (Exception ex)
                {
                    // surface error to user but avoid crashing
                    ProductImagePath = null;
                    MessageBox.Show($"Erreur lors du traitement de l'image : {ex.Message}", "Erreur image", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            this.DialogResult = true;
            this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : {ex.Message}\n\n{ex.StackTrace}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
