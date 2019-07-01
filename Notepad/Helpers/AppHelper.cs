using System;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.StartScreen;

namespace Notepad.Helpers
{
    /// <summary>
    /// Класс-помощник для общей работы с приложением.
    /// </summary>
    public static class AppHelper
    {
        public static string CurrentNewline = "\r";
        
        /// <summary>
        /// Возвращает версию приложения.
        /// </summary>
        /// <returns>Текущая версия.</returns>
        public static string GetAppVersion()
        {
            PackageVersion version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Получает список переходов для приложения в меню "Пуск" и панель задач.
        /// </summary>
        public static async void GetJumpList()
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            
            jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;

            if (jumpList.Items.Count > 0)
                jumpList.Items.RemoveAt(jumpList.Items.Count - 1);

            var item = JumpListItem.CreateWithArguments("window", StringHelper.GetString("NewWindow"));
            item.Logo = new Uri("ms-appx:///Assets/IC809410.png");

            jumpList.Items.Add(item);

            await jumpList.SaveAsync();
        }

        /// <summary>
        /// Добавляет файл в список последних открытых файлов.
        /// </summary>
        /// <param name="file">Файл.</param>
        public static void AddFileToMRU(StorageFile file)
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            string mruToken = mru.Add(file, string.Empty, RecentStorageItemVisibility.AppAndSystem);
        }
    }
}
