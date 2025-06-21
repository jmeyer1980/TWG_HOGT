using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace TinyWalnutGames.Localization
{
    [CreateAssetMenu(fileName = "LocalizationTable (WebGL)", menuName = "Localization/ScriptableTable")]
    public partial class LocalizationTable : ScriptableObject
    {
        [System.Serializable]
        public class LocalizationEntry
        {
            public string key; // Unique identifier for the localization entry, e.g., "play_tooltip", "settings_tooltip", "btn_play_label", etc.
            public string sourceText; // debugging text in english
            public string targetText; // translated text in the target language
            public string localeCode; // Optional locale code, e.g., "en", "fr", etc. This can be used to identify the language of the entry.
        }

        public List<LocalizationEntry> entries = new();

        public void LoadFromXLIFF(string xliffText)
        {
            entries.Clear();
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xliffText);

            XmlNamespaceManager nsmgr = new(xmlDoc.NameTable);
            nsmgr.AddNamespace("x", xmlDoc.DocumentElement.NamespaceURI);

            XmlNodeList units = xmlDoc.GetElementsByTagName("unit");
            foreach (XmlNode unit in units)
            {
                var entry = new LocalizationEntry()
                {
                    key = unit.Attributes["name"]?.Value ?? ""
                };

                // Optionally extract locale code if available as an attribute
                if (unit.Attributes["locale"] != null)
                    entry.localeCode = unit.Attributes["locale"].Value;

                XmlNode segmentNode = unit.SelectSingleNode("segment") ?? unit.SelectSingleNode("x:segment", nsmgr);
                XmlNode sourceNode = segmentNode?.SelectSingleNode("source") ?? segmentNode?.SelectSingleNode("x:source", nsmgr);
                XmlNode targetNode = segmentNode?.SelectSingleNode("target") ?? segmentNode?.SelectSingleNode("x:target", nsmgr);

                entry.sourceText = sourceNode?.InnerText ?? "";
                entry.targetText = targetNode?.InnerText ?? "";

                entries.Add(entry);
            }
        }

        public void LoadFromXLIFF2(string xliffText, string filename)
        {
            entries.Clear();

            // Extract locale code from filename, e.g., HOGTxliff_en.xlf -> "en"
            string localeCodeFromFilename = ExtractLocaleCodeFromFilename(filename);

            var doc = XDocument.Parse(xliffText);
            XNamespace ns = doc.Root.GetDefaultNamespace();

            foreach (var file in doc.Root.Elements(ns + "file"))
            {
                foreach (var group in file.Elements(ns + "group"))
                {
                    foreach (var unit in group.Elements(ns + "unit"))
                    {
                        string key = unit.Attribute("name")?.Value ?? unit.Attribute("id")?.Value;
                        if (string.IsNullOrEmpty(key))
                            continue;

                        // Prefer locale from unit attribute, fallback to filename
                        string localeCode = unit.Attribute("locale")?.Value ?? localeCodeFromFilename;

                        var segments = unit.Elements(ns + "segment");
                        if (!segments.Any())
                            continue;

                        foreach (var segment in segments)
                        {
                            string source = segment.Element(ns + "source")?.Value ?? "";
                            string target = segment.Element(ns + "target")?.Value ?? source;

                            var entry = new LocalizationEntry
                            {
                                key = key,
                                sourceText = source,
                                targetText = target,
                                localeCode = localeCode
                            };
                            entries.Add(entry);
                        }
                    }
                }
            }
        }

        // Helper to extract locale code from filename, e.g., HOGTxliff_en.xlf -> "en"
        private static string ExtractLocaleCodeFromFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;
            // Remove extension
            var name = System.IO.Path.GetFileNameWithoutExtension(filename);
            // Look for last '_' and take the part after it
            int underscoreIdx = name.LastIndexOf('_');
            if (underscoreIdx >= 0 && underscoreIdx < name.Length - 1)
            {
                return name[(underscoreIdx + 1)..];
            }
            return null;
        }

        private void AddEntry(string key, string value)
        {
            var entry = entries.Find(e => e.key == key);
            if (entry != null)
            {
                entry.targetText = value;
            }
            else
            {
                entries.Add(new LocalizationEntry { key = key, targetText = value });
            }
        }

        public string GetTranslation(string key)
        {
            var entry = entries.Find(e => e.key == key);
            return entry != null ? entry.targetText : key;
        }

        public void SaveToIndexedDB()
        {
            string jsonData = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("localizationData", jsonData);
            PlayerPrefs.Save();
        }

        public void LoadFromIndexedDB()
        {
            if (PlayerPrefs.HasKey("localizationData"))
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("localizationData"), this);
            }
        }
    }
}
