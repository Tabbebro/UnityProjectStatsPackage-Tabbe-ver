using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StatisticsTabTotal : StatisticsTab {
    public static StatisticsTabTotal Instance;

    double _savedTimeSpent = 0; // Time From Previous Sessions, Excluding The Current One

    public StatisticsTabTotal() {
        Instance = this;
        GetTimes();

        AddToClassList("stats-root");
        var styleSheet = StylesheetLocator.Load();
        if (styleSheet != null) {
            styleSheets.Add(styleSheet);
        }

        Add(SectionTitle("Timers"));
        var timerRow = StatRow();
        timerRow.Add(StatCard(ref _timeSpentLabel, "Time Spent\n(Total)", FormatTime(_timeSpent)));
        timerRow.Add(StatCard(ref _domainReloadTimeLabel, "Domain Reload\n(Total)", FormatTime(_domainReloadTime)));
        timerRow.Add(StatCard(ref _playmodeTimeLabel, "Play Time\n(Total)", FormatTime(_playmodeTime)));
        timerRow.Add(StatCard(ref _longestSessionLabel, "Longest Session\n(Total)", FormatTime(_longestSession)));
        Add(timerRow);

        Add(SectionTitle("Actions"));
        var actionRow = StatRow();
        actionRow.Add(StatCard(ref _compileAmountLabel, "Times Compiled\n(Total)", $"{_compileAmount}"));
        actionRow.Add(StatCard(ref _playmodeEnterAmountLabel, "Play Mode Entered\n(Total)", $"{_playmodeEnterAmount}"));
        actionRow.Add(StatCard(ref _sceneOpenAmountLabel, "Scenes Opened\n(Total)", $"{_sceneOpenAmount}"));
        actionRow.Add(StatCard(ref _projectOpenedAmountLabel, "Project Sessions\n(Total)", $"{_projectOpenedAmount}"));
        Add(actionRow);

        Add(SectionTitle("Edits"));
        var editRow = StatRow();
        editRow.Add(StatCard(ref _redoAmountLabel, "Times Redo\n(Total)", $"{_redoAmount}"));
        editRow.Add(StatCard(ref _undoAmountLabel, "Times Undo\n(Total)", $"{_undoAmount}"));
        Add(editRow);

        Add(SectionTitle("Logs"));
        var logRow = StatRow();
        logRow.Add(StatCard(ref _normalLogAmountLabel, "Normal Log Count\n(Total)", $"{_normalLogAmount}"));
        logRow.Add(StatCard(ref _warningLogAmountLabel, "Warning Log Count\n(Total)", $"{_warningLogAmount}"));
        logRow.Add(StatCard(ref _errorLogAmountLabel, "Error Log Count\n(Total)", $"{_errorLogAmount}"));
        logRow.Add(StatCard(ref _exceptionLogAmountLabel, "Exception Log Count\n(Total)", $"{_exceptionLogAmount}"));
        logRow.Add(StatCard(ref _assertLogAmountLabel, "Assert Log Count\n(Total)", $"{_assertLogAmount}"));
        Add(logRow);
    }

    protected override void GetTimes() {
        _projectOpenedAmount = LoadManagerTotal.Instance.Opened();
        _compileAmount = LoadManagerTotal.Instance.Compiled();
        _playmodeEnterAmount = LoadManagerTotal.Instance.PlayPressed();
        _sceneOpenAmount = LoadManagerTotal.Instance.ScenesOpened();
        _crashAmount = LoadManagerTotal.Instance.Crashesh();

        _redoAmount = LoadManagerTotal.Instance.Redo();
        _undoAmount = LoadManagerTotal.Instance.Undo();

        _normalLogAmount = LoadManagerTotal.Instance.GetLogType(LogType.Log);
        _warningLogAmount = LoadManagerTotal.Instance.GetLogType(LogType.Warning);
        _errorLogAmount = LoadManagerTotal.Instance.GetLogType(LogType.Error);
        _exceptionLogAmount = LoadManagerTotal.Instance.GetLogType(LogType.Exception);
        _assertLogAmount = LoadManagerTotal.Instance.GetLogType(LogType.Assert);

        _savedTimeSpent = LoadManagerTotal.Instance.TotalTime();
        _timeSpent = _savedTimeSpent;
        _domainReloadTime = LoadManagerTotal.Instance.TotalDomainReloadTime();
        _playmodeTime = LoadManagerTotal.Instance.TotalPlayModeTime();
        _longestSession = LoadManagerTotal.Instance.LongestSession();

        //_currentSessionLength = (float)(EditorApplication.timeSinceStartup);
    }

    // Total Time Is The Saved Time Plus The Current Session, Not Just The Session
    protected override void UpdateTime() {
        _timeSpent = _savedTimeSpent + EditorApplication.timeSinceStartup;
        _timeSpentLabel.text = FormatTime(_timeSpent);
    }

    protected override void Subscribe() {
        Instance = this; // Restores Instance When The Tab Is Attached Again

        LoadManagerTotal.Instance.CompiledAction += SetCompiled;
        LoadManagerTotal.Instance.PlayModeAction += UpdatePlayModeEntered;
        LoadManagerTotal.Instance.SceneAction += UpdateScene;

        LoadManagerTotal.Instance.RedoAction += UpdateRedo;
        LoadManagerTotal.Instance.UndoAction += UpdateUndo;

        LoadManagerTotal.Instance.LogAction += UpdateLog;

        LoadManagerTotal.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        LoadManagerTotal.Instance.PlayModeTimeAction += AddPlayModeTime;
    }

    protected override void Unsubscribe() {
        LoadManagerTotal.Instance.CompiledAction -= SetCompiled;
        LoadManagerTotal.Instance.PlayModeAction -= UpdatePlayModeEntered;
        LoadManagerTotal.Instance.SceneAction -= UpdateScene;

        LoadManagerTotal.Instance.RedoAction -= UpdateRedo;
        LoadManagerTotal.Instance.UndoAction -= UpdateUndo;

        LoadManagerTotal.Instance.LogAction -= UpdateLog;

        LoadManagerTotal.Instance.DomainReloadTimeAction -= AddDomainReloadTime;
        LoadManagerTotal.Instance.PlayModeTimeAction -= AddPlayModeTime;

        if (Instance == this) {
            Instance = null;
        }
    }
}