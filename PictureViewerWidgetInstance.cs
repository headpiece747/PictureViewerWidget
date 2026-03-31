using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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

        // Prevents overlapping renders if RequestUpdate fires rapidly
        private int _renderPending = 0;

        public PictureViewerWidgetInstance(IWidgetObject widgetObject, WidgetSize widgetSize, Guid instanceGuid)
        {
            WidgetObject = widgetObject;
            WidgetSize = widgetSize;
            Guid = instanceGuid;
        }

        public void RequestUpdate()
        {
            if (WidgetObject.WidgetManager != null && !_settingsLoaded)
            {
                LoadSettings();
                _settingsLoaded = true;
            }
            RenderAndBroadcast();
        }

        public void LoadSettings()
        {
            if (WidgetObject.WidgetManager == null) return;

            if (WidgetObject.WidgetManager.LoadSetting(this, "PictureFolderPath", out string savedPath)
                && !string.IsNullOrWhiteSpace(savedPath))
            {
                _folderPath = savedPath;
                RefreshImageList();
            }
        }

        public void RefreshImageList()
        {
            _imageFiles.Clear();
            _currentIndex = 0;

            if (Directory.Exists(_folderPath))
            {
                var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                _imageFiles = Directory
                    .GetFiles(_folderPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f)))
                    .OrderBy(f => f, new NaturalStringComparer())
                    .ToList();
            }
            RenderAndBroadcast();
        }

        public void ClickEvent(ClickType click_type, int x, int y)
        {
            if (click_type == ClickType.Single && _imageFiles.Count > 0)
            {
                _currentIndex = (_currentIndex + 1) % _imageFiles.Count;
                RenderAndBroadcast();
            }
        }

        private void RenderAndBroadcast()
        {
            // Drop the render request if one is already queued
            if (Interlocked.CompareExchange(ref _renderPending, 1, 0) != 0) return;

            Task.Run(() =>
            {
                Interlocked.Exchange(ref _renderPending, 0);

                Bitmap bmp = DrawWidget();
                if (bmp == null) return;

                // Clone on a background thread; the framework owns the clone
                Bitmap clone;
                try   { clone = (Bitmap)bmp.Clone(); }
                finally { bmp.Dispose(); }

                var args = new WidgetUpdatedEventArgs
                {
                    WidgetBitmap = clone,
                    Offset       = Point.Empty,
                    WaitMax      = 1000
                };
                WidgetUpdated?.Invoke(this, args);
            });
        }

        private Bitmap DrawWidget()
        {
            Size size = WidgetSize.ToSize();
            if (size.Width <= 0 || size.Height <= 0) return null;

            Bitmap bitmap = new Bitmap(size.Width, size.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Black);

                if (_imageFiles.Count == 0 || _currentIndex >= _imageFiles.Count)
                {
                    DrawTextCentered(g, size, "No Images Found\nCheck Settings");
                    return bitmap;
                }

                string currentImagePath = _imageFiles[_currentIndex];

                try
                {
                    // Load into a MemoryStream first — avoids GDI+ file lock
                    // that keeps the source file open until the Bitmap is GC'd
                    using (var ms = new MemoryStream(File.ReadAllBytes(currentImagePath)))
                    using (var img = System.Drawing.Image.FromStream(ms))
                    {
                        float scale = Math.Min(
                            (float)size.Width  / img.Width,
                            (float)size.Height / img.Height);

                        int drawW = (int)(img.Width  * scale);
                        int drawH = (int)(img.Height * scale);
                        int drawX = (size.Width  - drawW) / 2;
                        int drawY = (size.Height - drawH) / 2;

                        g.DrawImage(img, drawX, drawY, drawW, drawH);
                    }
                }
                catch (Exception ex)
                {
                    DrawTextCentered(g, size, "Error Loading Image");
                    WidgetObject.WidgetManager?.WriteLogMessage(
                        this, LogLevel.ERROR, $"Failed to load image: {ex.Message}");
                }
            }
            return bitmap;
        }

        private void DrawTextCentered(Graphics g, Size size, string text)
        {
            using (var font = new Font("Arial", 16, System.Drawing.FontStyle.Bold))
            using (var sf   = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(text, font, Brushes.White,
                    new RectangleF(0, 0, size.Width, size.Height), sf);
            }
        }

        // Sorts the same way Windows File Explorer does
        public class NaturalStringComparer : IComparer<string>
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            private static extern int StrCmpLogicalW(string x, string y);

            public int Compare(string x, string y) => StrCmpLogicalW(x, y);
        }

        public UserControl GetSettingsControl() => new PictureViewerWidgetSettings(this);
        public void EnterSleep() { }
        public void ExitSleep()  { RequestUpdate(); }

        public void Dispose()
        {
            // Nothing unmanaged held open after the MemoryStream fix,
            // but implement the pattern cleanly for the framework
            _imageFiles.Clear();
        }
    }
}
