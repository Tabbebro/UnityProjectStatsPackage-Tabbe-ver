using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StatisticsWindow : EditorWindow
{
    [MenuItem("Window/Total statistics")]
    public static void ShowWindow() {
        var window = GetWindow<StatisticsWindow>();
        window.titleContent = new GUIContent("Stats Monitor");
    }
    
    static readonly string[] tabLabels = {"Total Statistics","Current Session Statistics"};

    int _selectedIndex = -1;
    ScrollView _contentPane;
    readonly List<Button> _tabButtons = new();

    // Windows
    CurrentSession _currentSessionView;
    Total _totalView;

    void CreateGUI() {
        _currentSessionView = new CurrentSession();
        _totalView = new Total();

        var root = rootVisualElement;
        root.AddToClassList("stats-window");

        var styleSheet = StylesheetLocator.Load();
        if (styleSheet != null)
            root.styleSheets.Add(styleSheet);

        var tabBar = new VisualElement();
        tabBar.AddToClassList("tab-bar");
        root.Add(tabBar);

        for (int i = 0; i < tabLabels.Length; i++) {
            int tabIndex = i;
            var tab = new Button(() => SelectTab(tabIndex)) { text = tabLabels[i] };
            tab.AddToClassList("tab-button");
            tabBar.Add(tab);
            _tabButtons.Add(tab);
        }

        _contentPane = new ScrollView(ScrollViewMode.Vertical);
        _contentPane.AddToClassList("tab-content");
        root.Add(_contentPane);

        SelectTab(_selectedIndex);

    }

    void SelectTab(int index) {
        _selectedIndex = index;

        for (int i = 0; i < _tabButtons.Count; i++) {
            _tabButtons[i].EnableInClassList("tab-button--active", i == index);
        }

        _contentPane.Clear();
        _contentPane.Add(index == 0 ? (VisualElement)_totalView : _currentSessionView);
    }
}
