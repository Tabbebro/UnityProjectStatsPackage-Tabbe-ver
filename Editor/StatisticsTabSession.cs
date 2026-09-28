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
        _compileAmount = DailyLoadManager.Instance.Compiled();
        _playmodeEnterAmount = DailyLoadManager.Instance.PlayPressed();
        _redoAmount = DailyLoadManager.Instance.Redo();
        _undoAmount = DailyLoadManager.Instance.Undo();
        _domainReloadTime = DailyLoadManager.Instance.TotalDomainReloadTime();
        _playmodeTime = DailyLoadManager.Instance.TotalPlayModeTime();
        _sceneOpenAmount = DailyLoadManager.Instance.ScenesOpened();
        _normalLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Log);
        _warningLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Warning);
        _errorLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Error);
        _exceptionLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Exception);
        _assertLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Assert);
        _timeSpent = EditorApplication.timeSinceStartup;
    }

    protected override void Subscribe() {
        Instance = this; // Restores Instance When The Tab Is Attached Again

        DailyLoadManager.Instance.CompiledAction += SetCompiled;
        DailyLoadManager.Instance.PlayModeAction += UpdatePlayModeEntered;
        DailyLoadManager.Instance.SceneAction += UpdateScene;

        DailyLoadManager.Instance.RedoAction += UpdateRedo;
        DailyLoadManager.Instance.UndoAction += UpdateUndo;

        DailyLoadManager.Instance.LogAction += UpdateLog;

        DailyLoadManager.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        DailyLoadManager.Instance.PlayModeTimeAction += AddPlayModeTime;
    }

    protected override void Unsubscribe() {
        DailyLoadManager.Instance.CompiledAction -= SetCompiled;
        DailyLoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        DailyLoadManager.Instance.SceneAction -= UpdateScene;

        DailyLoadManager.Instance.RedoAction -= UpdateRedo;
        DailyLoadManager.Instance.UndoAction -= UpdateUndo;

        DailyLoadManager.Instance.LogAction -= UpdateLog;

        DailyLoadManager.Instance.DomainReloadTimeAction -= AddDomainReloadTime;
        DailyLoadManager.Instance.PlayModeTimeAction -= AddPlayModeTime;

        if (Instance == this) {
            Instance = null;
        }
    }
}