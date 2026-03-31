using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using WigiDashWidgetFramework;
using WigiDashWidgetFramework.WidgetUtility;

namespace PictureViewerWidget
{
    public class PictureViewerWidgetObject : IWidgetObject
    {
        public Guid   Guid        => new Guid("{A1B2C3D4-1234-5678-90AB-CDEF12345678}");
        public string Name        => "Picture Viewer";
        public string Author      => "headpiece747";
        public string Website     => "https://eclipticsight.com";
        public string Description => "Click to cycle through pictures in a folder.";

        // Assembly.GetName().Version is always 4-part (e.g. 1.0.0.0).
        // Trim trailing .0 segments so the display matches the 3-part
        // version set in AssemblyInfo.cs (e.g. 1.0.0).
        public Version Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                if (v == null) return new Version(1, 0, 0);

                if (v.Revision != 0)
                    return new Version(v.Major, v.Minor, v.Build, v.Revision);
                if (v.Build != 0)
                    return new Version(v.Major, v.Minor, v.Build);
                return new Version(v.Major, v.Minor);
            }
        }

        public SdkVersion TargetSdk => WidgetUtility.CurrentSdkVersion;

        public List<WidgetSize> SupportedSizes => new List<WidgetSize>
        {
            new WidgetSize(2, 2),
            new WidgetSize(3, 3),
            new WidgetSize(4, 4),
            new WidgetSize(5, 4)
        };

        public IWidgetManager WidgetManager    { get; set; }
        public string         LastErrorMessage { get; set; }

        // Cached once; disposed in Unload()
        private Bitmap _previewImage;

        // All three properties share the same cached instance
        public Bitmap PreviewImage                   => EnsurePreview();
        public Bitmap WidgetThumbnail                => EnsurePreview();
        public Bitmap GetWidgetPreview(WidgetSize _) => EnsurePreview();

        private Bitmap EnsurePreview()
        {
            if (_previewImage == null)
                _previewImage = CreatePlaceholderBitmap("Pic Viewer");
            return _previewImage;
        }

        public IWidgetInstance CreateWidgetInstance(WidgetSize widgetSize, Guid instanceGuid)
            => new PictureViewerWidgetInstance(this, widgetSize, instanceGuid);

        public bool RemoveWidgetInstance(Guid instanceGuid) => true;

        public WidgetError Load(string resourcePath) => WidgetError.NO_ERROR;

        public WidgetError Unload()
        {
            _previewImage?.Dispose();
            _previewImage = null;
            return WidgetError.NO_ERROR;
        }

        private Bitmap CreatePlaceholderBitmap(string text)
        {
            var bmp = new Bitmap(200, 145);
            using (var g  = Graphics.FromImage(bmp))
            using (var f  = new Font("Arial", 20, System.Drawing.FontStyle.Bold))
            using (var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Red);
                g.DrawString(text, f, Brushes.White, new RectangleF(0, 0, 200, 145), sf);
            }
            return bmp;
        }
    }
}
