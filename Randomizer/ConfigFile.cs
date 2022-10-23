using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Rayman1Randomizer {
    public class ConfigFile
    {
        #region Configuration Functions
        private const string _configFileName = "config.json";
        public static string ConfigFilePath => Path.Combine(Directory.GetCurrentDirectory(), _configFileName);

        public static ConfigFile GetConfigFile()
        {
            if (File.Exists(ConfigFilePath)) {
                try {
                    var config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(ConfigFilePath));

                    return config;
                } catch (Exception e) {
                    MessageBox.Show($"Could not load config file, creating a new one: {e}");
                }
            }

            // No config found, create a new one and save it
            var newConfig = new ConfigFile();
            newConfig.Save();
            return newConfig;
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(this));
        }

        public static void Update(Action<ConfigFile> action)
        {
            var config = GetConfigFile();
            action.Invoke(config);
            config.Save();
        }
    #endregion

        #region Configuration Fields

        public string Seed;
        public string GamePath;
        public string MkPsxIsoPath;

        #endregion
    }
}
