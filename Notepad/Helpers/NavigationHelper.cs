using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Notepad.Helpers
{
    /// <summary>
    /// Класс-помощник навигации.
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// Изменяет отступы содержимого страницы в зависимости от размера Frame.
        /// </summary>
        /// <param name="frame">Объект Frame.</param>
        /// <param name="contentPanel">Объект StackPanel (включающий содержимое).</param>
        public static void FrameSizeChanged(Frame frame, StackPanel contentPanel)
        {
            if (frame.ActualWidth < 480)
                contentPanel.Margin = new Thickness(15, 20, 15, 0);
            else
                contentPanel.Margin = new Thickness(30, 20, 30, 0);
        }

        /// <summary>
        /// Проверяет состояние опции полноэкранного режима.
        /// </summary>
        public static void FullScreenChecking()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("FullScreen"))
            {
                if (bool.Parse((string)ApplicationData.Current.LocalSettings.Values["FullScreen"]))
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                else
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
        }
    }
}
