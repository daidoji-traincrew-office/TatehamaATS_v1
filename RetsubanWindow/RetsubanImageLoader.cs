using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TatehamaATS_v1.RetsubanWindow
{
    public static class RetsubanImageLoader
    {
        // 表示候補は有限集合（7seg数字・dot・16dot文字・マゼンタ代替）のため、
        // (baseName, size) 単位で永続キャッシュし、毎回の Bitmap 再生成によるGDI+リークを防ぐ。
        // PictureBox側で参照され続けるためここではDisposeしない。
        private static readonly Dictionary<string, Image> _cache = new();

        // attempt to load image by base name from Image\Retsuban folder.
        // If not found, return a bitmap filled with magenta (255,0,255) sized to 'size'.
        public static Image Load(string baseName, Size size)
        {
            if (string.IsNullOrEmpty(baseName)) baseName = "";
            var cacheKey = $"{baseName}|{size.Width}x{size.Height}";
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }
            var result = LoadInternal(baseName, size);
            _cache[cacheKey] = result;
            return result;
        }

        private static Image LoadInternal(string baseName, Size size)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var folder = Path.Combine(baseDir, "Image", "Retsuban");
            var triedNames = new List<string> { baseName, "_" + baseName };
            var exts = new[] { ".png", ".bmp", ".jpg", ".jpeg", ".gif", ".ico" };
            foreach (var name in triedNames)
            {
                foreach (var ext in exts)
                {
                    var p = Path.Combine(folder, name + ext);
                    if (File.Exists(p))
                    {
                        try
                        {
                            using (var fs = new FileStream(p, FileMode.Open, FileAccess.Read))
                            {
                                var img = Image.FromStream(fs);
                                if (size.Width > 0 && size.Height > 0 && (img.Width != size.Width || img.Height != size.Height))
                                {
                                    var bmp = new Bitmap(size.Width, size.Height);
                                    using (var g = Graphics.FromImage(bmp))
                                    {
                                        g.DrawImage(img, 0, 0, size.Width, size.Height);
                                    }
                                    img.Dispose();
                                    return bmp;
                                }
                                return new Bitmap(img);
                            }
                        }
                        catch
                        {
                            // fallthrough to magenta fallback
                        }
                    }
                }
            }
            // not found -> return magenta bitmap sized to control
            var w = Math.Max(1, size.Width);
            var h = Math.Max(1, size.Height);
            var bmFallback = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmFallback))
            {
                g.Clear(Color.FromArgb(255, 0, 255));
            }
            return bmFallback;
        }
    }
}
