using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codeterpret.Common
{
    public enum InputTypes
    {
        Text,
        Check,
        Select,
        Label,
        LineBreak
    }


    /// <summary>
    /// Used to define Settings that can be rendered on a page, and passed to code generator methods.
    /// </summary>
    public class SettingGroup
    {
        public List<Setting> Settings;

        public SettingGroup()
        {
            Settings = new List<Setting>();
        }

        /// <summary>
        /// Gets the Value of Setting with the specified Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            string ret = "";

            var kv = Settings.FirstOrDefault(x => x.Key == key);
            if (kv != null)
            {
                ret = kv.Value;
            }

            return ret;
        }

        /// <summary>
        /// A superior Display check than at the Setting level, if a related Setting with a Key that matches this Setting's OnlyDisplayWhenKey is flagged to NOT Display, this will return false as well
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ShouldDisplay(string key)
        {

            bool ret = true;
            var obj = Settings.FirstOrDefault(x => x.Key == key);

            if (obj != null)
            {
                ret = obj.Display;

                if (ret) // Only if Display is TRUE, do we want to see if we need to actually hide it...
                {
                    if (!String.IsNullOrEmpty(obj.OnlyDisplayWhenKey))
                    {
                        var r = Settings.FirstOrDefault(x => x.Key == obj.OnlyDisplayWhenKey);
                        if (r != null)
                        {
                            ret = r.Display;
                        }

                        if (ret) // If we are still set to TRUE, dig a little deeper...
                        {
                            ret = ShouldDisplay(obj.OnlyDisplayWhenKey);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a Dictionary of only the Settings that were Displayed
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetActiveSettings()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            foreach (var s in Settings)
            {
                if (ShouldDisplay(s.Key))
                {
                    ret.Add(s.Key, s.Value);
                }
            }

            return ret;
        }


        /// <summary>
        /// Loads the Settings from JSON
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<Setting> LoadFromJSON(string json)
        {
            return JsonConvert.DeserializeObject<List<Setting>>(json);
        }

        /// <summary>
        /// Returns the Settings as JSON
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(Settings);
        }
    }

    public class Setting
    {
        private string _value;

        /// <summary>
        /// The Type of Input
        /// </summary>
        public InputTypes Type { get; set; }

        /// <summary>
        /// The Key (kind of like an ElementID) of the Input
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The Text that will Display above the Input
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// If the Type is Select, these are the Select's Options
        /// </summary>
        public List<SettingOption> Options { get; set; }

        /// <summary>
        /// This Input will only be displayed if Input with Key of "OnlyDisplayWhenKey" has a Value of "OnlyDisplayWHenValue"
        /// </summary>
        public string OnlyDisplayWhenKey { get; set; }

        /// <summary>
        /// This Input will only be displayed if Input with Key of "OnlyDisplayWhenKey" has a Value of "OnlyDisplayWHenValue"
        /// </summary>
        public string OnlyDisplayWhenValue { get; set; }

        /// <summary>
        /// Indicates that OnlyDisplayWhenValue contains comma separated values where one value needs to match
        /// </summary>
        public bool OnlyDisplayWhenValueMultiValues { get; set; }

        /// <summary>
        /// By default, is the Input visible
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// Gets or Sets the Value of the Setting
        /// </summary>
        public string Value
        {
            get
            {
                if (Type == InputTypes.Select)
                {
                    var o = Options.FirstOrDefault(x => x.Selected || x.Value == _value);
                    if (o != null)
                    {
                        return o.Value;
                    }
                    else
                        return "";
                }
                else
                    return _value;
            }

            set
            {
                if (Type == InputTypes.Select)
                {
                    var o = Options.FirstOrDefault(x => x.Value == value);
                    if (o != null)
                    {
                        foreach (var s in Options.Where(x => x.Selected))
                        {
                            s.Selected = false;
                        }

                        o.Selected = true;
                    }
                }

                _value = value;

            }
        }

        /// <summary>
        ///  This is used for rendering purposes
        /// </summary>
        public int TypeIndex { get; set; }


        public Setting()
        {
            Options = new List<SettingOption>();
        }


        /// <summary>
        /// Returns the value as bool
        /// </summary>
        /// <returns></returns>
        public bool GetValueAsBool()
        {
            if (_value == "true")
                return true;
            else
                return false;
        }
    }

    public class SettingOption
    {
        public bool Selected { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }


    /// <summary>
    /// Used by the Blazor Component as a way to organize Settings for rendering
    /// </summary>
    public class SettingGroupCollection
    {
        public List<SettingGroup> SettingGroups { get; private set; }

        public SettingGroupCollection()
        {
            SettingGroups = new List<SettingGroup>();
        }

        public SettingGroupCollection(SettingGroup settingGroup)
        {
            SettingGroups = new List<SettingGroup>();

            SettingGroup temp = new SettingGroup();

            foreach (var s in settingGroup.Settings)
            {
                if (s.Type != InputTypes.LineBreak)
                {
                    temp.Settings.Add(s);
                }
                else
                {
                    SettingGroups.Add(temp);
                    temp = new SettingGroup();
                }
            }

            if (temp.Settings.Count > 0)
                SettingGroups.Add(temp);
        }
    }
}
