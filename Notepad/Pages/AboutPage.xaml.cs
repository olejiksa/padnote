using Notepad.Helpers;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Notepad.Pages
{
    /// <summary>
    /// Страница сведений о программе.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        /// <summary>
        /// Доступ к специальным возможностям.
        /// </summary>
        private AccessibilitySettings access;

        /// <summary>
        /// Надпись версии, отображаемая пользователю.
        /// </summary>
        public string Version
        {
            get { return $"{StringHelper.GetString("Version")} {AppHelper.GetAppVersion()}"; }
        }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public AboutPage()
        {
            InitializeComponent();
            access = new AccessibilitySettings();
            access.HighContrastChanged += Access_HighContrastChanged;
        }

        private void Access_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            if (access.HighContrast)
                if (access.HighContrastScheme == "High Contrast White")
                    RequestedTheme = headerPanel.RequestedTheme = ElementTheme.Light;
                else
                    RequestedTheme = headerPanel.RequestedTheme = ElementTheme.Dark;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            byte theme = 3;
            App.NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            try
            {
                theme = byte.Parse(ApplicationData.Current.LocalSettings.Values["Background"].ToString());
            }
            catch { }

            NavigationHelper.FullScreenChecking();

            if (theme == 1)
            {
                RequestedTheme = ElementTheme.Light;
                var appView = ApplicationView.GetForCurrentView();
                var titleBar = appView.TitleBar;
                titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                    Color.FromArgb(255, 220, 220, 220);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 206, 206, 206);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 192, 192, 192);
                titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.Black;
            }
            else if (theme == 2)
            {
                RequestedTheme = ElementTheme.Dark;
                var appView = ApplicationView.GetForCurrentView();
                var titleBar = appView.TitleBar;
                titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                    Color.FromArgb(255, 31, 31, 31);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 53, 53, 53);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 76, 76, 76);
                titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.White;
            }
            else
                RequestedTheme = ElementTheme.Default;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                // Задаёт для заголовка страницы на мобильных устройствах подходящие интервалы.
                headerText.Margin = new Thickness(12, 2, 0, 0);
            NavigationHelper.FrameSizeChanged(Frame, contentPanel);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            NavigationHelper.FullScreenChecking();
            App.NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void PageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            NavigationHelper.FrameSizeChanged(Frame, contentPanel);
        }
    }
}
