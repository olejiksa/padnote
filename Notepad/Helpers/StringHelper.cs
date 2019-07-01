using Windows.ApplicationModel.Resources;

namespace Notepad.Helpers
{
    public static class StringHelper
    {
        private static readonly ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");

        public static string GetString(string resourceName)
        {
            return resourceLoader.GetString(resourceName);
        }
    }
}
