using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReferenceFinder.Extensions;

// using GameObject = UnityEngine.GameObject;
// using Object = UnityEngine.Object;
// using ScriptableObject = UnityEngine.ScriptableObject;

namespace ReferenceFinder.Utils
{
    /// <summary>
    /// Static class responsible with general purpose functions/methods
    /// related to editor usage.
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Helper method that gets all of the active scenes in an
        /// array.
        /// </summary>
        /// <returns>
        /// Array of all active scenes
        /// </returns>
        public static Scene[] GetAllActiveScenes()
        {
            int length = EditorSceneManager.sceneCount;
            List<Scene> activeSceneList = new List<Scene>();
            for (int i = 0; i < length; i++)
            {
                Scene currentScene = EditorSceneManager.GetSceneAt(i);
                if (currentScene.isLoaded)
                {
                    activeSceneList.Add(currentScene);
                }
            }
            return activeSceneList.ToArray();
        }


        /// <summary>
        /// Helper method that gets all of prefabs from the
        /// project.
        /// </summary>
        /// <returns>
        /// Array of all prefab GameObjects
        /// </returns>
        public static GameObject[] GetAllPrefabsFromProject()
        {
            string[] GUIDs = AssetDatabase.FindAssets("t:Prefab");
            List<GameObject> prefabs = new List<GameObject>();
            foreach (string GUID in GUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                GameObject prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabGameObject == null)
                {
                    continue;
                }
                GameObject[] objectsInPrefab = prefabGameObject.GetAllAssociatedGameObjects();
                foreach (GameObject gameObjectInPrefab in objectsInPrefab)
                {
                    prefabs.Add(gameObjectInPrefab);
                }
            }
            return prefabs.ToArray();
        }

        /// <summary>
        /// Helper method that gets all of ScriptableObjects from the
        /// project.
        /// </summary>
        /// <returns>
        /// Array of all ScriptableObjects
        /// </returns>
        public static ScriptableObject[] GetAllScriptableObjectsFromProject()
        {
            string[] GUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
            List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();
            foreach (string GUID in GUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(GUID);
                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (scriptableObject == null)
                {
                    continue;
                }
                scriptableObjects.Add(scriptableObject);
            }
            return scriptableObjects.ToArray();
        }

        /// <summary>
        /// Method that checks if an object has any references
        /// related to the given Object parameter, 'to', within its field
        /// and returns the result.
        /// </summary>
        /// <param name="unityObject">
        /// Object to be check for references
        /// </param>
        /// <param name="to">
        /// Object to find references
        /// </param>
        /// <returns>
        /// True if a reference was found, false otherwise
        /// </returns>
        public static bool HasReferenceTo<T>(T unityObject, Object to) where T : Object
        {
            SerializedObject serializedComponent = new SerializedObject(unityObject);
            SerializedProperty iterator = serializedComponent.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.IsUnityEvent())
                {
                    // We might have a UnityEvent here
                    SerializedProperty persistentCalls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    if (persistentCalls == null || persistentCalls.arraySize == 0)
                        continue;
                    
                    for (int i = 0; i < persistentCalls.arraySize; i++)
                    {
                        SerializedProperty persistentCall = persistentCalls.GetArrayElementAtIndex(i);
                        SerializedProperty persistentCallTarget = persistentCall.FindPropertyRelative("m_Target");
                        if (persistentCallTarget == null)
                            continue;

                        if (HasReferenceTo(persistentCallTarget, to))
                        {
                            return true;
                        }
                    }
                }
                else if (HasReferenceTo(iterator, to))
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool HasReferenceTo(SerializedProperty property, Object to)
        {
            if (!property.IsObjectReference()) return false;
            return property.objectReferenceValue == to;
        }
    }
}
