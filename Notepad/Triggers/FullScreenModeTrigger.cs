using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Notepad.Triggers
{
    /// <summary>
    /// Адаптивный триггер для полноэкранного режима.
    /// </summary>
    public class FullScreenModeTrigger : StateTriggerBase
    {
        /// <summary>
        /// Полноэкранно ли приложение в данный момент.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        /// <summary>
        /// Свойство полноэкранности.
        /// </summary>
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool),
            typeof(FullScreenModeTrigger),
            new PropertyMetadata(false, OnIsFullScreenPropertyChanged));

        private static void OnIsFullScreenPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var obj = (FullScreenModeTrigger)d;
            if (!DesignMode.DesignModeEnabled)
            {
                var isFullScreen = ApplicationView.GetForCurrentView().IsFullScreenMode;
                obj.UpdateTrigger(isFullScreen);
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public FullScreenModeTrigger()
        {
            if (!DesignMode.DesignModeEnabled)
                ApplicationView.GetForCurrentView().VisibleBoundsChanged +=
                    FullScreenModeTrigger_VisibleBoundsChanged;
        }

        private void FullScreenModeTrigger_VisibleBoundsChanged(ApplicationView sender,
            object args)
        {
            UpdateTrigger(sender.IsFullScreenMode);
        }

        /// <summary>
        /// Обновление триггера.
        /// </summary>
        /// <param name="isFullScreen">Полноэкранно ли приложение в данный момент.</param>
        private void UpdateTrigger(bool isFullScreen)
        {
            SetActive(isFullScreen == IsFullScreen);
        }
    }
}