using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Shared Fields, Saving/Loading Helpers And Increment Logic For LoadManager And DailyLoadManager Below
public class LoadManagerBase {
    public Action<int> CompiledAction;
    public Action<int> PlayModeAction;
    public Action<int> SceneAction;
    public Action<int> RedoAction;
    public Action<int> UndoAction;
    public Action<double> PlayModeTimeAction;
    public Action<double> DomainReloadTimeAction;
    public Action<LogType, int> LogAction;

    protected double _totalDomainReloadTime;
    protected double _totalTimeSpentInPlayMode;
    protected int _timesCompiled;
    protected int _timesPlayModePressed;
    protected int _totalSceneOpenedAmount;
    protected int _totalRedoAmount;
    protected int _totalUndoAmount;
    protected int _normalLogAmount;
    protected int _warningLogAmount;
    protected int _errorLogAmount;
    protected int _assertLogAmount;
    protected int _exceptionLogAmount;
    protected float _totalTimeSpent;
    protected string _date = "";

    readonly string _saveFilePath;

    protected LoadManagerBase(string saveFileName) {
        _saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    #region Saving And Loading Helpers

    // Reads The Save File Into Json, Or Returns False If It Doesn't Exist Yet
    protected bool TryReadFile(out string json) {
        json = null;
        if (!File.Exists(_saveFilePath))
            return false;

        json = File.ReadAllText(_saveFilePath);
        return true;
    }

    protected void WriteFile(string json) => File.WriteAllText(_saveFilePath, json);

    // Copies The Fields Common To Both Managers From A Loaded Snapshot
    protected void ApplyCommon(StatsSnapshot snapshot) {
        _date = snapshot.Date;
        _timesCompiled = snapshot.TimesCompiled;
        _timesPlayModePressed = snapshot.TimesPlayModePressed;
        _totalTimeSpent = snapshot.TotalTimeProjectOpen;
        _totalDomainReloadTime = snapshot.TotalDomainReloadTime;
        _totalTimeSpentInPlayMode = snapshot.TotalPlayModeTime;
        _totalSceneOpenedAmount = snapshot.TotalSceneOpenedAmount;
        _totalRedoAmount = snapshot.TotalRedoAmount;
        _totalUndoAmount = snapshot.TotalUndoAmount;
        _normalLogAmount = snapshot.NormalLogAmount;
        _warningLogAmount = snapshot.WarningLogAmount;
        _errorLogAmount = snapshot.ErrorLogAmount;
        _assertLogAmount = snapshot.AssertLogAmount;
        _exceptionLogAmount = snapshot.ExceptionLogAmount;
    }

    // Copies The Fields Common To Both Managers Into A Snapshot That Is About To Be Saved
    protected void FillCommon(StatsSnapshot snapshot) {
        snapshot.Date = _date;
        snapshot.TimesCompiled = _timesCompiled;
        snapshot.TimesPlayModePressed = _timesPlayModePressed;
        snapshot.TotalTimeProjectOpen = _totalTimeSpent;
        snapshot.TotalDomainReloadTime = _totalDomainReloadTime;
        snapshot.TotalPlayModeTime = _totalTimeSpentInPlayMode;
        snapshot.TotalSceneOpenedAmount = _totalSceneOpenedAmount;
        snapshot.TotalRedoAmount = _totalRedoAmount;
        snapshot.TotalUndoAmount = _totalUndoAmount;
        snapshot.NormalLogAmount = _normalLogAmount;
        snapshot.WarningLogAmount = _warningLogAmount;
        snapshot.ErrorLogAmount = _errorLogAmount;
        snapshot.AssertLogAmount = _assertLogAmount;
        snapshot.ExceptionLogAmount = _exceptionLogAmount;
    }

    #endregion

    #region Setters / Incrementers

    public void IncrementCompiled() {
        _timesCompiled++;
        CompiledAction?.Invoke(_timesCompiled);
    }
    public void IncrementPlayPressed() {
        _timesPlayModePressed++;
        PlayModeAction?.Invoke(_timesPlayModePressed);
    }
    public void IncrementUndo() {
        _totalUndoAmount++;
        UndoAction?.Invoke(_totalUndoAmount);
    }
    public void IncrementRedo() {
        _totalRedoAmount++;
        RedoAction?.Invoke(_totalRedoAmount);
    }
    public void IncrementLog(LogType type) {
        int incremented = 0;
        switch (type) {
            case LogType.Error:
                incremented = ++_errorLogAmount;
                break;
            case LogType.Assert:
                incremented = ++_assertLogAmount;
                break;
            case LogType.Warning:
                incremented = ++_warningLogAmount;
                break;
            case LogType.Log:
                incremented = ++_normalLogAmount;
                break;
            case LogType.Exception:
                incremented = ++_exceptionLogAmount;
                break;
        }
        LogAction?.Invoke(type, incremented);
    }
    public void IncrementScenesOpened() {
        _totalSceneOpenedAmount++;
        SceneAction?.Invoke(_totalSceneOpenedAmount);
    }
    public void AddTime(float seconds) => _totalTimeSpent += seconds;
    public void AddDomainReloadTime(double seconds) {
        _totalDomainReloadTime += seconds;
        DomainReloadTimeAction?.Invoke(_totalDomainReloadTime);
    }
    public void AddPlayModeTime(double seconds) {
        _totalTimeSpentInPlayMode += seconds;
        PlayModeTimeAction?.Invoke(_totalTimeSpentInPlayMode);
    }
    public void SetDate(string date) {
        if (_date != date)
            _date = date;
    }

    #endregion

    #region Getters

    public int ScenesOpened() => _totalSceneOpenedAmount;
    public int Compiled() => _timesCompiled;
    public int PlayPressed() => _timesPlayModePressed;
    public int Redo() => _totalRedoAmount;
    public int Undo() => _totalUndoAmount;
    public int GetLogType(LogType type) {
        switch (type) {
            case LogType.Error: return _errorLogAmount;
            case LogType.Assert: return _assertLogAmount;
            case LogType.Warning: return _warningLogAmount;
            case LogType.Log: return _normalLogAmount;
            case LogType.Exception: return _exceptionLogAmount;
            default: return 0;
        }
    }
    public string GetDate() => _date;
    public float TotalTime() => _totalTimeSpent;
    public double TotalDomainReloadTime() => _totalDomainReloadTime;
    public double TotalPlayModeTime() => _totalTimeSpentInPlayMode;

    #endregion
}

// Tracks Statistics For The Lifetime Of The Project
public class LoadManagerTotal : LoadManagerBase {
    static LoadManagerTotal _instance;
    public static LoadManagerTotal Instance => _instance ??= new LoadManagerTotal();

