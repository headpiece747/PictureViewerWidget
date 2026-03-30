using System.Windows;
using System.Windows.Controls;

namespace PictureViewerWidget
{
    public partial class PictureViewerWidgetSettings : UserControl
    {
        private PictureViewerWidgetInstance _instance;

        public PictureViewerWidgetSettings(PictureViewerWidgetInstance instance)
        {
            InitializeComponent();
            _instance = instance;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Now routing through _instance.WidgetObject.WidgetManager
            if (_instance.WidgetObject.WidgetManager != null &&
                _instance.WidgetObject.WidgetManager.LoadSetting(_instance, "PictureFolderPath", out string path))
            {
                TxtFolderPath.Text = path;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Using WinForms FolderBrowserDialog to easily pick a folder
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing pictures";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TxtFolderPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Now routing through _instance.WidgetObject.WidgetManager
            if (_instance.WidgetObject.WidgetManager != null)
            {
                _instance.WidgetObject.WidgetManager.StoreSetting(_instance, "PictureFolderPath", TxtFolderPath.Text);

                // Tell the instance to reload the image list from the new folder
                _instance.LoadSettings();

                MessageBox.Show("Settings saved successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}