#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace TinyWalnutGames.UITKTemplates.Tools.Editor
{
    /// <summary>
    /// Converts a CSV file to a JSON string.
    /// </summary>
    /// <remarks>
    /// This class reads a CSV file, converts its content into a JSON format,
    /// and returns the JSON string. It assumes the first line of the CSV contains headers.
    /// </remarks> 
    public static class CSVtoJSONConverter
    {
        public static string ConvertCSVtoJSON(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV file not found: {csvPath}");
                return "{}";
            }

            var csvLines = File.ReadAllLines(csvPath);
            if (csvLines.Length < 2) return "{}";

            string[] headers = ParseCsvLine(csvLines[0]);
            List<Dictionary<string, object>> jsonData = new();

            for (int i = 1; i < csvLines.Length; i++)
            {
                var line = csvLines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = ParseCsvLine(line);
                if (values.Length == 0) continue;

                var entry = new Dictionary<string, object>();
                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    entry[headers[j]] = ParseValue(values[j]);
                }
                jsonData.Add(entry);
            }

            if (jsonData.Count == 0) return "{}";

            var wrapped = new Dictionary<string, object> { { "locales", jsonData } };
            return JsonConvert.SerializeObject(wrapped, Formatting.Indented);
        }

        private static object ParseValue(string value)
        {
            // Try int
            if (int.TryParse(value, out int intResult))
                return intResult;

            // Try float (culture-invariant)
            if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatResult))
                return floatResult;

            // Try bool
            if (bool.TryParse(value, out bool boolResult))
                return boolResult;

            // Fallback to string
            return value;
        }


        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var value = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }
            result.Add(value.ToString());
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Trim().Trim('"');
            }
            return result.ToArray();
        }
    }
}
#endif
