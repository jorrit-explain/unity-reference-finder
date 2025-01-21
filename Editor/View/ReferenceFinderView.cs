using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ReferenceFinder.Enums;
using ReferenceFinder.Extensions;
using ReferenceFinder.Utils;

using Object = UnityEngine.Object;

namespace ReferenceFinder.Editor
{
    /// <summary>
    /// Reference finder editor responsible for displaying the interface
    /// and process basic information.
    /// </summary>
    public class ReferenceFinderView : EditorWindow
    {
        private ReferenceFinderPresenter _presenter;
        private StyleSheet _styleSheet;

        /// <summary>
        /// Static method called to show UI in the editor.
        /// </summary>
        [MenuItem("Tools/Reference Finder")]
        public static void ShowUI()
        {
            ReferenceFinderView window = GetWindow<ReferenceFinderView>();
            window.titleContent = new GUIContent("Reference Finder");
        }

        [MenuItem("Assets/Find References", true)]
        public static bool CanContextFindReferences()
        {
            var activeObject = Selection.activeObject;
            if (!activeObject)
                return false;

            // When right clicking in the project window without a specific asset under the mouse, we get a DefaultAsset...
            return activeObject.GetType() != typeof(DefaultAsset);
        }

        /// <summary>
        /// Static method which is available in the <em>Assets</em> menu as well as through
        /// right click in the project window.
        /// </summary>
        [MenuItem("Assets/Find References", false, 21)]
        public static void OnContextFindReferences()
        {
            // Show the UI
            ReferenceFinderView window = GetWindow<ReferenceFinderView>();
            window.titleContent = new GUIContent("Reference Finder");

            var presenter = window._presenter;
            var activeObject = Selection.activeObject;

            // Changing the search type switches the object field...
            switch (activeObject)
            {
                case GameObject _:
                    presenter.ChangeSearchType(SearchType.GameObject);
                    break;
                case ScriptableObject _:
                    presenter.ChangeSearchType(SearchType.ScriptableObject);
                    break;
                default:
                    presenter.ChangeSearchType(SearchType.Any);
                    break;
            }
            
            // Set the data in the object field
            window.rootVisualElement.Q<ObjectField>(className: "body-element").value = activeObject;

            presenter.FindReferences();
        }

        /// <summary>
        /// Initializes the view with a presenter and event
        /// subscription.
        /// </summary>
        private void InitializeView()
        {
            _presenter = new ReferenceFinderPresenter(this);
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        /// <summary>
        /// Unity callback for creating the GUI of the editor window.
        /// </summary>
        public void CreateGUI()
        {
            if (_presenter == null)
            {
                InitializeView();
            }
            _styleSheet = Resources.Load<StyleSheet>("ReferenceFinderStyleSheet");
            rootVisualElement.styleSheets.Add(_styleSheet);
            rootVisualElement.AddToClassList("root-element");
            _presenter.CreateGUI();
        }

        /// <summary>
        /// Unity callback for when the hierarchy changes.
        /// </summary>
        private void OnHierarchyChange()
        {
            _presenter.OnHierarchyChange();
        }

        /// <summary>
        /// Unity callback for when the window is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            // This can happen when enabling and disabling maximise on a window
            if (_presenter != null)
            {
                _presenter.Reset();
            }
        }

        /// <summary>
        /// Creates the toolbar with a "Clear" and a "Refresh" button.
        /// </summary>
        public void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            toolbar.AddToClassList("toolbar-container");
            toolbar.Add(VisualElementUtils.CreateButton(OnClearClicked, "Clear", string.Empty).AddStyleClass("toolbar-button"));
            toolbar.Add(VisualElementUtils.CreateButton(OnRefreshClicked, "Refresh", string.Empty).AddStyleClass("toolbar-button"));
            toolbar.Add(VisualElementUtils.CreateEnumField(OnLogLevelSelected, "Log Level:", _presenter.GetCurrentLogLevel()));
            rootVisualElement.Insert((int)ReferenceFinderUIOrder.Toolbar, toolbar);
        }

