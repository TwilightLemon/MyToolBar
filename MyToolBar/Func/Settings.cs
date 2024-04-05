using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public static async Task Save(object Data,string Sign)
        {
            await File.WriteAllTextAsync(Path.Combine(CachePath, Sign+".json"), ObjectToJson(Data));
        }
        public static async Task<T> Load<T>(string Sign)
        {
            string path = Path.Combine(CachePath, Sign + ".json");
            if (!File.Exists(path))
                return default;
            string json = await File.ReadAllTextAsync(path);
            return (T)JsonToObject(json, typeof(T));
        }

        public static string ObjectToJson(object obj)
          {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int) stream.Length);
            return Encoding.UTF8.GetString(dataBytes);
         }
         public static object JsonToObject(string jsonString, Type obj)
         {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj);
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return serializer.ReadObject(mStream);
         }

    }
}
