using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using _Project.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Editor
{
    public static class ItemDataImporter
    {
        private const string CsvPath = "Assets/_Project/Data/Items.csv";
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/ItemData/Item";
        private const string JobFolder = "Assets/_Project/ScriptableObjects/JobData";
        private static readonly string[] Columns =
        {
            "rowType", "itemID", "itemName", "width", "height", "shape", "value",
            "handlingTime", "isFragile", "jobAssetPath", "requiredMinGold", "requiredItemID"
        };

        private sealed class ItemRow
        {
            public ItemData Asset;
            public string Path, Id, Name;
            public int Width, Height, Value;
            public float HandlingTime;
            public bool Fragile;
            public bool[] Shape;
        }

        [MenuItem("Tools/Import Items from CSV")]
        private static void Import()
        {
            try
            {
                Import(File.ReadAllText(CsvPath, new UTF8Encoding(false, true)));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[ItemDataImporter] {exception.Message}");
            }
        }

        private static void Import(string csv)
        {
            var rows = ReadCsv(csv).GetEnumerator();
            using (rows)
            {
                if (!rows.MoveNext() || !rows.Current.Fields.SequenceEqual(Columns))
                    throw new FormatException($"Line 1: expected header {string.Join(",", Columns)}.");

                var assets = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
                foreach (var guid in AssetDatabase.FindAssets("t:ItemData", new[] { ItemFolder }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                    if (asset == null || string.IsNullOrWhiteSpace(asset.ItemID))
                        continue;
                    if (assets.ContainsKey(asset.ItemID))
                        throw new FormatException($"Existing ItemData assets share ID '{asset.ItemID}' ({path}).");
                    assets.Add(asset.ItemID, asset);
                }

                var items = new Dictionary<string, ItemRow>(StringComparer.OrdinalIgnoreCase);
                var jobs = new List<(int Line, JobData Asset, int Gold, string ItemId)>();
                var jobPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (rows.MoveNext())
                {
                    var (line, fields) = rows.Current;
                    if (fields.Length != Columns.Length)
                        throw Error(line, $"expected {Columns.Length} columns.");
                    switch (fields[0])
                    {
                        case "Item":
                            var item = ReadItem(fields, line, assets);
                            if (items.ContainsKey(item.Id))
                                throw Error(line, $"duplicate itemID '{item.Id}'.");
                            items.Add(item.Id, item);
                            break;
                        case "JobCondition":
                            var relativePath = fields[9].Replace('\\', '/');
                            if (!relativePath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) ||
                                relativePath.Split('/').Any(part => part.Length == 0 || part == "." || part == ".."))
                                throw Error(line, "jobAssetPath must be relative to the JobData folder.");
                            var path = $"{JobFolder}/{relativePath}";
                            var job = AssetDatabase.LoadAssetAtPath<JobData>(path);
                            if (job == null)
                                throw Error(line, $"JobData '{relativePath}' was not found.");
                            if (!jobPaths.Add(path))
                                throw Error(line, $"duplicate JobCondition for '{relativePath}'.");
                            var gold = fields[10].Length == 0 ? 0 : ReadInt(fields[10], 0, line, "requiredMinGold");
                            jobs.Add((line, job, gold, fields[11]));
                            break;
                        default:
                            throw Error(line, "rowType must be Item or JobCondition.");
                    }
                }

                foreach (var job in jobs)
                {
                    if (job.ItemId.Length > 0 && !assets.ContainsKey(job.ItemId) && !items.ContainsKey(job.ItemId))
                        throw Error(job.Line, $"requiredItemID '{job.ItemId}' was not found.");
                }

                int created = 0;
                foreach (var item in items.Values)
                {
                    var asset = item.Asset;
                    bool isNew = asset == null;
                    if (isNew)
                        asset = ScriptableObject.CreateInstance<ItemData>();

                    var serialized = new SerializedObject(asset);
                    serialized.FindProperty("itemID").stringValue = item.Id;
                    serialized.FindProperty("itemName").stringValue = item.Name;
                    serialized.FindProperty("width").intValue = item.Width;
                    serialized.FindProperty("height").intValue = item.Height;
                    serialized.FindProperty("value").intValue = item.Value;
                    serialized.FindProperty("handlingTime").floatValue = item.HandlingTime;
                    serialized.FindProperty("isFragile").boolValue = item.Fragile;
                    var shape = serialized.FindProperty("shapeSerialized");
                    shape.arraySize = item.Shape.Length;
                    for (int i = 0; i < item.Shape.Length; i++)
                        shape.GetArrayElementAtIndex(i).boolValue = item.Shape[i];
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    if (isNew)
                    {
                        AssetDatabase.CreateAsset(asset, item.Path);
                        if (!AssetDatabase.Contains(asset))
                            throw new IOException($"Could not create '{item.Path}'.");
                        assets.Add(item.Id, asset);
                        created++;
                    }
                }

                foreach (var job in jobs)
                {
                    var serialized = new SerializedObject(job.Asset);
                    serialized.FindProperty("requiredMinGold").intValue = job.Gold;
                    serialized.FindProperty("requiredItem").objectReferenceValue =
                        job.ItemId.Length == 0 ? null : assets[job.ItemId];
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"[ItemDataImporter] Created {created} items, updated {items.Count - created} items and {jobs.Count} jobs.");
            }
        }

        private static ItemRow ReadItem(string[] fields, int line, Dictionary<string, ItemData> assets)
        {
            string id = fields[1];
            if (!Regex.IsMatch(id, @"\A[A-Za-z0-9_-]+\z") || fields[2].Length == 0)
                throw Error(line, "an item needs a valid itemID and a name.");
            assets.TryGetValue(id, out var asset);
            var path = $"{ItemFolder}/{id}.asset";
            if (asset == null && (!AssetDatabase.IsValidFolder(ItemFolder) ||
                AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets).Length > 0 ||
                File.Exists(path) || File.Exists(path + ".meta") || Directory.Exists(path)))
                throw Error(line, $"cannot create an item at '{path}'.");

            int width = ReadInt(fields[3], 1, line, "width");
            int height = ReadInt(fields[4], 1, line, "height");
            var cells = fields[5].Split('|');
            if (cells.LongLength != (long)width * height ||
                cells.Any(cell => cell != "0" && cell != "1") || !cells.Contains("1"))
                throw Error(line, "shape must contain width × height cells separated by |, with at least one 1.");

            if (!float.TryParse(fields[7], NumberStyles.Float, CultureInfo.InvariantCulture, out float handlingTime) ||
                float.IsNaN(handlingTime) || float.IsInfinity(handlingTime) || handlingTime < 0.1f)
                throw Error(line, "handlingTime must be at least 0.1 (use a decimal point).");

            bool fragile;
            if (fields[8] == "0" || fields[8] == "1")
                fragile = fields[8] == "1";
            else if (!bool.TryParse(fields[8], out fragile))
                throw Error(line, "isFragile must be 0, 1, true or false.");

            return new ItemRow
            {
                Asset = asset, Path = path, Id = asset == null ? id : asset.ItemID, Name = fields[2],
                Width = width, Height = height, Shape = cells.Select(cell => cell == "1").ToArray(),
                Value = ReadInt(fields[6], 1, line, "value"), HandlingTime = handlingTime, Fragile = fragile
            };
        }

        private static int ReadInt(string text, int minimum, int line, string column)
        {
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value < minimum)
                throw Error(line, $"{column} must be an integer >= {minimum}.");
            return value;
        }

        private static FormatException Error(int line, string message) =>
            new FormatException($"Line {line}: {message}");

        // Quoted fields may contain commas, escaped quotes and newlines.
        private static IEnumerable<(int Line, string[] Fields)> ReadCsv(string csv)
        {
            using var reader = new StringReader(csv);
            var fields = new List<string>();
            var field = new StringBuilder();
            int line = 1;
            int recordLine = 1;
            bool quoted = false;
            bool closedQuote = false;
            int next;
            while ((next = reader.Read()) != -1)
            {
                char character = (char)next;
                if (character == '\r')
                {
                    if (reader.Peek() == '\n')
                        reader.Read();
                    character = '\n';
                }
                if (character == '\n')
                    line++;

                if (quoted)
                {
                    if (character != '"')
                        field.Append(character);
                    else if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        quoted = false;
                        closedQuote = true;
                    }
                    continue;
                }

                if (character == ',' || character == '\n')
                {
                    fields.Add(field.ToString().Trim());
                    field.Clear();
                    closedQuote = false;
                    if (character == '\n')
                    {
                        if (fields.Count > 1 || fields[0].Length > 0)
                            yield return (recordLine, fields.ToArray());
                        fields.Clear();
                        recordLine = line;
                    }
                }
                else if (closedQuote)
                    throw Error(recordLine, "unexpected character after a quoted field.");
                else if (character == '"' && field.Length == 0)
                    quoted = true;
                else if (character == '"')
                    throw Error(recordLine, "unexpected quote in an unquoted field.");
                else
                    field.Append(character);
            }

            if (quoted)
                throw Error(recordLine, "unterminated quoted field.");
            if (field.Length > 0 || fields.Count > 0 || closedQuote)
            {
                fields.Add(field.ToString().Trim());
                yield return (recordLine, fields.ToArray());
            }
        }
    }
}
