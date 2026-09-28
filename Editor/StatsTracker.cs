using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Collects Editor Statistics And Feeds Both The Total (LoadManager) & Session (DailyLoadManager) Data
[InitializeOnLoad]
static class StatsTracker
{
    const string ReloadStartKey = "StatsMonitor.ReloadStart";
    const string PlayModeStartKey = "StatsMonitor.PlayModeStart";

    static bool _incrementedThisCompilation = false;

    static StatsTracker() {
        // Assembly Reload
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        // Compilation
        CompilationPipeline.compilationStarted += OnCompilationStarted;
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        EditorApplication.quitting += OnQuit;

        EditorSceneManager.sceneOpened += OnSceneOpened;

        Undo.undoRedoEvent += OnUndoOrRedo;

        Application.logMessageReceived += OnLogReceived;

        CountProjectSession();
        SaveAll();
    }

    #region Event Subscribers

    static void OnBeforeAssemblyReload() {
        StartTimer(ReloadStartKey);
    }

    static void OnAfterAssemblyReload() {
        double elapsedSeconds = StopTimer(ReloadStartKey);
        if (elapsedSeconds < 0) { return; }

        LoadManager.Instance.AddDomainReloadTime(elapsedSeconds);
        DailyLoadManager.Instance.AddDomainReloadTime(elapsedSeconds);
        SaveAll();
    }

    static void OnCompilationStarted(object obj) {
        _incrementedThisCompilation = false;
    }

    static void OnCompilationFinished(object obj) {
        if (_incrementedThisCompilation) { return; }

        LoadManager.Instance.IncrementCompiled();
        DailyLoadManager.Instance.IncrementCompiled();
        SaveAll();
        _incrementedThisCompilation = true;
    }

    static void OnPlaymodeStateChanged(PlayModeStateChange change) {
        switch (change) {
            case PlayModeStateChange.ExitingEditMode:
                LoadManager.Instance.IncrementPlayPressed();
                DailyLoadManager.Instance.IncrementPlayPressed();
                SaveAll();
                break;
            case PlayModeStateChange.EnteredPlayMode:
                StartTimer(PlayModeStartKey);
                break;
            case PlayModeStateChange.ExitingPlayMode:
                double elapsedSeconds = StopTimer(PlayModeStartKey);
                if (elapsedSeconds >= 0) {
                    LoadManager.Instance.AddPlayModeTime(elapsedSeconds);
                    DailyLoadManager.Instance.AddPlayModeTime(elapsedSeconds);
                    SaveAll();
                }
                break;
            default:
                break;
        }
    }

    static void OnQuit() {
        // Total: Add This Session To The Lifetime Data
        double sessionSeconds = EditorApplication.timeSinceStartup;
        LoadManager.Instance.AddTime((float)sessionSeconds);
        if (sessionSeconds > LoadManager.Instance.LongestSession()) {
            LoadManager.Instance.SetLongestSession(sessionSeconds);
        }
        LoadManager.Instance.Opened(false);

        DailyLoadManager.Instance.Reset();

        SaveAll();
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
        LoadManager.Instance.IncrementScenesOpened();
        DailyLoadManager.Instance.IncrementScenesOpened();
        SaveAll();
    }

    static void OnUndoOrRedo(in UndoRedoInfo undo) {
        if (undo.isRedo) {
            LoadManager.Instance.IncrementRedo();
            DailyLoadManager.Instance.IncrementRedo();
        }
        else {
            LoadManager.Instance.IncrementUndo();
            DailyLoadManager.Instance.IncrementUndo();
        }
    }

    static void OnLogReceived(string condition, string stackTrace, LogType type) {
        LoadManager.Instance.IncrementLog(type);
        DailyLoadManager.Instance.IncrementLog(type);
    }

    #endregion


    #region Helpers

    static void CountProjectSession() {
        if (LoadManager.Instance.GetOpened()) { return; }

        LoadManager.Instance.IncrementOpened();
        LoadManager.Instance.Opened(true);
    }

    static void SaveAll() {
        LoadManager.Instance.Save();
        DailyLoadManager.Instance.Save();
    }

    // Stores The Current Timestamp In EditorPrefs So It Survives A Domain Reload
    static void StartTimer(string key) {
        EditorPrefs.SetString(key, Stopwatch.GetTimestamp().ToString());
    }

    // Returns The Seconds Elapsed Since StartTimer And Clears The Key. Returns -1 If It Was Never Started
    static double StopTimer(string key) {
        if (!EditorPrefs.HasKey(key)) { return -1; }

        bool parsed = long.TryParse(EditorPrefs.GetString(key), out long startTimestamp);
        EditorPrefs.DeleteKey(key);
        if (!parsed) { return -1; }

        return (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
    }

    #endregion
}
