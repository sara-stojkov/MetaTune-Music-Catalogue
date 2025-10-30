namespace Core.Utils
{
    public static class Env
    {
        private static bool _loaded = false;

        /// <summary>
        /// Loads .env file from a path, default current folder
        /// </summary>
        public static void Load(string relativePathFromExe = ".env")
        {
            if (_loaded) return;

            var fullPath = Path.GetFullPath(relativePathFromExe);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($".env file not found at {fullPath}");
            }

            DotNetEnv.Env.Load(fullPath);
            _loaded = true;
        }
    }
}
