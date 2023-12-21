using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/*
 * These are static methods used in the editor scripts of the Equipment System.
 */

namespace InfinityPBR
{
    
    
    /// <summary>
    /// Finds all prefab objects in the project with EquipmentObject.cs attached, and will return a list of game objects
    /// </summary>
    
    [System.Serializable]
    public static class EquipmentSystemStatic
    {
        public static List<GameObject> WardrobePrefabManagers = new List<GameObject>();
        
        public static List<string> EquipmentObjectTypes(List<GameObject> equipmentObjects)
        {
            var newList = new List<string>();
            foreach (var obj in equipmentObjects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                string[] pathParts = path.Split('/');
                var type = pathParts[pathParts.Length - 2];
                if (newList.Contains(type)) continue;

                newList.Add(type);
            }

            return newList.OrderBy(x => x).ToList();
        }

        public static string ParentDirectoryName(GameObject gameObject)
        {
            var path = AssetDatabase.GetAssetPath(gameObject);
            string[] pathParts = path.Split('/');
            return pathParts[pathParts.Length - 2];
        }
        
        public static List<GameObject> EquipmentObjectObjects(List<GameObject> equipmentObjects, string type)
        {
            return equipmentObjects
                .Distinct() // Only distinct assets
                .OrderBy(x => x.name) // Alphabetize
                .Where(x => !PrefabUtility.IsPartOfPrefabInstance(x.gameObject)) // Ensure it is not part of an instance (i.e in the scene)
                .Where(x => ParentDirectoryName(x.gameObject) == type)
                .Select(x => x.gameObject) // Select the game object itself
                .ToList();
        }

        public static string[] AllPrefabGuids => AssetDatabase.FindAssets("t:Prefab");
        public static string[] AllPrefabPaths => AllPrefabGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        
        public static List<GameObject> EquipmentObjectObjects()
        {
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (var path in AllPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path) as GameObject;
                if (!prefab.TryGetComponent(out EquipmentObject equipmentObject)) continue;
                foundObjects.Add(prefab);
            }
            
            if (foundObjects.Count == 0) return foundObjects;

            return foundObjects
                .Distinct() // Only distinct assets
                .OrderBy(x => x.name) // Alphabetize
                .Where(x => !PrefabUtility.IsPartOfPrefabInstance(x.gameObject)) // Ensure it is not part of an instance (i.e in the scene)
                .Select(x => x.gameObject) // Select the game object itself
                .ToList();
        }
        
        /// <summary>
        /// Provides a list of the names of all prefabs in the project with an EquipmentObject.cs attached
        /// </summary>
        /// <returns></returns>
        public static List<string> EquipmentObjectObjectNames(List<GameObject> equipmentObjects) => equipmentObjects.Count > 0 
            ? equipmentObjects
                .Where(x => x.gameObject != null)
                .Select(x => x.name)
                .ToList() 
            : new List<string>();
        
        /// <summary>
        /// Finds all prefab objects in the project with WardrobePrefabManager.cs attached, and will return a list of game objects
        /// </summary>
        public static List<GameObject> CacheWardrobePrefabManagers()
        {
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (var path in AllPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path) as GameObject;
                if (!prefab.TryGetComponent(out WardrobePrefabManager equipmentObject)) continue;
                foundObjects.Add(prefab);
            }

            WardrobePrefabManagers = foundObjects;
            return WardrobePrefabManagers;

            return foundObjects
                .Distinct() // Only distinct assets
                .OrderBy(x => x.name) // Alphabetize
                .Where(x => !PrefabUtility.IsPartOfPrefabInstance(x.gameObject)) // Ensure it is not part of an instance (i.e in the scene)
                .Select(x => x.gameObject) // Select the game object itself
                .ToList();
        }
    }
}
