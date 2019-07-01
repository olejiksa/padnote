using Microsoft.ApplicationInsights;
using Notepad;
using Notepad.Helpers;
using Notepad.Pages;
using SecondaryViewsHelpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Notepad
{
    sealed partial class App : Application
    {
        public static bool show = true;
        public static string Title = StringHelper.GetString("Untitled");

        public static bool bold = false;
        public static bool italic = false;
        public static bool underline = false;

        public static SystemNavigationManager NavigationManager;
        public static Frame Frame;
        public ObservableCollection<ViewLifetimeControl> SecondaryViews = new ObservableCollection<ViewLifetimeControl>();
        private CoreDispatcher mainDispatcher;
        public CoreDispatcher MainDispatcher
        {
            get
            {
                return mainDispatcher;
            }
        }

        private int mainViewId;
        internal static bool wrapWork;
        internal static bool spellWork;

        public int MainViewId
        {
            get
            {
                return mainViewId;
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
#if DEBUG
            var culture = new System.Globalization.CultureInfo("en");
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
#endif
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 320));
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;

                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += OnBackRequested;

                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = Frame = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
                ApplicationView.GetForCurrentView().Title = StringHelper.GetString("Untitled");
            }
            else
            {
                // Выполняется для всех устройств, кроме смартфонов.
                if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    var selectedView = await createMainPageAsync();
                    if (null != selectedView)
                    {
                        selectedView.StartViewInUse();
                        var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                            selectedView.Id,
                            ViewSizePreference.Default,
                            ApplicationView.GetForCurrentView().Id,
                            ViewSizePreference.Default
                            );

                        await selectedView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {

                            var currentPage = ((Frame)Window.Current.Content).Content;
                            Window.Current.Activate();
                        });

                        selectedView.StopViewInUse();
                    }
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        partial void Construct();
        partial void OverrideOnLaunched(LaunchActivatedEventArgs args, ref bool handled);
        partial void InitializeRootFrame(Frame frame);

        partial void OverrideOnLaunched(LaunchActivatedEventArgs args, ref bool handled)
        {
            // Check if a secondary view is supposed to be shown
            ViewLifetimeControl ViewLifetimeControl;
            handled = TryFindViewLifetimeControlForViewId(args.CurrentlyShownApplicationViewId, out ViewLifetimeControl);
            if (handled)
            {
                var task = ViewLifetimeControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Window.Current.Activate();
                });
            }
        }

        partial void InitializeRootFrame(Frame frame)
        {
            mainDispatcher = Window.Current.Dispatcher;
            mainViewId = ApplicationView.GetForCurrentView().Id;
        }

        bool TryFindViewLifetimeControlForViewId(int viewId, out ViewLifetimeControl foundData)
        {
            foreach (var ViewLifetimeControl in SecondaryViews)
            {
                if (ViewLifetimeControl.Id == viewId)
                {
                    foundData = ViewLifetimeControl;
                    return true;
                }
            }
            foundData = null;
            return false;
        }

        public static async Task<ViewLifetimeControl> createMainPageAsync()
        {
            ViewLifetimeControl viewControl = null;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // This object is used to keep track of the views and important
                // details about the contents of those views across threads
                // In your app, you would probably want to track information
                // like the open document or page inside that window
                viewControl = ViewLifetimeControl.CreateForCurrentView();
                viewControl.Title = StringHelper.GetString("Untitled");
                // Increment the ref count because we just created the view and we have a reference to it                
                viewControl.StartViewInUse();

                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 320));

                var frame = new Frame();

                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += (a, b) => frame.GoBack();

                frame.Navigate(typeof(MainPage), viewControl);
                Window.Current.Content = frame;
                // This is a change from 8.1: In order for the view to be displayed later it needs to be activated.
                Window.Current.Activate();
                ApplicationView.GetForCurrentView().Title = viewControl.Title;
            });

            ((App)Current).SecondaryViews.Add(viewControl);

            return viewControl;
        }

        private async Task<ViewLifetimeControl> createMainPageAsync(string name)
        {
            ViewLifetimeControl viewControl = null;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // This object is used to keep track of the views and important
                // details about the contents of those views across threads
                // In your app, you would probably want to track information
                // like the open document or page inside that window
                viewControl = ViewLifetimeControl.CreateForCurrentView();
                viewControl.Title = name;
                // Increment the ref count because we just created the view and we have a reference to it                
                viewControl.StartViewInUse();

                var frame = new Frame();
                frame.Navigate(typeof(MainPage), viewControl);
                Window.Current.Content = frame;
                // This is a change from 8.1: In order for the view to be displayed later it needs to be activated.
                Window.Current.Activate();
                ApplicationView.GetForCurrentView().Title = viewControl.Title;
            });

            ((App)Current).SecondaryViews.Add(viewControl);

            return viewControl;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            // TODO: Handle file activation

            // The number of files received is args.Files.Size
            // The first file is args.Files[0].Name

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 320));

                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += (a, b) => OnBackRequested(a, b);

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), args.Files[0]);
                ApplicationView.GetForCurrentView().Title = args.Files[0].Name.Substring(0, args.Files[0].Name.Length - 4);

                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += OnBackRequested;
            }
            else
            {
                if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    var selectedView = await createMainPageAsync(args.Files[0].Name.Substring(0, args.Files[0].Name.Length - 4));
                    if (null != selectedView)
                    {
                        selectedView.StartViewInUse();
                        var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                            selectedView.Id,
                            ViewSizePreference.Default,
                            ApplicationView.GetForCurrentView().Id,
                            ViewSizePreference.Default
                            );

                        await selectedView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ((Frame)Window.Current.Content).Navigate(typeof(MainPage), args.Files[0]);
                            Window.Current.Activate();
                        });

                        selectedView.StopViewInUse();
                    }

                    NavigationManager = SystemNavigationManager.GetForCurrentView();
                    NavigationManager.BackRequested += OnBackRequested;
                }
            }

            Window.Current.Activate();
        }

        /// <summary>
        /// Происходит при нажатии аппаратной кнопки "Назад".
        /// </summary>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;

            if (frame.BackStack.Count > 0)
            {
                e.Handled = true;
                frame.GoBack();
            }
        }
    }
}
