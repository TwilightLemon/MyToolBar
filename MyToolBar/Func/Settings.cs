using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace MyToolBar.Func
{
    public static class Settings
    {
        public static string MainPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , "MyToolBar");
        private static string CachePath =>
            Path.Combine(MainPath, "Settings");
        public static void LoadPath() { 
            if (!Directory.Exists(MainPath))
                Directory.CreateDirectory(MainPath);
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }
        public static async Task Save<T>(T Data,string Sign)where T:class
        {
            string path = Path.Combine(CachePath, Sign + ".json");
            var fs=File.Create(path);
            await JsonSerializer.SerializeAsync<T>(fs, Data, new JsonSerializerOptions { WriteIndented = true });
            fs.Close();
        }
        public static async Task<T?> Load<T>(string Sign)where T:class
        {
            string path = Path.Combine(CachePath, Sign + ".json");
            if (!File.Exists(path))
                return default;
            var fs = File.OpenRead(path);
            var data = await JsonSerializer.DeserializeAsync<T>(fs);
            fs.Close();
            return data;
        }
    }
}
