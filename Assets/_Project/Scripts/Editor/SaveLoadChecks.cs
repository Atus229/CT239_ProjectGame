using System;
using System.IO;
using System.Reflection;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using _Project.Scripts.Managers;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Editor
{
    public static class SaveLoadChecks
    {
        [MenuItem("Tools/Cargo Fit/Check Save and Load")]
        public static void Run()
        {
            string directory = Path.Combine(Path.GetTempPath(), "CargoFit-save-check-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var managerType = typeof(UpgradeManager).Assembly.GetType("_Project.Scripts.Managers.SaveLoadManager");
                Require(managerType != null, "SaveLoadManager is missing.");
                var write = managerType.GetMethod("WriteSnapshot", BindingFlags.Static | BindingFlags.NonPublic);
                var read = managerType.GetMethod("ReadSnapshot", BindingFlags.Static | BindingFlags.NonPublic);
                Require(write != null && read != null, "Snapshot I/O methods are missing.");

                string path = Path.Combine(directory, "savegame.json");
                Require(read.Invoke(null, new object[] { path }) == null, "A missing save should return no snapshot.");
                string json = "{\"currentDayIndex\":4,\"playerGold\":230,\"unlockedUpgradeIDs\":[1]," +
                    "\"playerUpgrades\":[{\"playerUpgradeID\":1,\"playerID\":1,\"upgradeID\":1,\"currentLevel\":2}]}";
                object snapshot = JsonUtility.FromJson(json, write.GetParameters()[1].ParameterType);
                write.Invoke(null, new[] { path, snapshot });
                object loaded = read.Invoke(null, new object[] { path });
                var loadedType = loaded.GetType();
                Require((int)loadedType.GetField("currentDayIndex").GetValue(loaded) == 4, "Expected to resume Day 4.");
                Require((int)loadedType.GetField("playerGold").GetValue(loaded) == 230, "Expected 230 gold.");
                var upgrades = (System.Collections.Generic.List<Player_Upgrade>)loadedType.GetField("playerUpgrades").GetValue(loaded);
                Require(upgrades.Count == 1 && upgrades[0].UpgradeID == 1 && upgrades[0].CurrentLevel == 2, "Expected upgrade 1 at level 2.");

                loadedType.GetField("playerGold").SetValue(loaded, 130);
                write.Invoke(null, new[] { path, loaded });
                object replaced = read.Invoke(null, new object[] { path });
                Require((int)loadedType.GetField("playerGold").GetValue(replaced) == 130, "Expected the updated balance after replacing an existing save.");

                string original = File.ReadAllText(path);
                Directory.CreateDirectory(path + ".tmp");
                bool failed = false;
                try
                {
                    write.Invoke(null, new[] { path, snapshot });
                }
                catch (TargetInvocationException e) when (e.InnerException is IOException || e.InnerException is UnauthorizedAccessException)
                {
                    failed = true;
                }
                Require(failed && File.ReadAllText(path) == original, "A failed write must preserve the existing save.");
                File.WriteAllText(path, "{\"currentDayIndex\":0}");
                bool invalidRejected = false;
                try
                {
                    read.Invoke(null, new object[] { path });
                }
                catch (TargetInvocationException e) when (e.InnerException is InvalidDataException)
                {
                    invalidRejected = true;
                }
                Require(invalidRejected, "An invalid snapshot must be rejected.");
                Debug.Log("[SaveLoadChecks] Passed: missing file, JSON round trip, replacement, failed write preservation and invalid data.");
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
