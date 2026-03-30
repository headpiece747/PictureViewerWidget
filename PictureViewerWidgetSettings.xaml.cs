using System.Windows;
using System.Windows.Controls;

namespace PictureViewerWidget
{
    public partial class PictureViewerWidgetSettings : UserControl
    {
        private PictureViewerWidgetInstance _instance;

        public PictureViewerWidgetSettings(PictureViewerWidgetInstance instance)
        {
            _instance = instance;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (_instance.WidgetObject.WidgetManager != null &&
                _instance.WidgetObject.WidgetManager.LoadSetting(_instance, "PictureFolderPath", out string path))
            {
                // Show the current folder path as the button label so the user can see it at a glance
                BtnBrowse.Content = string.IsNullOrWhiteSpace(path) ? "Browse..." : path;
                BtnBrowse.Tag = path;
            }
            else
            {
                BtnBrowse.Content = "Browse...";
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing pictures";

                // Pre-select the already-saved folder if one exists
                string current = BtnBrowse.Tag as string;
                if (!string.IsNullOrWhiteSpace(current))
                    dialog.SelectedPath = current;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;

                    BtnBrowse.Content = selectedPath;
                    BtnBrowse.Tag = selectedPath;

                    // Save immediately (same pattern as ClockWidget)
                    if (_instance.WidgetObject.WidgetManager != null)
                    {
                        _instance.WidgetObject.WidgetManager.StoreSetting(_instance, "PictureFolderPath", selectedPath);
                        _instance.LoadSettings();
                    }
                }
            }
        }
    }
}
