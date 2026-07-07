using System.Windows;

namespace WastelandForge.Desktop;

internal static class HeatAssets
{
    private const string ResourceRoot = "Assets/Heat/";

    public static Uri ResourceRootUri { get; } = new("pack://application:,,,/Assets/Heat/", UriKind.Absolute);

    public static bool TryGetResource(string relativePath, out Uri resourceUri)
    {
        resourceUri = new Uri($"pack://application:,,,/{ResourceRoot}{relativePath}", UriKind.Absolute);
        try
        {
            var info = Application.GetResourceStream(resourceUri);

            if (info is null)
            {
                return false;
            }

            info.Stream.Dispose();
            return true;
        }
        catch (System.IO.IOException)
        {
            return false;
        }
    }
}
