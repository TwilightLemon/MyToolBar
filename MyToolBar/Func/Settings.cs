using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace MyToolBar.Func
{
    public class SettingsMgr<T>where T:class
    {
        public string Sign { get;set; }
        public string PackageName { get; set; }
        public T Data { get; set; }
        [JsonIgnore]
        private FileSystemWatcher _watcher;
        public event Action<T> OnDataChanged;
        public SettingsMgr() { }
        public SettingsMgr(string Sign,string pkgName)
        {
            this.Sign = Sign;
            this.PackageName = pkgName;
            if (!GlobalService.ManagedSettingsKey.Contains(Sign))
                GlobalService.ManagedSettingsKey.Add(Sign);
            _watcher=new FileSystemWatcher(Settings.CachePath)
            {
                Filter = Sign + ".json",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _watcher.Changed += _watcher_Changed;
        }
        ~SettingsMgr()
        {
            _watcher?.Dispose();
        }
        public async Task Load()
        {
            Debug.WriteLine(Sign + "   Load!!");
            var dt = await Settings.Load<SettingsMgr<T>>(Sign);
            if (dt != null)
                Data = dt.Data;
            else{
                Data = Activator.CreateInstance<T>();
                await Save();
            }
        }
        public async Task Save()
        {
            Debug.WriteLine(Sign + "   SAVE!!");
            _watcher.EnableRaisingEvents = false;
            await Settings.Save(this, Sign);
            _watcher.EnableRaisingEvents = true;
        }
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (DateTime.Now - _lastUpdateTime > TimeSpan.FromSeconds(1))
            {
                _lastUpdateTime = DateTime.Now;
                Debug.WriteLine(Sign + "   changed!!");
                OnDataChanged?.Invoke(Data);
            }
            else
            {
                Debug.WriteLine(Sign + "   Change canceled!!");
            }
        }
    }
    public static class Settings
    {
        public static string MainPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyToolBar");
        public static string CachePath =>
            Path.Combine(MainPath, "Settings");
        public static void LoadPath()
        {
            if (!Directory.Exists(MainPath))
                Directory.CreateDirectory(MainPath);
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }
        public static string GetPathBySign(string Sign) => Path.Combine(CachePath, Sign + ".json");
        public static async Task Save<T>(T Data, string Sign) where T : class
        {
            string path = GetPathBySign(Sign);
            var fs = File.Create(path);
            await JsonSerializer.SerializeAsync<T>(fs, Data, new JsonSerializerOptions { WriteIndented = true });
            fs.Close();
        }
        public static async Task<T?> Load<T>(string Sign) where T : class
        {
            try
            {
                string path = GetPathBySign(Sign);
                if (!File.Exists(path))
                    return null;
                var fs = File.OpenRead(path);
                var data = await JsonSerializer.DeserializeAsync<T>(fs);
                fs.Close();
                return data;
            }catch
            {
                return null;
            }
        }
    }
}
