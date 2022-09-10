using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using static System.Net.WebRequestMethods;
using Windows.ApplicationModel.Preview.Holographic;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace MiXhortcut
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
//        StorageFile imageFile;
        StorageFile modelFile;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        string GenerateTileId()
        {
            // Tile IDs consist of up to 64 alphanumerics, underscore, and period.
            // Create a guid to use as a unique tile id. Use "N" format so it consists
            // solely of hex digits with no extra punctuation.
            Guid guid = Guid.NewGuid();
            return guid.ToString("N");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string tileId = GenerateTileId();

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StreamWriter SW = new StreamWriter(storageFolder.Path + "\\AppDB.dat", true);

            SW.WriteLine(tileId);
            SW.WriteLine(AppFolder.Text);
            SW.WriteLine(AppExecutable.Text);
            SW.WriteLine(AppArguments.Text);

            SW.Close();

            const string SpecialFolder = "tilecontent";
/*            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(SpecialFolder, CreationCollisionOption.OpenIfExists);
                imageFile = await imageFile.CopyAsync(folder, tileId + "-" + imageFile.Name, NameCollisionOption.ReplaceExisting);
            }
            catch
            {
                // File I/O errors are reported as exceptions.
                // rootPage.NotifyUser("Could not copy file to local storage.", NotifyType.ErrorMessage);
                return;
            } */
            var imageUri = new Uri("ms-appx:///Images/StoreLogo.png");
            //var imageUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(imageFile.Name)}");

            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(SpecialFolder, CreationCollisionOption.OpenIfExists);
                modelFile = await modelFile.CopyAsync(folder, tileId + "-" + modelFile.Name, NameCollisionOption.ReplaceExisting);
            }
            catch
            {
                // File I/O errors are reported as exceptions.
                // rootPage.NotifyUser("Could not copy file to local storage.", NotifyType.ErrorMessage);
                return;
            }            

            var modelUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(modelFile.Name)}");

            var tile = new SecondaryTile(
                    tileId, // TileId
                    tileId, // Display Name
                    "", // Arguments
                    imageUri, // Square150x150Logo
                    TileSize.Square150x150); // DesiredSize

            tile.VisualElements.MixedRealityModel.Uri = modelUri;

            tile.Arguments = tileId;

            bool created = await tile.RequestCreateAsync();
            if (created)
            {
                modelFile = null;
                AppModel.Text = "Ничего не выбрано";

                AppExecutable.Text = "";
                AppFolder.Text = "";
                AppArguments.Text = "";
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private async void PickExecutable_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".exe");
            picker.FileTypeFilter.Add(".cmd");
            picker.FileTypeFilter.Add(".bat");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            // ms-appdata content must be of the form ms-appdata:///local/...
            // Copy the picked file into our local folder.

            /*
            const string SpecialFolder = "tilecontent";
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(SpecialFolder,
                    CreationCollisionOption.OpenIfExists);
                file = await file.CopyAsync(folder, file.Name, NameCollisionOption.ReplaceExisting);
            }
            catch
            {
                // File I/O errors are reported as exceptions.
                var dialog = new MessageDialog("Could not copy file to local storage.","Error");
                await dialog.ShowAsync();
                return;
            }
            */

            AppExecutable.Text = file.Path;
        }

        private async void PickModel_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Objects3D;
            picker.FileTypeFilter.Add(".glb");

            modelFile = await picker.PickSingleFileAsync();
            if (modelFile == null)
            {
                AppModel.Text = "Ничего не выбрано";
                return;
            }

            AppModel.Text = modelFile.Path + "\\" + modelFile.Name;
        }

        private async void PickFolder_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            AppFolder.Text = folder.Path;
        }
    }
}