    public Action<int> ProjectOpenedAction;
    public Action<int> CrashAction;

    double _longestSession;
    int _timesProjectOpened;
    int _crashAmount;
    bool _openedIncremented;

    LoadManagerTotal() : base("spentTimeData.json") {
        Load();
    }

    void Load() {
        if (!TryReadFile(out string json))
            return;

        SpendDataList list = JsonUtility.FromJson<SpendDataList>(json);
        if (list == null || list.spendDatas.Count == 0)
            return;

        SpendData item = list.spendDatas[0];
        ApplyCommon(item);
        _openedIncremented = item.OpenedIncremented;
        _timesProjectOpened = item.TimesOpened;
        _crashAmount = item.CrashAmount;
        _longestSession = item.LongestSession;
    }

    public void Save() {
        SpendData snapshot = new SpendData();
        FillCommon(snapshot);
        snapshot.OpenedIncremented = _openedIncremented;
        snapshot.TimesOpened = _timesProjectOpened;
        snapshot.CrashAmount = _crashAmount;
        snapshot.LongestSession = _longestSession;

        SpendDataList list = new SpendDataList();
        list.spendDatas.Add(snapshot);
        WriteFile(JsonUtility.ToJson(list, true));
    }

    public void Opened(bool value) => _openedIncremented = value;
    public bool GetOpened() => _openedIncremented;
    public void IncrementOpened() {
        _timesProjectOpened++;
        ProjectOpenedAction?.Invoke(_timesProjectOpened);
    }
    public void IncrementCrashesh() => _crashAmount++;
    public void SetLongestSession(double seconds) => _longestSession = seconds;

    public int Opened() => _timesProjectOpened;
    public int Crashesh() => _crashAmount;
    public double LongestSession() => _longestSession;
}

// Tracks Statistics For The Current Editor Session. Reset On Quit
public class LoadManagerSession : LoadManagerBase {
    static LoadManagerSession _instance;
    public static LoadManagerSession Instance => _instance ??= new LoadManagerSession();

    LoadManagerSession() : base("currentSessionSpentTimeData.json") {
        Load();
    }

    void Load() {
        if (!TryReadFile(out string json))
            return;

        StatsSnapshotList list = JsonUtility.FromJson<StatsSnapshotList>(json);
        if (list == null || list.spendDatas.Count == 0)
            return;

        ApplyCommon(list.spendDatas[0]);
    }

    public void Save() {
        StatsSnapshot snapshot = new StatsSnapshot();
        FillCommon(snapshot);

        StatsSnapshotList list = new StatsSnapshotList();
        list.spendDatas.Add(snapshot);
        WriteFile(JsonUtility.ToJson(list, true));
    }

    // Starts The Next Editor Launch From Zero
    public void Reset() {
        _totalDomainReloadTime = 0;
        _totalTimeSpentInPlayMode = 0;
        _timesCompiled = 0;
        _timesPlayModePressed = 0;
        _totalSceneOpenedAmount = 0;
        _totalRedoAmount = 0;
        _totalUndoAmount = 0;
        _normalLogAmount = 0;
        _warningLogAmount = 0;
        _errorLogAmount = 0;
        _assertLogAmount = 0;
        _exceptionLogAmount = 0;
    }
}

#region Save Data

// Fields Common To Both The Total And The Session Save File
[System.Serializable]
public class StatsSnapshot {
    public string Date;
    public int TimesCompiled;
    public int TimesPlayModePressed;
    public int TotalSceneOpenedAmount;
    public int TotalRedoAmount;
    public int TotalUndoAmount;
    public int NormalLogAmount;
    public int WarningLogAmount;
    public int ErrorLogAmount;
    public int ExceptionLogAmount;
    public int AssertLogAmount;
    public float TotalTimeProjectOpen;
    public double TotalDomainReloadTime;
    public double TotalPlayModeTime;
}

// Adds The Fields That Only Make Sense For The Lifetime Total
[System.Serializable]
public class SpendData : StatsSnapshot {
    public bool OpenedIncremented;
    public int TimesOpened;
    public int CrashAmount;
    public double LongestSession;
}

[System.Serializable]
public class StatsSnapshotList {
    public List<StatsSnapshot> spendDatas = new List<StatsSnapshot>();
}

[System.Serializable]
public class SpendDataList {
    public List<SpendData> spendDatas = new List<SpendData>();
}

#endregion