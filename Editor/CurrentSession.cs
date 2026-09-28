using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentSession : VisualElement
{
    public static CurrentSession Instance;

    // Action Stats
    int _timesProjectCompiled = 0;      // How Many Times Project Script Have Been Compiled
    int _timesPlayModePressed = 0;      // How Many Times Playmode Has Been Entered
    int _totalScenesOpenedAmount = 0;   // How Many Times Scenes Have Been Opened

    // Edit Stats
    int _totalRedoAmount = 0;   // How Many Times Redo Action Has Been Done
    int _totalUndoAmount = 0;   // How Many Times Undo Action Has Been Done

    // Log Stats
    int _normalLogAmount = 0;   // How Many Normal Logs Have Been Logged
    int _warningLogAmount = 0;  // How Many Warning Logs Have Been Logged
    int _errorLogAmount = 0;    // How Many Error Logs Have Been Logged
    int _assertLogAmount = 0;   // How Many Assert Logs Have Been Logged
    int _exceptionLogAmount = 0;// How Many Exception Logs Have Been Logged
    
    // Timed Stats
    float _currentSessionLength = 0f;   // Time Which The Current Editor Instance Has Been On
    double _totalDomainReloadTime = 0f; // Total Time Which Domain Reload Has Taken
    double _totalPlayModeTime = 0f;     // Total Time Which Has Been Spent In Playmode

    // Labels
    Label _sessionLengthLabel;
    Label _sessionSceneOpenedCountLabel;
    Label _sessionPlayModeCountLabel;
    Label _sessionCompiledCountLabel;
    Label _sessionRedoCountLabel;
    Label _sessionUndoCountLabel;
    Label _sessionNormalLogCountLabel;
    Label _sessionErrorLogCountLabel;
    Label _sessionWarningLogCountLabel;
    Label _sessionAssertLogCountLabel;
    Label _sessionExceptionLogCountLabel;
    Label _sessionDomainReloadTimeLabel;
    Label _sessionPlayModeTimeLabel;

    public CurrentSession()
    {
        Instance = this;
        GetTimes();

        AddToClassList("stats-root");
        var styleSheet = StylesheetLocator.Load();
        if (styleSheet != null) {
            styleSheets.Add(styleSheet);
        }

        Add(SectionTitle("Timers"));
        var timerRow = StatRow();
        timerRow.Add(StatCard(ref _sessionLengthLabel, "Time Spent\n(Session)",        FormatTime(_currentSessionLength)));
        timerRow.Add(StatCard(ref _sessionDomainReloadTimeLabel, "Domain Reload\n(Session)",  FormatTime(_totalDomainReloadTime)));
        timerRow.Add(StatCard(ref _sessionPlayModeTimeLabel, "Play Time\n(Session)",      FormatTime(_totalPlayModeTime)));
        Add(timerRow);

        Add(SectionTitle("Actions"));
        var actionRow = StatRow();
        actionRow.Add(StatCard(ref _sessionCompiledCountLabel, "Times Compiled\n(Session)",       _timesProjectCompiled.ToString()));
        actionRow.Add(StatCard(ref _sessionPlayModeCountLabel, "Play Mode Entered\n(Session)",    _timesPlayModePressed.ToString()));
        actionRow.Add(StatCard(ref _sessionSceneOpenedCountLabel, "Scenes Opened\n(Session)",        _totalScenesOpenedAmount.ToString()));
        Add(actionRow);

        Add(SectionTitle("Edits"));
        var editRow = StatRow();
        editRow.Add(StatCard(ref _sessionRedoCountLabel, "Times Redo\n(Session)", _totalRedoAmount.ToString()));
        editRow.Add(StatCard(ref _sessionUndoCountLabel, "Times Undo\n(Session)", _totalUndoAmount.ToString()));
        Add(editRow);

        Add(SectionTitle("Logs"));
        var logRow = StatRow();
        logRow.Add(StatCard(ref _sessionNormalLogCountLabel, "Normal Log Count\n(Session)",     _normalLogAmount.ToString()));
        logRow.Add(StatCard(ref _sessionWarningLogCountLabel, "Warning Log Count\n(Session)",    _warningLogAmount.ToString()));
        logRow.Add(StatCard(ref _sessionErrorLogCountLabel, "Error Log Count\n(Session)",      _errorLogAmount.ToString()));
        logRow.Add(StatCard(ref _sessionExceptionLogCountLabel, "Exception Log Count\n(Session)",  _exceptionLogAmount.ToString()));
        logRow.Add(StatCard(ref _sessionAssertLogCountLabel, "Assert Log Count\n(Session)",     _assertLogAmount.ToString()));
        Add(logRow);

        EditorApplication.update += UpdateCurrentSessionTime;
        DailyLoadManager.Instance.CompiledAction += SetCompiled;
        DailyLoadManager.Instance.PlayModeAction += UpdatePlayModeEntered;
        DailyLoadManager.Instance.PlayModeTimeAction += AddPlayModeTime;
        DailyLoadManager.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        DailyLoadManager.Instance.SceneAction += UpdateScene;
        DailyLoadManager.Instance.RedoAction += UpdateRedo;
        DailyLoadManager.Instance.UndoAction += UpdateUndo;
        DailyLoadManager.Instance.LogAction += UpdateLog;

        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    #region Label Makers

    VisualElement StatRow() {
        var row = new VisualElement();
        row.AddToClassList("stat-row");
        return row;
    }

    Label SectionTitle(string txt) {
        var label = new Label($"[ {txt} ]");
        label.AddToClassList("section-title");
        return label;
    }

    VisualElement StatCard(ref Label valueLabel, string caption, string value) {
        var card = new VisualElement();
        card.AddToClassList("stat-card");

        valueLabel = new Label(value);
        valueLabel.AddToClassList("stat-value");
        card.Add(valueLabel);

        var captionLabel = new Label(caption);
        captionLabel.AddToClassList("stat-label");
        card.Add(captionLabel);

        return card;
    }

    #endregion

    #region Label updaters

    // Formats Time From Seconds Into Hours, Minutes & Seconds
    string FormatTime(double seconds) {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours}:{ts.Minutes}:{ts.Seconds} s";
    }

    // Updates Time Which Editor Has Been On
    void UpdateCurrentSessionTime()
    {
        _currentSessionLength = (float)(EditorApplication.timeSinceStartup);
        _sessionLengthLabel.text = FormatTime(_currentSessionLength);
    }

    // Updates Time Domain Reload Has Taken
    private void AddDomainReloadTime(double obj)
    {
        _totalDomainReloadTime = obj;
        _sessionDomainReloadTimeLabel.text = FormatTime(_totalDomainReloadTime);
    }

    // Updates Time Spent In Playmode
    private void AddPlayModeTime(double obj)
    {
        _sessionPlayModeTimeLabel.text = FormatTime(obj);
    }

    // Updates Times Playmode Has Been Entered
    private void UpdatePlayModeEntered(int obj)
    {
        _sessionPlayModeCountLabel.text = obj.ToString();
    }

    // Updates Times The Redo Action Has Been Done
    private void UpdateRedo(int obj)
    {
        _sessionRedoCountLabel.text = obj.ToString();
    }

    // Updates Times The Undo Action Has Been Done
    private void UpdateUndo(int arg2)
    {
        _sessionUndoCountLabel.text = arg2.ToString();
    }

    // Updates Times Each Log Type Has Been "Logged" 
    private void UpdateLog(LogType type, int arg2)
    {
        switch (type)
        {
            case LogType.Error:
                _sessionErrorLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Assert:
                _sessionAssertLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Warning:
                _sessionWarningLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Log:
                _sessionNormalLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Exception:
                _sessionExceptionLogCountLabel.text = arg2.ToString();
                break;
        }
    }

    // Updates Times Scenes Has Been Opened
    private void UpdateScene(int count)
    {
        _sessionSceneOpenedCountLabel.text = count.ToString();
    }

    // Updates Times Project Has Compiled
    public void SetCompiled(int count)
    {
        _sessionCompiledCountLabel.text = count.ToString();
    }

    #endregion

    void GetTimes()
    {
        _timesProjectCompiled = DailyLoadManager.Instance.Compiled();
        _timesPlayModePressed = DailyLoadManager.Instance.PlayPressed();
        _totalRedoAmount = DailyLoadManager.Instance.Redo();
        _totalUndoAmount = DailyLoadManager.Instance.Undo();
        _totalDomainReloadTime = DailyLoadManager.Instance.TotalDomainReloadTime();
        _totalPlayModeTime = DailyLoadManager.Instance.TotalPlayModeTime();
        _totalScenesOpenedAmount = DailyLoadManager.Instance.ScenesOpened();
        _normalLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Log);
        _warningLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Warning);
        _errorLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Error);
        _exceptionLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Exception);
        _assertLogAmount = DailyLoadManager.Instance.GetLogType(LogType.Assert);
        _currentSessionLength = (float)(EditorApplication.timeSinceStartup);
    }

    void OnDetachFromPanel(DetachFromPanelEvent evt) {
        EditorApplication.update -= UpdateCurrentSessionTime;
        DailyLoadManager.Instance.CompiledAction -= SetCompiled;
        DailyLoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        DailyLoadManager.Instance.PlayModeTimeAction -= AddPlayModeTime;
        DailyLoadManager.Instance.DomainReloadTimeAction -= AddDomainReloadTime;
        DailyLoadManager.Instance.SceneAction -= UpdateScene;
        DailyLoadManager.Instance.RedoAction -= UpdateRedo;
        DailyLoadManager.Instance.UndoAction -= UpdateUndo;
        DailyLoadManager.Instance.LogAction -= UpdateLog;
        
        if (Instance == this) {
            Instance = null;
        }
    }
}
