using System;
using System.IO;

namespace BMTECHRD.Pos.App.Core
{
    public static class AppPaths
    {
        public static string Root
        {
            get
            {
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BMTECHRD", "POS");
                try
                {
                    Directory.CreateDirectory(baseDir);
                }
                catch
                {
                    // ignore
                }
                return baseDir;
            }
        }

        public static string LogsFolder => Path.Combine(Root, "logs");
    }
}
