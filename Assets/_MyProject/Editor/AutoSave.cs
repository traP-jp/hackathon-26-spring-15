using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AutoSave
{
    const string ManualSaveKey = "autosave@manualSave";

    const string AutoSaveKey = "auto save";
    const string AutoSavePrefabKey = "auto save prefab";
    const string AutoSaveSceneKey = "auto save scene";
    const string AutoSaveSceneTimerKey = "auto save scene timer";
    const string AutoSaveIntervalKey = "save scene interval";
    const int MinIntervalSeconds = 60;

    static double nextSaveTime;
    static bool isHierarchyChanged;

    static AutoSave()
    {
        IsManualSave = true;
        nextSaveTime = EditorApplication.timeSinceStartup + IntervalSeconds;

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && IsAutoSave)
        {
            SaveConfiguredTargets();
        }

        isHierarchyChanged = false;
    }

    static void OnEditorUpdate()
    {
        if (!isHierarchyChanged || nextSaveTime >= EditorApplication.timeSinceStartup)
        {
            return;
        }

        nextSaveTime = EditorApplication.timeSinceStartup + IntervalSeconds;
        if (IsSaveSceneTimer && IsAutoSave && !EditorApplication.isPlaying)
        {
            SaveConfiguredTargets();
        }

        isHierarchyChanged = false;
    }

    static void OnHierarchyChanged()
    {
        if (!EditorApplication.isPlaying)
        {
            isHierarchyChanged = true;
        }
    }

    static void SaveConfiguredTargets()
    {
        IsManualSave = false;
        try
        {
            if (IsSavePrefab)
            {
                AssetDatabase.SaveAssets();
            }

            if (IsSaveScene)
            {
                SaveActiveScene();
            }
        }
        finally
        {
            IsManualSave = true;
        }
    }

    static void SaveActiveScene()
    {
        if (!TryGetActiveScenePath(out _))
        {
            return;
        }

        Debug.Log($"save scene {DateTime.Now}");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    static bool TryGetActiveScenePath(out string scenePath)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        scenePath = activeScene.path;
        return activeScene.IsValid() && !string.IsNullOrEmpty(scenePath);
    }

    static string GetBackupPath(string scenePath)
    {
        return Path.Combine("Backup", scenePath);
    }

    static bool ReadBoolConfig(string key)
    {
        string value = EditorUserSettings.GetConfigValue(key);
        return string.Equals(value, bool.TrueString, StringComparison.Ordinal);
    }

    static bool IsManualSave
    {
        get => EditorPrefs.GetBool(ManualSaveKey);
        set => EditorPrefs.SetBool(ManualSaveKey, value);
    }

    static bool IsAutoSave
    {
        get => ReadBoolConfig(AutoSaveKey);
        set => EditorUserSettings.SetConfigValue(AutoSaveKey, value.ToString());
    }

    static bool IsSavePrefab
    {
        get => ReadBoolConfig(AutoSavePrefabKey);
        set => EditorUserSettings.SetConfigValue(AutoSavePrefabKey, value.ToString());
    }

    static bool IsSaveScene
    {
        get => ReadBoolConfig(AutoSaveSceneKey);
        set => EditorUserSettings.SetConfigValue(AutoSaveSceneKey, value.ToString());
    }

    static bool IsSaveSceneTimer
    {
        get => ReadBoolConfig(AutoSaveSceneTimerKey);
        set => EditorUserSettings.SetConfigValue(AutoSaveSceneTimerKey, value.ToString());
    }

    static int IntervalSeconds
    {
        get
        {
            string rawValue = EditorUserSettings.GetConfigValue(AutoSaveIntervalKey);
            if (!int.TryParse(rawValue, out int parsed))
            {
                parsed = MinIntervalSeconds;
            }

            return Math.Max(parsed, MinIntervalSeconds);
        }
        set
        {
            int clamped = Math.Max(value, MinIntervalSeconds);
            EditorUserSettings.SetConfigValue(AutoSaveIntervalKey, clamped.ToString());
        }
    }

    [SettingsProvider]
    static SettingsProvider CreateSettingsProvider()
    {
        return new SettingsProvider("Preferences/Auto Save", SettingsScope.User)
        {
            label = "Auto Save",
            guiHandler = _ => DrawSettingsGui(),
            keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Auto Save",
                "Backup",
                "Scene",
                "Prefab"
            }
        };
    }

    static void DrawSettingsGui()
    {
        bool isAutoSave = EditorGUILayout.BeginToggleGroup("auto save", IsAutoSave);
        IsAutoSave = isAutoSave;
        EditorGUILayout.Space();

        IsSavePrefab = EditorGUILayout.ToggleLeft("save prefab", IsSavePrefab);
        IsSaveScene = EditorGUILayout.ToggleLeft("save scene", IsSaveScene);
        IsSaveSceneTimer = EditorGUILayout.BeginToggleGroup("save scene interval", IsSaveSceneTimer);
        IntervalSeconds = EditorGUILayout.IntField("interval(sec) min60sec", IntervalSeconds);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndToggleGroup();
    }

    [MenuItem("File/Backup/Backup", true)]
    static bool ValidateBackup()
    {
        return TryGetActiveScenePath(out _);
    }

    [MenuItem("File/Backup/Backup")]
    static void Backup()
    {
        if (!TryGetActiveScenePath(out string scenePath))
        {
            Debug.LogWarning("Cannot backup because active scene has not been saved.");
            return;
        }

        string exportPath = GetBackupPath(scenePath);
        string directoryPath = Path.GetDirectoryName(exportPath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.Copy(scenePath, exportPath, true);
    }

    [MenuItem("File/Backup/Rollback", true)]
    static bool ValidateRollback()
    {
        if (!TryGetActiveScenePath(out string scenePath))
        {
            return false;
        }

        return File.Exists(GetBackupPath(scenePath));
    }

    [MenuItem("File/Backup/Rollback")]
    static void RollBack()
    {
        if (!TryGetActiveScenePath(out string scenePath))
        {
            Debug.LogWarning("Cannot rollback because active scene has not been saved.");
            return;
        }

        string exportPath = GetBackupPath(scenePath);
        if (!File.Exists(exportPath))
        {
            Debug.LogWarning($"Backup file not found: {exportPath}");
            return;
        }

        File.Copy(exportPath, scenePath, true);
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }
}
