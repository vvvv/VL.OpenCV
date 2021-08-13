using System.IO;
using System.Reflection;

namespace VL.OpenCV
{
    static class HAARCascadeResolver
    {
        internal static string ResolveHaarcascadesDirectory()
        {
            // TODO: Fix nuget package in a way that this is always the same path relative from this assembly!
            var thisDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            {
                var insidePackagesFolder = Path.Combine(thisDir, "..", "..", "content", "haarcascades");
                if (Directory.Exists(insidePackagesFolder))
                {
                    return insidePackagesFolder;
                }
            }
            {
                var insideBuildOutputFolder = Path.Combine(thisDir, "content", "haarcascades");
                if (Directory.Exists(insideBuildOutputFolder))
                {
                    return insideBuildOutputFolder;
                }
            }
            return null;
        }
    }
}
