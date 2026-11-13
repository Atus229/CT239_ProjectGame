using System;
using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Managers
{
    public sealed class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        public static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool HasSaveData() => File.Exists(SavePath);

        public bool SaveGame()
        {
            var day = DayManager.Instance;
            var upgrades = UpgradeManager.Instance;
            if (day == null || upgrades == null || !day.CanSaveProgress) return false;

            var snapshot = new GameSaveData
            {
                currentDayIndex = day.CurrentDayIndex + 1,
                playerGold = day.PlayerGold,
                unlockedUpgradeIDs = new List<int>(),
                playerUpgrades = upgrades.ExportPlayerUpgrades()
            };
            snapshot.playerUpgrades.RemoveAll(record => record.CurrentLevel == 0);
            foreach (var record in snapshot.playerUpgrades)
                snapshot.unlockedUpgradeIDs.Add(record.UpgradeID);

            try
            {
                WriteSnapshot(SavePath, snapshot);
                return true;
            }
            catch (Exception e) when (IsFileError(e))
            {
                Debug.LogError($"[SaveLoadManager] Không thể lưu tiến trình. {e.Message}");
                return false;
            }
        }

        public bool LoadGame()
        {
            var day = DayManager.Instance;
            var upgrades = UpgradeManager.Instance;
            if (day == null || upgrades == null || day.CurrentDayIndex > 0) return false;

            GameSaveData snapshot;
            try
            {
                snapshot = ReadSnapshot(SavePath);
                if (snapshot == null) return false;

                foreach (var record in snapshot.playerUpgrades)
                {
                    var definition = upgrades.GetUpgradeData(record.UpgradeID);
                    if (definition != null && record.CurrentLevel > definition.MaxLevel)
                        throw new InvalidDataException("Cấp nâng cấp vượt quá giới hạn.");
                }
            }
            catch (Exception e) when (e is InvalidDataException || e is ArgumentException)
            {
                QuarantineSave();
                Debug.LogWarning($"[SaveLoadManager] Dữ liệu lưu bị hỏng. {e.Message}");
                return false;
            }
            catch (Exception e) when (IsFileError(e))
            {
                Debug.LogError($"[SaveLoadManager] Không thể đọc tiến trình. {e.Message}");
                return false;
            }

            snapshot.playerUpgrades.RemoveAll(record => upgrades.GetUpgradeData(record.UpgradeID) == null);
            upgrades.ImportPlayerUpgrades(snapshot.playerUpgrades);
            GridManager.Instance?.RestoreVehicleCapacity(upgrades.GetBonusGridColumns());
            day.RestoreProgress(snapshot.currentDayIndex, snapshot.playerGold);
            return true;
        }

        private static GameSaveData ReadSnapshot(string path)
        {
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }

            var snapshot = JsonUtility.FromJson<GameSaveData>(json);
            if (snapshot == null || snapshot.currentDayIndex < 1 || snapshot.currentDayIndex == int.MaxValue ||
                snapshot.playerGold < 0 || snapshot.playerUpgrades == null || snapshot.unlockedUpgradeIDs == null)
                throw new InvalidDataException("Thiếu dữ liệu bắt buộc hoặc giá trị không hợp lệ.");

            var ids = new HashSet<int>();
            foreach (var record in snapshot.playerUpgrades)
            {
                if (record == null || record.UpgradeID <= 0 || record.CurrentLevel <= 0 || !ids.Add(record.UpgradeID))
                    throw new InvalidDataException("Bản ghi nâng cấp không hợp lệ.");
            }

            if (snapshot.unlockedUpgradeIDs.Count != ids.Count || !ids.SetEquals(snapshot.unlockedUpgradeIDs))
                throw new InvalidDataException("Danh sách ID nâng cấp không khớp với cấp độ đã lưu.");

            return snapshot;
        }

        private static void WriteSnapshot(string path, GameSaveData snapshot)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporaryPath = path + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(snapshot, true));
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
                }
                catch (Exception e) when (IsFileError(e))
                {
                    Debug.LogWarning($"[SaveLoadManager] Không thể dọn file lưu tạm. {e.Message}");
                }
            }
        }

        private static void QuarantineSave()
        {
            try
            {
                File.Move(SavePath, SavePath + ".corrupt-" + Guid.NewGuid().ToString("N"));
            }
            catch (Exception e) when (IsFileError(e))
            {
                Debug.LogWarning($"[SaveLoadManager] Không thể di chuyển file lưu bị hỏng. {e.Message}");
            }
        }

        private static bool IsFileError(Exception e) => e is IOException || e is UnauthorizedAccessException;
    }
}
