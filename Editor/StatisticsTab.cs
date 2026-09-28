using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class StatisticsTab : VisualElement {
    // Action Stats
    protected int _projectOpenedAmount = 0; // How Many Times Project Has Been Opened In Total
    protected int _compileAmount = 0;       // How Many Times Project Script Have Been Compiled
    protected int _playmodeEnterAmount = 0; // How Many Times Playmode Has Been Entered
    protected int _sceneOpenAmount = 0;     // How Many Times Scenes Have Been Opened
    protected int _crashAmount = 0;         // How Many Times Project Has Crashed In Total

    // Edit Stats
    protected int _redoAmount = 0;   // How Many Times Redo Action Has Been Done
    protected int _undoAmount = 0;   // How Many Times Undo Action Has Been Done

    // Log Stats
    protected int _normalLogAmount = 0;   // How Many Normal Logs Have Been Logged
    protected int _warningLogAmount = 0;  // How Many Warning Logs Have Been Logged
    protected int _errorLogAmount = 0;    // How Many Error Logs Have Been Logged
    protected int _assertLogAmount = 0;   // How Many Assert Logs Have Been Logged
    protected int _exceptionLogAmount = 0;// How Many Exception Logs Have Been Logged

    // Timed Stats
    protected double _timeSpent = 0f;           // Time Which The Current Editor Instance Has Been On
    protected double _domainReloadTime = 0f;    // Total Time Which Domain Reload Has Taken
    protected double _playmodeTime = 0f;        // Total Time Which Has Been Spent In Playmode
    protected double _longestSession = 0f;      // What Is The Time Of The Longes Session

    // Labels
    protected Label _projectOpenedAmountLabel;
    protected Label _compileAmountLabel;
    protected Label _playmodeEnterAmountLabel;
    protected Label _sceneOpenAmountLabel;
    protected Label _crashAmountLabel;

    protected Label _redoAmountLabel;
    protected Label _undoAmountLabel;

    protected Label _normalLogAmountLabel;
    protected Label _errorLogAmountLabel;
    protected Label _warningLogAmountLabel;
    protected Label _assertLogAmountLabel;
    protected Label _exceptionLogAmountLabel;

    protected Label _timeSpentLabel;
    protected Label _domainReloadTimeLabel;
    protected Label _playmodeTimeLabel;
    protected Label _longestSessionLabel;

    protected StatisticsTab() {
        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    #region Label Makers

    protected virtual VisualElement StatRow() {
        var row = new VisualElement();
        row.AddToClassList("stat-row");
        return row;
    }

    protected virtual Label SectionTitle(string txt) {
        var label = new Label(txt.ToUpper());
        label.AddToClassList("section-title");
        return label;
    }

    protected virtual VisualElement StatCard(ref Label valueLabel, string caption, string value) {
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
    protected virtual string FormatTime(double seconds) {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    // Updates Time Which Editor Has Been On
    protected virtual void UpdateTime() {
        _timeSpent = EditorApplication.timeSinceStartup;
        _timeSpentLabel.text = FormatTime(_timeSpent);
    }

    // Updates Time Domain Reload Has Taken
    protected virtual void AddDomainReloadTime(double obj) {
        _domainReloadTime = obj;
        _domainReloadTimeLabel.text = FormatTime(_domainReloadTime);
    }

    // Updates Time Spent In Playmode
    protected virtual void AddPlayModeTime(double obj) {
        _playmodeTimeLabel.text = FormatTime(obj);
    }

    // Updates Times Playmode Has Been Entered
    protected virtual void UpdatePlayModeEntered(int obj) {
        _playmodeEnterAmountLabel.text = obj.ToString();
    }

    // Updates Times The Redo Action Has Been Done
    protected virtual void UpdateRedo(int obj) {
        _redoAmountLabel.text = obj.ToString();
    }

    // Updates Times The Undo Action Has Been Done
    protected virtual void UpdateUndo(int arg2) {
        _undoAmountLabel.text = arg2.ToString();
    }

    // Updates Times Each Log Type Has Been "Logged" 
    protected virtual void UpdateLog(LogType type, int arg2) {
        switch (type) {
            case LogType.Error:
                _errorLogAmountLabel.text = arg2.ToString();
                break;
            case LogType.Assert:
                _assertLogAmountLabel.text = arg2.ToString();
                break;
            case LogType.Warning:
                _warningLogAmountLabel.text = arg2.ToString();
                break;
            case LogType.Log:
                _normalLogAmountLabel.text = arg2.ToString();
                break;
            case LogType.Exception:
                _exceptionLogAmountLabel.text = arg2.ToString();
                break;
        }
    }

    // Updates Times Scenes Has Been Opened
    protected virtual void UpdateScene(int count) {
        _sceneOpenAmountLabel.text = count.ToString();
    }

    // Updates Times Project Has Compiled
    protected virtual void SetCompiled(int count) {
        _compileAmountLabel.text = count.ToString();
    }

    #endregion

    #region Lifecycle

    // Reads The Current Values From The Data Source (LoadManager / DailyLoadManager)
    protected abstract void GetTimes();

    // Subscribes To The Data Source Events
    protected abstract void Subscribe();

    // Unsubscribes From The Data Source Events
    protected abstract void Unsubscribe();

    // Runs Every Time The Tab Is Added To A Panel (Also When Switching Back To It)
    void OnAttachToPanel(AttachToPanelEvent evt) {
        // Events Are Missed While The Tab Is Detached, So Re-read And Refresh First
        GetTimes();
        RefreshLabels();

        EditorApplication.update += UpdateTime;
        Subscribe();
    }

    // Runs Every Time The Tab Is Removed From A Panel (Also When Switching Away From It)
    void OnDetachFromPanel(DetachFromPanelEvent evt) {
        EditorApplication.update -= UpdateTime;
        Unsubscribe();
    }

    // Pushes The Current Field Values Into The Labels
    protected virtual void RefreshLabels() {
        SetText(_timeSpentLabel, FormatTime(_timeSpent));
        SetText(_domainReloadTimeLabel, FormatTime(_domainReloadTime));
        SetText(_playmodeTimeLabel, FormatTime(_playmodeTime));
        SetText(_longestSessionLabel, FormatTime(_longestSession));

        SetText(_projectOpenedAmountLabel, _projectOpenedAmount.ToString());
        SetText(_compileAmountLabel, _compileAmount.ToString());
        SetText(_playmodeEnterAmountLabel, _playmodeEnterAmount.ToString());
        SetText(_sceneOpenAmountLabel, _sceneOpenAmount.ToString());
        SetText(_crashAmountLabel, _crashAmount.ToString());

        SetText(_redoAmountLabel, _redoAmount.ToString());
        SetText(_undoAmountLabel, _undoAmount.ToString());

        SetText(_normalLogAmountLabel, _normalLogAmount.ToString());
        SetText(_warningLogAmountLabel, _warningLogAmount.ToString());
        SetText(_errorLogAmountLabel, _errorLogAmount.ToString());
        SetText(_assertLogAmountLabel, _assertLogAmount.ToString());
        SetText(_exceptionLogAmountLabel, _exceptionLogAmount.ToString());
    }

    // Null-safe, Since Each Tab Only Creates The Labels It Needs
    static void SetText(Label label, string text) {
        if (label != null) {
            label.text = text;
        }
    }

    #endregion
}