using Pronia.Utilities.Enums;

namespace Pronia.Utilities.Extensions
{
    public static class FileValidator
    {
        private static string CreatePath(string filename, params string[] roots)
        {
            string path = string.Empty;

            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            return path = Path.Combine(path, filename);
        }

        private static string NameCutter(this string filename)
        {
            return filename = filename.Substring(filename.LastIndexOf('.'));
        }

        public static bool ValidateType(this IFormFile file, string type)
        {
            if (file.ContentType.Contains(type))
            {
                return true;
            }
            return false;
        }

        public static bool ValidateSize(this IFormFile file, FileSize filesize, int size)
        {
            switch (filesize)
            {
                case FileSize.KB:
                    return file.Length <= size * 1024;
                case FileSize.MB:
                    return file.Length <= size * 1024 * 1024;
                case FileSize.GB:
                    return file.Length <= size * 1024 * 1024 * 1024;
            }
            return false;
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
        {
            string filename = string.Concat(Guid.NewGuid().ToString(), file.FileName.NameCutter());

            string path = CreatePath(filename, roots);

            using (FileStream fileStream = new(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filename;

        }

        public static void DeleteFile(this string filename, params string[] roots)
        {
            string path = CreatePath(filename, roots);


            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
