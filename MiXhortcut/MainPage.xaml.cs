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
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using System.Diagnostics;

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

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            new StreamWriter(storageFolder.Path + "\\AppDB.dat", true).Close(); // Если файла ещё нет - создать его

            StreamReader SR = new StreamReader(storageFolder.Path + "\\AppDB.dat", true);

            while (!SR.EndOfStream)
            {
                SR.ReadLine();
                string AppFolder1 = SR.ReadLine();
                string AppExecutable1 = SR.ReadLine();
                string AppArguments1 = SR.ReadLine();

                ShortcutsList.Items.Add(AppExecutable1 + " " + AppArguments1 + Convert.ToChar(9) + " @ " + AppFolder1);
            }
            SR.Close();
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
//            string tileId = GenerateTileId();
            string tileId = "";
            Uri imageUri = new Uri("ms-appx:///Images/StoreLogo.png");
            Uri modelUri = null;
            const string SpecialFolder = "tilecontent";

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            new StreamWriter(storageFolder.Path + "\\AppDB.dat", true).Close(); // Если файла ещё нет - создать его

            StreamReader SR = new StreamReader(storageFolder.Path + "\\AppDB.dat", true);

            while (!SR.EndOfStream)
            {
                string tileId1 = SR.ReadLine();
                string AppFolder1 = SR.ReadLine();
                string AppExecutable1 = SR.ReadLine();
                string AppArguments1 = SR.ReadLine();

                if ((AppFolder1 == AppFolder.Text) && (AppExecutable1 == AppExecutable.Text) && (AppArguments1 == AppArguments.Text) && (AppModel.Text == "(Модель уже загружена)"))
                {
                    tileId = tileId1;
//                    imageUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(tileId + "-" + imageFile.Name)}");
                    break;
                }
            }

            SR.Close();

            if (tileId == "")  // Если нет тайла с такими параметрами - дописать в файл
            {
                tileId = GenerateTileId();


                StreamWriter SW = new StreamWriter(storageFolder.Path + "\\AppDB.dat", true);

                SW.WriteLine(tileId);
                SW.WriteLine(AppFolder.Text);
                SW.WriteLine(AppExecutable.Text);
                SW.WriteLine(AppArguments.Text);

                SW.Close();

                ShortcutsList.Items.Add(AppExecutable.Text + " " + AppArguments.Text + Convert.ToChar(9) + " @ " + AppFolder.Text);

                try
                {
                    StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(SpecialFolder, CreationCollisionOption.OpenIfExists);
                    modelFile = await modelFile.CopyAsync(folder, tileId + ".glb", NameCollisionOption.ReplaceExisting);
                }
                catch
                {
                    // File I/O errors are reported as exceptions.
                    // rootPage.NotifyUser("Could not copy file to local storage.", NotifyType.ErrorMessage);
                    return;
                }

//                modelUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(modelFile.Name)}");
            }

            modelUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(tileId + ".glb")}");

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

            //var imageUri = new Uri($"ms-appdata:///local/{SpecialFolder}/{Uri.EscapeDataString(imageFile.Name)}");

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

                ShortcutsList.SelectedIndex = 0;
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

        private void rootPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ShortcutsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = 0;

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            new StreamWriter(storageFolder.Path + "\\AppDB.dat", true).Close(); // Если файла ещё нет - создать его

            StreamReader SR = new StreamReader(storageFolder.Path + "\\AppDB.dat", true);

            while (!SR.EndOfStream)
            {
                string tileId1 = SR.ReadLine();
                string AppFolder1 = SR.ReadLine();
                string AppExecutable1 = SR.ReadLine();
                string AppArguments1 = SR.ReadLine();

                if (index == ShortcutsList.SelectedIndex)
                {
                    AppModel.Text = "(Модель уже загружена)";

                    AppExecutable.Text = AppExecutable1;
                    AppFolder.Text = AppFolder1;
                    AppArguments.Text = AppArguments1;
                    break;
                }

                index++;
            }
            SR.Close();
        }

        async private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ShortcutsList.SelectedIndex == -1) return;

            int index = 0;

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            new StreamWriter(storageFolder.Path + "\\AppDB.dat", true).Close(); // Если файла ещё нет - создать его

            StreamReader SR = new StreamReader(storageFolder.Path + "\\AppDB.dat", true);
            StreamWriter SW = new StreamWriter(storageFolder.Path + "\\Temporary.dat", false);

            while (!SR.EndOfStream)
            {
                string tileId1 = SR.ReadLine();
                string AppFolder1 = SR.ReadLine();
                string AppExecutable1 = SR.ReadLine();
                string AppArguments1 = SR.ReadLine();

                if (index != ShortcutsList.SelectedIndex)
                {
                    SW.WriteLine(tileId1);
                    SW.WriteLine(AppFolder1);
                    SW.WriteLine(AppExecutable1);
                    SW.WriteLine(AppArguments1);
                }

                index++;
            }
            SR.Close();
            SW.Close();
            ShortcutsList.Items.Remove(ShortcutsList.Items[ShortcutsList.SelectedIndex]);

            
            var FD = await storageFolder.GetFileAsync("Temporary.dat");
            await FD.CopyAsync(storageFolder, "AppDB.dat", NameCollisionOption.ReplaceExisting);
        }
    }
}
