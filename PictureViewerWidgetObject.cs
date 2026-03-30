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
        // Generate a new unique GUID for this widget
        public Guid Guid => new Guid("{A1B2C3D4-1234-5678-90AB-CDEF12345678}");
        public string Name => "Picture Viewer";
        public string Author => "headpiece747";
        public string Website => "https://eclipticsight.com";
        public string Description => "Click to cycle through pictures in a folder.";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public SdkVersion TargetSdk => WidgetUtility.CurrentSdkVersion;

        // Support standard sizes, e.g., 2x2, 3x3, 4x4, 5x4
        public List<WidgetSize> SupportedSizes => new List<WidgetSize>
        {
            new WidgetSize(2, 2), new WidgetSize(3, 3), new WidgetSize(4, 4), new WidgetSize(5, 4)
        };

        public IWidgetManager WidgetManager { get; set; }
        public string LastErrorMessage { get; set; }

        private Bitmap _previewImage;

        public Bitmap PreviewImage => GetWidgetPreview(new WidgetSize(2, 2));

        public Bitmap WidgetThumbnail => GetWidgetPreview(new WidgetSize(2, 2));

        public Bitmap GetWidgetPreview(WidgetSize widgetSize)
        {
            if (_previewImage == null)
            {
                _previewImage = CreatePlaceholderBitmap("Pic Viewer");
            }
            return _previewImage;
        }

        public IWidgetInstance CreateWidgetInstance(WidgetSize widgetSize, Guid instanceGuid)
        {
            return new PictureViewerWidgetInstance(this, widgetSize, instanceGuid);
        }

        public bool RemoveWidgetInstance(Guid instanceGuid)
        {
            return true;
        }

        public WidgetError Load(string resourcePath)
        {
            return WidgetError.NO_ERROR;
        }

        public WidgetError Unload()
        {
            _previewImage?.Dispose();
            _previewImage = null;
            return WidgetError.NO_ERROR;
        }

        private Bitmap CreatePlaceholderBitmap(string text)
        {
            Bitmap placeholder = new Bitmap(200, 145);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                // Improve text quality
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                
                g.Clear(Color.Red);
                using (Font f = new Font("Arial", 20, FontStyle.Bold))

                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(text, f, Brushes.White, new RectangleF(0, 0, 200, 145), sf);
                }
            }
            return placeholder;
        }
    }
}