        /// <summary>
        /// Creates the search type field.
        /// </summary>
        /// <param name="currentSearchType">
        /// Current value of the search type (Default value = SearchType.AutoGameObject)
        /// </summary>
        public void CreateSearchTypeField(SearchType currentSearchType = SearchType.Any)
        {
            rootVisualElement.Insert((int)ReferenceFinderUIOrder.SearchTypeEnumField
                , VisualElementUtils.CreateEnumField(OnSearchTypeSelected, "Search Type:", currentSearchType)
                .AddStyleClass("body-element"));
        }

        /// <summary>
        /// Creates the object search field.
        /// </summary>
        /// <param name="currentSearchType">
        /// Current value of the search type (Default value = SearchType.AutoGameObject)
        /// </summary>
        public void CreateObjectField(SearchType currentSearchType = SearchType.Any)
        {
            bool searchOnScenes = false;
            Type objectType = null;
            switch (currentSearchType)
            {
                case SearchType.GameObject:
                    searchOnScenes = true;
                    objectType = typeof(GameObject);
                    break;
                case SearchType.ScriptableObject:
                    objectType = typeof(ScriptableObject);
                    break;
                case SearchType.Any:
                    searchOnScenes = true;
                    objectType = typeof(Object);
                    break;
            }

            rootVisualElement.Insert((int)ReferenceFinderUIOrder.ObjectField
                , VisualElementUtils.CreateObjectField(OnObjectSelected, objectType, "Object:", searchOnScenes)
                .AddStyleClass("body-element"));
        }

        /// <summary>
        /// Creates the "find references" button (and any other necessary elements).
        /// </summary>
        /// <param name="tooltip">
        /// Button's tooltip (Default value = "")
        /// </summary>
        /// <param name="buttonState">
        /// State of the button (Default value = false)
        /// </summary>
        public void CreateFindReferencesElements(string tooltip = "", bool buttonState = false)
        {
            VisualElement tooltipElement = new VisualElement();
            tooltipElement.tooltip = tooltip;
            Button findReferencesButton = VisualElementUtils.CreateButton(OnFindReferencesClicked, "Find References", string.Empty);
            findReferencesButton.SetEnabled(buttonState);
            findReferencesButton.AddToClassList("find-references-element");
            tooltipElement.Add(findReferencesButton);
            rootVisualElement.Insert((int)ReferenceFinderUIOrder.FindReferencesButton, tooltipElement);
        }

        /// <summary>
        /// Callback for when the clear button is pressed.
        /// </summary>
        private void OnClearClicked()
        {
            _presenter.Clear();
        }

        /// <summary>
        /// Callback for when the refresh button is pressed.
        /// </summary>
        private void OnRefreshClicked()
        {
            _presenter.Refresh();
        }

        /// <summary>
        /// Callback for when the log level is updated/selected.
        /// </summary>
        /// <param name="evt">
        /// Event data with updated information
        /// </param>
        private void OnLogLevelSelected(ChangeEvent<Enum> evt)
        {
            _presenter.ChangeLogLevel((LogLevel)evt.newValue);
        }

        /// <summary>
        /// Callback for when the search type is updated/selected.
        /// </summary>
        /// <param name="evt">
        /// Event data with updated information
        /// </param>
        private void OnSearchTypeSelected(ChangeEvent<Enum> evt)
        {
            _presenter.ChangeSearchType((SearchType)evt.newValue);
        }

        /// <summary>
        /// Callback for when the object field is updated.
        /// </summary>
        /// <param name="evt">
        /// Event data with updated information
        /// </param>
        private void OnObjectSelected(ChangeEvent<Object> evt)
        {
            _presenter.ChangeObject(evt.newValue);
        }

