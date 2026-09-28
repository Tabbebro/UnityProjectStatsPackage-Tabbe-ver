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

        LoadManagerTotal.Instance.AddDomainReloadTime(elapsedSeconds);
        LoadManagerSession.Instance.AddDomainReloadTime(elapsedSeconds);
        SaveAll();
    }

    static void OnCompilationStarted(object obj) {
        _incrementedThisCompilation = false;
    }

    static void OnCompilationFinished(object obj) {
        if (_incrementedThisCompilation) { return; }

        LoadManagerTotal.Instance.IncrementCompiled();
        LoadManagerSession.Instance.IncrementCompiled();
        SaveAll();
        _incrementedThisCompilation = true;
    }

    static void OnPlaymodeStateChanged(PlayModeStateChange change) {
        switch (change) {
            case PlayModeStateChange.ExitingEditMode:
                LoadManagerTotal.Instance.IncrementPlayPressed();
                LoadManagerSession.Instance.IncrementPlayPressed();
                SaveAll();
                break;
            case PlayModeStateChange.EnteredPlayMode:
                StartTimer(PlayModeStartKey);
                break;
            case PlayModeStateChange.ExitingPlayMode:
                double elapsedSeconds = StopTimer(PlayModeStartKey);
                if (elapsedSeconds >= 0) {
                    LoadManagerTotal.Instance.AddPlayModeTime(elapsedSeconds);
                    LoadManagerSession.Instance.AddPlayModeTime(elapsedSeconds);
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
        LoadManagerTotal.Instance.AddTime((float)sessionSeconds);
        if (sessionSeconds > LoadManagerTotal.Instance.LongestSession()) {
            LoadManagerTotal.Instance.SetLongestSession(sessionSeconds);
        }
        LoadManagerTotal.Instance.Opened(false);

        LoadManagerSession.Instance.Reset();

        SaveAll();
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
        LoadManagerTotal.Instance.IncrementScenesOpened();
        LoadManagerSession.Instance.IncrementScenesOpened();
        SaveAll();
    }

    static void OnUndoOrRedo(in UndoRedoInfo undo) {
        if (undo.isRedo) {
            LoadManagerTotal.Instance.IncrementRedo();
            LoadManagerSession.Instance.IncrementRedo();
        }
        else {
            LoadManagerTotal.Instance.IncrementUndo();
            LoadManagerSession.Instance.IncrementUndo();
        }
    }

    static void OnLogReceived(string condition, string stackTrace, LogType type) {
        LoadManagerTotal.Instance.IncrementLog(type);
        LoadManagerSession.Instance.IncrementLog(type);
    }

    #endregion


    #region Helpers

    static void CountProjectSession() {
        if (LoadManagerTotal.Instance.GetOpened()) { return; }

        LoadManagerTotal.Instance.IncrementOpened();
        LoadManagerTotal.Instance.Opened(true);
    }

    static void SaveAll() {
        LoadManagerTotal.Instance.Save();
        LoadManagerSession.Instance.Save();
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
