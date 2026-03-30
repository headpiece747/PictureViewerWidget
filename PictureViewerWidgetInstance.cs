using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using WigiDashWidgetFramework;
using WigiDashWidgetFramework.WidgetUtility;

namespace PictureViewerWidget
{
    public class PictureViewerWidgetInstance : IWidgetInstance
    {
        public IWidgetObject WidgetObject { get; }
        public Guid Guid { get; }
        public WidgetSize WidgetSize { get; }
        public event WidgetUpdatedEventHandler WidgetUpdated;

        private string _folderPath = string.Empty;
        private List<string> _imageFiles = new List<string>();
        private int _currentIndex = 0;
        private bool _settingsLoaded = false;

        public PictureViewerWidgetInstance(IWidgetObject widgetObject, WidgetSize widgetSize, Guid instanceGuid)
        {
            WidgetObject = widgetObject;
            WidgetSize = widgetSize;
            Guid = instanceGuid;
        }

        public void RequestUpdate()
        {
            // Changed to use WidgetObject.WidgetManager
            if (WidgetObject.WidgetManager != null && !_settingsLoaded)
            {
                LoadSettings();
                _settingsLoaded = true;
            }
            RenderAndBroadcast();
        }

        public void LoadSettings()
        {
            // Changed to use WidgetObject.WidgetManager
            if (WidgetObject.WidgetManager != null)
            {
                if (WidgetObject.WidgetManager.LoadSetting(this, "PictureFolderPath", out string savedPath) && !string.IsNullOrWhiteSpace(savedPath))
                {
                    _folderPath = savedPath;
                    RefreshImageList();
                }
            }
        }

        public void RefreshImageList()
        {
            _imageFiles.Clear();
            _currentIndex = 0;

            if (Directory.Exists(_folderPath))
            {
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                _imageFiles = Directory.GetFiles(_folderPath)
                                       .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                                       .OrderBy(f => f, new NaturalStringComparer()) // <-- UPDATED LINE
                                       .ToList();
            }
            RenderAndBroadcast();
        }

        public void ClickEvent(ClickType click_type, int x, int y)
        {
            if (click_type == ClickType.Single && _imageFiles.Count > 0)
            {
                // Advance to the next image and wrap around if at the end
                _currentIndex = (_currentIndex + 1) % _imageFiles.Count;
                RenderAndBroadcast();
            }
        }

        private void RenderAndBroadcast()
        {
            Task.Run(() =>
            {
                using (Bitmap bmp = DrawWidget())
                {
                    if (bmp != null)
                    {
                        var args = new WidgetUpdatedEventArgs
                        {
                            WidgetBitmap = (Bitmap)bmp.Clone(),
                            Offset = Point.Empty,
                            WaitMax = 1000
                        };
                        WidgetUpdated?.Invoke(this, args);
                    }
                }
            });
        }

        private Bitmap DrawWidget()
        {
            Size size = WidgetSize.ToSize();
            if (size.Width <= 0 || size.Height <= 0) return null;

            Bitmap bitmap = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Black);

                if (_imageFiles.Count == 0 || _currentIndex >= _imageFiles.Count)
                {
                    DrawTextCentered(g, size, "No Images Found\nCheck Settings");
                    return bitmap;
                }

                try
                {
                    string currentImagePath = _imageFiles[_currentIndex];
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(currentImagePath))
                    {
                        // Calculate aspect ratio to fit the screen without stretching
                        float scale = Math.Min((float)size.Width / img.Width, (float)size.Height / img.Height);
                        int drawW = (int)(img.Width * scale);
                        int drawH = (int)(img.Height * scale);
                        int drawX = (size.Width - drawW) / 2;
                        int drawY = (size.Height - drawH) / 2;

                        g.DrawImage(img, drawX, drawY, drawW, drawH);
                    }
                }
                catch (Exception ex)
                {
                    DrawTextCentered(g, size, "Error Loading Image");
                    // Changed to use WidgetObject.WidgetManager
                    WidgetObject.WidgetManager?.WriteLogMessage(this, LogLevel.ERROR, $"Failed to load image: {ex.Message}");
                }
            }
            return bitmap;
        }

        private void DrawTextCentered(Graphics g, Size size, string text)
        {
            using (Font font = new Font("Arial", 16, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(text, font, Brushes.White, new RectangleF(0, 0, size.Width, size.Height), sf);
            }
        }
        // This tells C# to sort strings the exact same way Windows File Explorer does
        public class NaturalStringComparer : IComparer<string>
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y)
            {
                return StrCmpLogicalW(x, y);
            }
        }

        public UserControl GetSettingsControl() => new PictureViewerWidgetSettings(this);
        public void EnterSleep() { }
        public void ExitSleep() { RequestUpdate(); }
        public void Dispose() { }
    }
}