using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SpritzMod.Scripts.Data
{
    public static class DataUtil
    {
        private static string ToJSON<T>(T t)
        {
            return SaveManager.ToJSON(t);
        }
        
        private static T FromJSON<T>(string json)
        {
            return SaveManager.FromJSON<T>(json);
        }
        
        public static T LoadFromFile<T>(string path) where T : AData
        {
            string fullPath = Path.Combine(Plugin.Directory, path);
            
            if (!File.Exists(fullPath))
            {
                Plugin.Log.LogError("File at path '" + fullPath + "' does not exist! Creating one now.");
                
                T t = (T)Activator.CreateInstance(typeof(T));
                string json = ToJSON(t);
                File.WriteAllText(fullPath, json);
                return t;
            }

            // Read JSON
            string text = File.ReadAllText(fullPath);
            string str = text.Substring(text.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            
            // Deserialize JSON
            T loadedData = FromJSON<T>(str);
            if (loadedData == null)
            {
                Plugin.Log.LogWarning("Failed to load " + str);
            }
            else
            {
                loadedData.OnPostLoad();
            }

            return loadedData;
        }
        
        public static List<T> LoadAllInDirectory<T>(string pathDirectory, string pattern) where T : AData
        {
            Plugin.Log.LogInfo("Loading files at: " + pathDirectory);
            
            List<T> allData = new List<T>();
            if (!Directory.Exists(pathDirectory))
            {
                Plugin.Log.LogError("Directory '" + pathDirectory + "' does not exist! Creating one now.");
                Directory.CreateDirectory(pathDirectory);
                return allData;
            }

            Regex reg = new Regex(pattern);
            List<string> files = Directory.GetFiles(pathDirectory, "*", SearchOption.AllDirectories)
                .Where(path => reg.IsMatch(path))
                .ToList();
            foreach (string filePath in files)
            {
                // Read JSON
                string text = File.ReadAllText(filePath);
                string str = text.Substring(text.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        
                // Deserialize JSON
                T loadedData = FromJSON<T>(str);
                if (loadedData == null)
                {
                    Plugin.Log.LogWarning("Failed to load file at " + filePath + " with contents: " + str);
                }
                else
                {
                    Plugin.Log.LogInfo("Loaded: " + filePath);
                    allData.Add(loadedData);
                    loadedData.OnPostLoad();
                }
            } 

            return allData;
        }
    }
}