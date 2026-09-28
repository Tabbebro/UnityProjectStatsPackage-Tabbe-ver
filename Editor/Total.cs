using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Total : VisualElement
{
    public static Total Instance;

    // Action Stats
    int _timesProjectOpened = 0;        // How Many Times Project Has Been Opened In Total
    int _timesProjectCompiled = 0;      // How Many Times Project Has Been Compiled In Total
    int _timesPlayModePressed = 0;      // How Many Times Playmode Has Been Entered In Total
    int _totalScenesOpenedAmount = 0;   // How Many Times A Scene Has Been Opened In Total
    int _crashAmount = 0;               // How Many Times Project Has Crashed In Total

    // Edit Stats
    int _totalRedoAmount = 0;   // How Many Times The Redo Action Has Been Used In Total
    int _totalUndoAmount = 0;   // How Many Times The Undo Action Has Been Used In Total

    // Log Stats
    int _normalLogAmount = 0;   // How Many Normal Logs Have Been Logged In Total
    int _warningLogAmount = 0;  // How Many Warning Logs Have Been Logged In Total
    int _errorLogAmount = 0;    // How Many Error Logs Have Been Logged In Total
    int _assertLogAmount = 0;   // How Many Assert Logs Have Been Logged In Total
    int _exceptionLogAmount = 0;// How Many Exception Logs Have Been Logged In Total

    // Timer Stats
    float _totalTimeSpent = 0f;  // How Much Time The Project Has Been On In Total
    float _currentSessionLength = 0f; // How Much Time Current Session Has Been On
    double _totalDomainReloadTime = 0f; // How Much Time Has Been Spent On Domain Reload In Total
    double _totalPlayModeTime = 0f; // How Much Time Has Been Spent In Playmode In Total
    double _longestSession = 0f; // What Is The Time Of The Longes Session

    // Labels
    Label _totalSceneOpenedCountLabel;
    Label _totalTimeSpentLabel;
    Label _totalPlayModeCountLabel;
    Label _totalCompiledCountLabel;
    Label _totalRedoCountLabel;
    Label _totalUndoCountLabel;
    Label _totalNormalLogCountLabel;
    Label _totalErrorLogCountLabel;
    Label _totalWarningLogCountLabel;
    Label _totalAssertLogCountLabel;
    Label _totalExceptionLogCountLabel;
    Label _totalDomainReloadTimeLabel;
    Label _totalPlayModeTimeLabel;
    Label _totalProjectOpenedLabel;
    Label _totalCrashAmountLabel;
    Label _totalLongestSessionLabel;

    public Total()
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
        timerRow.Add(StatCard(ref _totalTimeSpentLabel, "Time Spent\n(Total)",   FormatTime(_totalTimeSpent)));
        timerRow.Add(StatCard(ref _totalDomainReloadTimeLabel, "Domain Reload\n(Total)",  FormatTime(_totalDomainReloadTime)));
        timerRow.Add(StatCard(ref _totalPlayModeTimeLabel, "Play Time\n(Total)",      FormatTime(_totalPlayModeTime)));
        timerRow.Add(StatCard(ref _totalLongestSessionLabel, "Longest Session\n(Total)",      FormatTime(_longestSession)));
        Add(timerRow);

        Add(SectionTitle("Actions"));
        var actionRow = StatRow();
        actionRow.Add(StatCard(ref _totalCompiledCountLabel, "Times Compiled\n(Total)",               $"{_timesProjectCompiled}"));
        actionRow.Add(StatCard(ref _totalPlayModeCountLabel, "Play Mode Entered\n(Total)",    $"{_timesPlayModePressed}"));
        actionRow.Add(StatCard(ref _totalSceneOpenedCountLabel, "Scenes Opened\n(Total)",        $"{_totalScenesOpenedAmount}"));
        actionRow.Add(StatCard(ref _totalProjectOpenedLabel, "Project Sessions\n(Total)",     $"{_timesProjectOpened}"));
        Add(actionRow);

        Add(SectionTitle("Edits"));
        var editRow = StatRow();
        editRow.Add(StatCard(ref _totalRedoCountLabel, "Times Redo\n(Total)", $"{_totalRedoAmount}"));
        editRow.Add(StatCard(ref _totalUndoCountLabel, "Times Undo\n(Total)", $"{_totalUndoAmount}"));
        Add(editRow);

        Add(SectionTitle("Logs"));
        var logRow = StatRow();
        logRow.Add(StatCard(ref _totalNormalLogCountLabel, "Normal Log Count\n(Total)",     $"{_normalLogAmount}"));
        logRow.Add(StatCard(ref _totalWarningLogCountLabel, "Warning Log Count\n(Total)",    $"{_warningLogAmount}"));
        logRow.Add(StatCard(ref _totalErrorLogCountLabel, "Error Log Count\n(Total)",      $"{_errorLogAmount}"));
        logRow.Add(StatCard(ref _totalExceptionLogCountLabel, "Exception Log Count\n(Total)",  $"{_exceptionLogAmount}"));
        logRow.Add(StatCard(ref _totalAssertLogCountLabel, "Assert Log Count\n(Total)",     $"{_assertLogAmount}"));
        Add(logRow);

        EditorApplication.update += UpdateTime;
        LoadManager.Instance.CompiledAction += SetCompiled;
        LoadManager.Instance.PlayModeAction += UpdatePlayModeEntered;
        LoadManager.Instance.PlayModeTimeAction += AddPlayModeTime;
        LoadManager.Instance.DomainReloadTimeAction += AddDomainReloadTime;
        LoadManager.Instance.SceneAction += UpdateScene;
        LoadManager.Instance.RedoAction += UpdateRedo;
        LoadManager.Instance.UndoAction += UpdateUndo;
        LoadManager.Instance.LogAction += UpdateLog;

        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    #region Label Makers
    VisualElement StatRow() {
        var row = new VisualElement();
        row.AddToClassList("stat-row");
        return row;
    }

    Label SectionTitle(string txt) {
        var label = new Label(txt.ToUpper());
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

    #region Label Updaters

    // Formats Time From Seconds Into Hours, Minutes & Seconds
    string FormatTime(double seconds) {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    // Updates Domain Reload Time To The Total Amount
    private void AddDomainReloadTime(double obj)
    {
        _totalDomainReloadTime = obj;
        _totalDomainReloadTimeLabel.text = FormatTime(_totalDomainReloadTime);
    }

    // Updates PlayMode Time To The Total Amount
    private void AddPlayModeTime(double obj)
    {
        _totalPlayModeTimeLabel.text = FormatTime(obj);
    }

    // Updates Total Amount Playmode Has Been Entered
    private void UpdatePlayModeEntered(int obj)
    {
        _totalPlayModeCountLabel.text = obj.ToString();
    }

    // Updates Amount Of Redo Actions Done In Total
    private void UpdateRedo(int obj)
    {
        _totalRedoCountLabel.text = obj.ToString();
    }

    // Updates Amount Of Undo Actions Done In Total 
    private void UpdateUndo(int arg2)
    {
        _totalUndoCountLabel.text = arg2.ToString();
    }

    // Updates Amount Of Logs "Logged"
    private void UpdateLog(LogType type, int arg2)
    {
        switch (type)
        {
            case LogType.Error:
                _totalErrorLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Assert:
                _totalAssertLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Warning:
                _totalWarningLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Log:
                _totalNormalLogCountLabel.text = arg2.ToString();
                break;
            case LogType.Exception:
                _totalExceptionLogCountLabel.text = arg2.ToString();
                break;
            default:
                break;
        }
    }

    // Updates Amount Of Scenes Opened In Total
    private void UpdateScene(int count)
    {
        _totalSceneOpenedCountLabel.text = count.ToString();
    }

    // Updates Amount Of Compilations In Total
    public void SetCompiled(int count)
    {
        _totalCompiledCountLabel.text = count.ToString();
    }

    // Updates Current Session Length
    void UpdateTime()
    {
        _currentSessionLength = (float)(EditorApplication.timeSinceStartup);
        _totalTimeSpentLabel.text = FormatTime(_totalTimeSpent);
    }

    #endregion

    void GetTimes()
    {
        _totalTimeSpent = LoadManager.Instance.TotalTime();
        _timesProjectCompiled = LoadManager.Instance.Compiled();
        _timesPlayModePressed = LoadManager.Instance.PlayPressed();
        _totalRedoAmount = LoadManager.Instance.Redo();
        _totalUndoAmount = LoadManager.Instance.Undo();
        _totalDomainReloadTime = LoadManager.Instance.TotalDomainReloadTime();
        _longestSession = LoadManager.Instance.LongestSession();
        _crashAmount = LoadManager.Instance.Crashesh();
        _timesProjectOpened = LoadManager.Instance.Opened();
        _totalPlayModeTime = LoadManager.Instance.TotalPlayModeTime();
        _totalScenesOpenedAmount = LoadManager.Instance.ScenesOpened();
        _normalLogAmount = LoadManager.Instance.GetLogType(LogType.Log);
        _warningLogAmount = LoadManager.Instance.GetLogType(LogType.Warning);
        _errorLogAmount = LoadManager.Instance.GetLogType(LogType.Error);
        _exceptionLogAmount = LoadManager.Instance.GetLogType(LogType.Exception);
        _assertLogAmount = LoadManager.Instance.GetLogType(LogType.Assert);
        _currentSessionLength = (float)(EditorApplication.timeSinceStartup);
    }

    void OnDetachFromPanel(DetachFromPanelEvent evt) {

        EditorApplication.update -= UpdateTime;
        LoadManager.Instance.CompiledAction -= SetCompiled;
        LoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        LoadManager.Instance.SceneAction -= UpdateScene;
        LoadManager.Instance.UndoAction -= UpdateRedo;
        LoadManager.Instance.RedoAction -= UpdateUndo;
        LoadManager.Instance.LogAction = UpdateLog;
        LoadManager.Instance.PlayModeAction -= UpdatePlayModeEntered;
        
        if (Instance == this) {
            Instance = null;
        }
    }
}
