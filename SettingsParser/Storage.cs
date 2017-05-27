using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace SettingsParser
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SettingsStore : Attribute
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public SettingsStore(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class SettingsStorage
    {
        private Dictionary<string, IConfigParser> storage = new Dictionary<string, IConfigParser>();

        public dynamic this[string settingsKey]
        {
            get
            {
                return storage[settingsKey].Settings;
            }
        }

        private Dictionary<string, Type> implementations = new Dictionary<string, Type>();
        private Dictionary<string, string> paths = new Dictionary<string, string>();

        private void LoadImplementations()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    object[] attrs = type.GetCustomAttributes(typeof(SettingsStore), true);
                    if (attrs.Length > 0)
                    {
                        implementations[(attrs[0] as SettingsStore).Name] = type;
                        paths[(attrs[0] as SettingsStore).Name] = (attrs[0] as SettingsStore).Path;
                    }
                }
            }
        }

        public void Initialize(string configFolder)
        {
            foreach (KeyValuePair<string, Type> kvpair in implementations)
            {
                Type type = typeof(ConfigParser<>).MakeGenericType(kvpair.Value);

                storage.Add(kvpair.Key, Activator.CreateInstance(type, new object[1] { Path.Combine(configFolder, paths[kvpair.Key]) }) as IConfigParser);
            }
        }

        public void Load()
        {
            foreach (IConfigParser settings in storage.Values)
            {
                settings.LoadSettings();
            }
        }

        public void Save()
        {
            foreach (IConfigParser settings in storage.Values)
            {
                settings.SaveSettings();
            }
        }
    }
}
