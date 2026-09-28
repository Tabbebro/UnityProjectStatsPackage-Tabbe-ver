using UnityEditor;
using UnityEngine;

public class StatisticsTabSession : StatisticsTab {
    public static StatisticsTabSession Instance;

    public StatisticsTabSession() {
        Instance = this;
        GetTimes();

        AddToClassList("stats-root");
        var styleSheet = StylesheetLocator.Load();
        if (styleSheet != null) {
            styleSheets.Add(styleSheet);
        }

        Add(SectionTitle("Timers"));
        var timerRow = StatRow();
        timerRow.Add(StatCard(ref _timeSpentLabel, "Time Spent\n(Session)", FormatTime(_timeSpent)));
        timerRow.Add(StatCard(ref _domainReloadTimeLabel, "Domain Reload\n(Session)", FormatTime(_domainReloadTime)));
        timerRow.Add(StatCard(ref _playmodeTimeLabel, "Play Time\n(Session)", FormatTime(_playmodeTime)));
        Add(timerRow);

        Add(SectionTitle("Actions"));
        var actionRow = StatRow();
        actionRow.Add(StatCard(ref _compileAmountLabel, "Times Compiled\n(Session)", _compileAmount.ToString()));
        actionRow.Add(StatCard(ref _playmodeEnterAmountLabel, "Play Mode Entered\n(Session)", _playmodeEnterAmount.ToString()));
        actionRow.Add(StatCard(ref _sceneOpenAmountLabel, "Scenes Opened\n(Session)", _sceneOpenAmount.ToString()));
        Add(actionRow);

        Add(SectionTitle("Edits"));
        var editRow = StatRow();
        editRow.Add(StatCard(ref _redoAmountLabel, "Times Redo\n(Session)", _redoAmount.ToString()));
        editRow.Add(StatCard(ref _undoAmountLabel, "Times Undo\n(Session)", _undoAmount.ToString()));
        Add(editRow);

        Add(SectionTitle("Logs"));
        var logRow = StatRow();
        logRow.Add(StatCard(ref _normalLogAmountLabel, "Normal Log Count\n(Session)", _normalLogAmount.ToString()));
        logRow.Add(StatCard(ref _warningLogAmountLabel, "Warning Log Count\n(Session)", _warningLogAmount.ToString()));
        logRow.Add(StatCard(ref _errorLogAmountLabel, "Error Log Count\n(Session)", _errorLogAmount.ToString()));
        logRow.Add(StatCard(ref _exceptionLogAmountLabel, "Exception Log Count\n(Session)", _exceptionLogAmount.ToString()));
        logRow.Add(StatCard(ref _assertLogAmountLabel, "Assert Log Count\n(Session)", _assertLogAmount.ToString()));
        Add(logRow);
    }

    protected override void GetTimes() {
        _compileAmount = LoadManagerSession.Instance.Compiled();
        _playmodeEnterAmount = LoadManagerSession.Instance.PlayPressed();
        _redoAmount = LoadManagerSession.Instance.Redo();
        _undoAmount = LoadManagerSession.Instance.Undo();
        _domainReloadTime = LoadManagerSession.Instance.TotalDomainReloadTime();
        _playmodeTime = LoadManagerSession.Instance.TotalPlayModeTime();
        _sceneOpenAmount = LoadManagerSession.Instance.ScenesOpened();
        _normalLogAmount = LoadManagerSession.Instance.GetLogType(LogType.Log);
        _warningLogAmount = LoadManagerSession.Instance.GetLogType(LogType.Warning);
        _errorLogAmount = LoadManagerSession.Instance.GetLogType(LogType.Error);
        _exceptionLogAmount = LoadManagerSession.Instance.GetLogType(LogType.Exception);
        _assertLogAmount = LoadManagerSession.Instance.GetLogType(LogType.Assert);
        _timeSpent = EditorApplication.timeSinceStartup;
    }

    protected override void Subscribe() {
        Instance = this; // Restores Instance When The Tab Is Attached Again

        LoadManagerSession.Instance.CompiledAction += SetCompiled;
        LoadManagerSession.Instance.PlayModeAction += UpdatePlayModeEntered;
        LoadManagerSession.Instance.SceneAction += UpdateScene;

        LoadManagerSession.Instance.RedoAction += UpdateRedo;
        LoadManagerSession.Instance.UndoAction += UpdateUndo;

        LoadManagerSession.Instance.LogAction += UpdateLog;

        LoadManagerSession.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        LoadManagerSession.Instance.PlayModeTimeAction += AddPlayModeTime;
    }

    protected override void Unsubscribe() {
        LoadManagerSession.Instance.CompiledAction -= SetCompiled;
        LoadManagerSession.Instance.PlayModeAction -= UpdatePlayModeEntered;
        LoadManagerSession.Instance.SceneAction -= UpdateScene;

        LoadManagerSession.Instance.RedoAction -= UpdateRedo;
        LoadManagerSession.Instance.UndoAction -= UpdateUndo;

        LoadManagerSession.Instance.LogAction -= UpdateLog;

        LoadManagerSession.Instance.DomainReloadTimeAction -= AddDomainReloadTime;
        LoadManagerSession.Instance.PlayModeTimeAction -= AddPlayModeTime;

        if (Instance == this) {
            Instance = null;
        }
    }
}