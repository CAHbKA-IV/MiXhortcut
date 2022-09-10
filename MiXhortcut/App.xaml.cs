using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MiXhortcut
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static event EventHandler AppServiceDisconnected;
        public static event EventHandler<AppServiceTriggerDetails> AppServiceConnected;
        public static bool IsForeground = false;

        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода, поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        async void MessageBox(string Message, string Title = "")
        {
            if (Title == "")
            {
                var dialog = new MessageDialog(Message);
                await dialog.ShowAsync();
            }
            else
            {
                var dialog = new MessageDialog(Message, Title);
                await dialog.ShowAsync();
            }
        }
        async Task Launch(string AppFolder, string AppExecutable, string AppArguments)
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                ApplicationData.Current.LocalSettings.Values["AppFolder"] = AppFolder;
                ApplicationData.Current.LocalSettings.Values["AppExecutable"] = AppExecutable;
                ApplicationData.Current.LocalSettings.Values["AppArguments"] = AppArguments;

                var task = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await task;
            }
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем. Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            ApplicationView.PreferredLaunchViewSize = new Size(400, 400);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 400));

            string idOfTappedTile = e.TileId;

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            new StreamWriter(storageFolder.Path + "\\AppDB.dat", true).Close(); // Если файл не существует, создать его.

            StreamReader SW = new StreamReader(storageFolder.Path + "\\AppDB.dat", true);

            while (!SW.EndOfStream)
            {
                string tile = SW.ReadLine();
                if (tile == idOfTappedTile) break;
            }
            if (!SW.EndOfStream)
            {
                string AppFolder = SW.ReadLine();
                string AppExecutable = SW.ReadLine();
                string AppArguments = SW.ReadLine();

                var task = Launch(AppFolder, AppExecutable, AppArguments);

                SW.Close();

                await task;

                Application.Current.Exit();
            }
            else
            {
                SW.Close();

                Frame rootFrame = Window.Current.Content as Frame;

                // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
                // только обеспечьте активность окна
                if (rootFrame == null)
                {
                    // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Загрузить состояние из ранее приостановленного приложения
                    }

                    // Размещение фрейма в текущем окне
                    Window.Current.Content = rootFrame;
                }

                if (e.PrelaunchActivated == false)
                {
                    if (rootFrame.Content == null)
                    {
                        // Если стек навигации не восстанавливается для перехода к первой странице,
                        // настройка новой страницы путем передачи необходимой информации в качестве параметра
                        // навигации
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    // Обеспечение активности текущего окна
                    Window.Current.Activate();
                }
            }
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