        /// <summary>
        /// Callback to start finding the references.
        /// </summary>
        private void OnFindReferencesClicked()
        {
            _presenter.FindReferences();
        }

        /// <summary>
        /// Callback for when the scene is closing.
        /// </summary>
        /// <param name="scene">
        /// Scene closing
        /// </summary>
        /// <param name="buttonState">
        /// Whether the scene is being removed
        /// </summary>
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            _presenter.OnSceneClosing(scene);
        }

        /// <summary>
        /// Callback for when the scene is saved.
        /// </summary>
        /// <param name="scene">
        /// Scene saving
        /// </summary>
        private void OnSceneSaved(Scene scene)
        {
            _presenter.OnSceneSaved(scene);
        }

        /// <summary>
        /// Shows the references lists in a scroll view.
        /// </summary>
        /// <param name="objectsPerLocationDictionary">
        /// Dictionary with the list of objects in each location found
        /// </summary>
        public void ShowReferencesPerLocation(Dictionary<string, Object[]> objectsPerLocationDictionary)
        {
            ScrollView scrollView = new ScrollView();
            foreach (string sceneName in objectsPerLocationDictionary.Keys)
            {
                scrollView.Add(CreateListViewForReferences(sceneName, objectsPerLocationDictionary[sceneName]));
            }
            rootVisualElement.Insert((int)ReferenceFinderUIOrder.ReferencesLists, scrollView);
        }

        /// <summary>
        /// Creates a list view for a specific location and objects found
        /// </summary>
        /// <param name="locationName">
        /// Name of the location where the object was found
        /// </summary>
        /// <param name="foundObjects">
        /// Objects with references
        /// </summary>
        private VisualElement CreateListViewForReferences(string locationName, Object[] foundObjects)
        {
            if (foundObjects.Length == 0)
            {
                return new Label($"No references were found in {locationName}").AddStyleClass("body-element");
            }
            VisualElement container = new VisualElement();
            Func<VisualElement> makeItem = () =>
            {
                ObjectField objectField = new ObjectField();
                objectField.DisableDragAndDrop();
                objectField.DisableSearchObjectButton();
                return objectField;
            };
            Action<VisualElement, int> bindItem = (element, index) =>
            {
                if (element is ObjectField objectField)
                {
                    objectField.value = foundObjects[index];
                }
            };
            ListView listView = VisualElementUtils.CreateListView(foundObjects, bindItem, makeItem);
            Label referencesFoundLabel = new Label($"List of references for {locationName}");
            container.Add(referencesFoundLabel.AddStyleClass("body-element"));
            container.Add(listView.AddStyleClass("body-element"));
            return container;
        }

        /// <summary>
        /// Subscribes to any necessary events, such as scene closing, saved
        /// and hierarchy changed.
        /// </summary>
        private void SubscribeToEvents()
        {
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        /// <summary>
        /// Unsubscribes from any of the events subscribed on initialization.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
        }

        /// <summary>
        /// Helper method to clear a visual element in root at the given
        /// index.
        /// </summary>
        /// <param name="index">
        /// Index position of child visual element to remove
        /// </param>
        public void ClearRootAtIndex(int index)
        {
            if (rootVisualElement.childCount < index + 1)
            {
                CustomDebug.Log("Index out of range for root visual element.", LogType.Warning, _presenter.GetCurrentLogLevel());
                return;
            }
            rootVisualElement.RemoveAt(index);
        }

        /// <summary>
        /// Helper method to clear all visual elements from root.
        /// </summary>
        public void ClearGUI()
        {
            if (_styleSheet != null)
            {
                rootVisualElement.styleSheets.Remove(_styleSheet);
            }
            _styleSheet = Resources.Load<StyleSheet>("ReferenceFinderStyleSheet");
            rootVisualElement.styleSheets.Add(_styleSheet);
            rootVisualElement.AddToClassList("root-element");
            rootVisualElement.Clear();
        }
    }
}
