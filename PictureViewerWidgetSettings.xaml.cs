using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Data;
using Color = System.Drawing.Color;

namespace PictureViewerWidget
{
    public partial class PictureViewerWidgetSettings : UserControl
    {
        private PictureViewerWidgetInstance _instance;

        // overlay state
        private Color _bgColor      = Color.Black;
        private Color _overlayColor = Color.White;
        private System.Drawing.Font _overlayFont;
        private int   _overlayXPos    = 0;  // 0=Center 1=Left 2=Right
        private int   _overlayYPos    = 0;  // 0=Center 1=Top  2=Bottom
        private int   _overlayXOffset = 0;
        private int   _overlayYOffset = 0;
        private bool  _useGlobal      = false;

        public PictureViewerWidgetSettings(PictureViewerWidgetInstance instance)
        {
            _instance = instance;

            // Fully qualified to avoid WPF FontStyle ambiguity
            _overlayFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            InitializeComponent();
            LoadCurrentSettings();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  LOAD
        // ─────────────────────────────────────────────────────────────────────
        private void LoadCurrentSettings()
        {
            var mgr = _instance.WidgetObject.WidgetManager;
            if (mgr == null) return;

            // Image section
            if (mgr.LoadSetting(_instance, "PictureFolderPath", out string path))
                TxtFolderPath.Text = path ?? string.Empty;

            // Design section
            if (mgr.LoadSetting(_instance, "BackColor", out string bgHtml) && !string.IsNullOrEmpty(bgHtml))
                try { _bgColor = ColorTranslator.FromHtml(bgHtml); } catch { }
            BgColorSelect.Content = ColorTranslator.ToHtml(_bgColor);

            if (mgr.LoadSetting(_instance, "OverlayText", out string overlayText))
                TextOverlay.Text = overlayText ?? string.Empty;

            if (mgr.LoadSetting(_instance, "OverlayColor", out string olHtml) && !string.IsNullOrEmpty(olHtml))
                try { _overlayColor = ColorTranslator.FromHtml(olHtml); } catch { }
            OverlayColorSelect.Content = ColorTranslator.ToHtml(_overlayColor);

            if (mgr.LoadSetting(_instance, "OverlayFont", out string fontStr) && !string.IsNullOrEmpty(fontStr))
                try { _overlayFont = (System.Drawing.Font)new FontConverter().ConvertFromInvariantString(fontStr); } catch { }
            OverlayFontSelect.Content = new FontConverter().ConvertToInvariantString(_overlayFont);
            OverlayFontSelect.Tag = _overlayFont;

            if (mgr.LoadSetting(_instance, "OverlayXPos", out string xposStr) && int.TryParse(xposStr, out int xpos))
                _overlayXPos = xpos;
            OverlayXPos.SelectedIndex = _overlayXPos;

            if (mgr.LoadSetting(_instance, "OverlayYPos", out string yposStr) && int.TryParse(yposStr, out int ypos))
                _overlayYPos = ypos;
            OverlayYPos.SelectedIndex = _overlayYPos;

            if (mgr.LoadSetting(_instance, "UseGlobal", out string useGlobalStr))
                bool.TryParse(useGlobalStr, out _useGlobal);
            GlobalThemeCheck.IsChecked = _useGlobal;
            ApplyGlobalThemeState();

            // Advanced section
            if (mgr.LoadSetting(_instance, "OverlayXOffset", out string xoffStr) && int.TryParse(xoffStr, out int xoff))
                _overlayXOffset = xoff;
            OverlayXOffset.Value = _overlayXOffset;

            if (mgr.LoadSetting(_instance, "OverlayYOffset", out string yoffStr) && int.TryParse(yoffStr, out int yoff))
                _overlayYOffset = yoff;
            OverlayYOffset.Value = _overlayYOffset;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SAVE
        // ─────────────────────────────────────────────────────────────────────
        private void SaveAll()
        {
            var mgr = _instance.WidgetObject.WidgetManager;
            if (mgr == null) return;

            mgr.StoreSetting(_instance, "BackColor",      ColorTranslator.ToHtml(_bgColor));
            mgr.StoreSetting(_instance, "OverlayText",    TextOverlay.Text);
            mgr.StoreSetting(_instance, "OverlayColor",   ColorTranslator.ToHtml(_overlayColor));
            mgr.StoreSetting(_instance, "OverlayFont",    new FontConverter().ConvertToInvariantString(_overlayFont));
            mgr.StoreSetting(_instance, "OverlayXPos",    _overlayXPos.ToString());
            mgr.StoreSetting(_instance, "OverlayYPos",    _overlayYPos.ToString());
            mgr.StoreSetting(_instance, "OverlayXOffset", _overlayXOffset.ToString());
            mgr.StoreSetting(_instance, "OverlayYOffset", _overlayYOffset.ToString());
            mgr.StoreSetting(_instance, "UseGlobal",      _useGlobal.ToString());

            _instance.LoadSettings();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  IMAGE SECTION
        // ─────────────────────────────────────────────────────────────────────
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing pictures";
                if (!string.IsNullOrWhiteSpace(TxtFolderPath.Text))
                    dialog.SelectedPath = TxtFolderPath.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TxtFolderPath.Text = dialog.SelectedPath;
                    _instance.WidgetObject.WidgetManager?.StoreSetting(
                        _instance, "PictureFolderPath", dialog.SelectedPath);
                    _instance.LoadSettings();
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DESIGN SECTION
        // ─────────────────────────────────────────────────────────────────────
        private void GlobalThemeCheck_Click(object sender, RoutedEventArgs e)
        {
            _useGlobal = GlobalThemeCheck.IsChecked ?? false;
            ApplyGlobalThemeState();
            SaveAll();
        }

        private void ApplyGlobalThemeState()
        {
            BgColorSelect.IsEnabled      = !_useGlobal;
            OverlayColorSelect.IsEnabled = !_useGlobal;
            OverlayFontSelect.IsEnabled  = !_useGlobal;
        }

        private void ColorSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button caller)) return;

            Color defaultColor;
            try { defaultColor = ColorTranslator.FromHtml(caller.Content.ToString()); }
            catch { defaultColor = Color.Black; }

            Color selected = _instance.WidgetObject.WidgetManager.RequestColorSelection(defaultColor);
            caller.Content = ColorTranslator.ToHtml(selected);

            try { _bgColor      = ColorTranslator.FromHtml(BgColorSelect.Content.ToString()); }      catch { }
            try { _overlayColor = ColorTranslator.FromHtml(OverlayColorSelect.Content.ToString()); } catch { }

            SaveAll();
        }

        private void TextOverlay_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveAll();
        }

        private void OverlayFontSelect_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Font selected = _instance.WidgetObject.WidgetManager.RequestFontSelection(_overlayFont);

            _overlayFont = selected;
            OverlayFontSelect.Content = new FontConverter().ConvertToInvariantString(selected);
            OverlayFontSelect.Tag = selected;

            SaveAll();
        }

        private void OverlayPos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OverlayXPos.SelectedIndex == -1 || OverlayYPos.SelectedIndex == -1) return;
            _overlayXPos = OverlayXPos.SelectedIndex;
            _overlayYPos = OverlayYPos.SelectedIndex;
            SaveAll();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ADVANCED SECTION
        // ─────────────────────────────────────────────────────────────────────
        private void OverlayOffset_ValueChanged(object sender, FunctionEventArgs<double> e)
        {
            _overlayXOffset = (int)OverlayXOffset.Value;
            _overlayYOffset = (int)OverlayYOffset.Value;
            SaveAll();
        }
    }
}
