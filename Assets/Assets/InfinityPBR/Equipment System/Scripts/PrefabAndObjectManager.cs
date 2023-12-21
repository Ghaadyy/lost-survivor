using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = System.Random;

/*
 * Prefab Child Manager is meant to make it a bit easier to set up and manage large amounts of prefab options, whether
 * they are prefabs added at runtime or children of an object turned on/off. It makes it easier to create groups of
 * objects, and then activate and deactivate them at runtime with a single line of code.
 *
 * Use with "Wardrobe Prefab Manager" to include rigging and blend shape handling (for Infinity PBR characters)
 */

namespace InfinityPBR
{
    [System.Serializable]
    public class PrefabAndObjectManager : MonoBehaviour
    {
        public PrefabChildEvent _event;
        
        public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
        public bool onlyOneGroupActivePerType = true;
        public bool unpackPrefabs = true;
        public bool revertToDefaultGroupByType = true;
        public string objectFit = "";

        public WardrobePrefabManager WardrobePrefabManager => GetWardrobePrefabManager();
        private WardrobePrefabManager _wardrobePrefabManager;
    
        public BlendShapesManager BlendShapesManager => GetBlendShapesManager();
        private BlendShapesManager _blendShapesManager;
        
        private BlendShapesManager GetBlendShapesManager()
        {
            if (_blendShapesManager != null) return _blendShapesManager;
            if (TryGetComponent(out BlendShapesManager foundManager))
                _blendShapesManager = foundManager;
            return _blendShapesManager;
        }
    
        private WardrobePrefabManager GetWardrobePrefabManager()
        {
            if (_wardrobePrefabManager != null) return _wardrobePrefabManager;
            if (TryGetComponent(out WardrobePrefabManager foundManager))
                _wardrobePrefabManager = foundManager;
            return _wardrobePrefabManager;
        }
        
        public Transform thisTransform => transform;
        [HideInInspector] public bool showHelpBoxes = true;
        [HideInInspector] public bool showSetup = true;
        [HideInInspector] public bool showFullInspector = false;
        [HideInInspector] public bool showPrefabGroups = true;
        [HideInInspector] public bool instantiatePrefabsAsAdded = true;
        [HideInInspector] public List<GameObject> equipmentObjects = new List<GameObject>();

        public List<string> GroupTypeNames => GetGroupTypeNames();
        private List<string> _groupTypeNames = new List<string>();
        public bool cacheTypes = true;

        private void Start()
        {
            if (_event == null)
                _event = new PrefabChildEvent();

            cacheTypes = true; // Set true on start, so this will trigger the first time it's called.
        }

        public PrefabGroup CreateNewPrefabGroup(string groupType = null)
        {
            var newGroup = new PrefabGroup {name = GetNextDefaultName(), groupType = groupType};
            newGroup.CreateNewUid();

            prefabGroups.Add(newGroup);
            return newGroup;
        }

        private string GetNextDefaultName()
        {
            int g = prefabGroups.Count;
            var newName = "Prefab Group " + g;
            while (NameExists(newName))
            {
                g++;
                newName = "Prefab Group " + g;
            }

            return newName;
        }

        private bool NameExists(string newName)
        {
            if (prefabGroups.FirstOrDefault(x => x.name == newName) != null)
                return true;
            return false;
        }

        public void ActivateRandomGroupType()
        {
            Debug.Log($"Count: {GroupTypeNames.Count}");
            var groupType = GroupTypeNames[UnityEngine.Random.Range(0, GroupTypeNames.Count)];
            ActivateRandomGroup(groupType);
        }
        
        public void ActivateRandomAllGroups()
        { 
            foreach (var groupType in GroupTypeNames)
            {
                if (!CanRandomize(groupType)) continue;
                ActivateRandomGroup(groupType);
            }
        }

        /// <summary>
        /// Will activate a random group of a named type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allTypes"></param>
        public void ActivateRandomGroup(string type, bool excludeActive = true)
        {
            if (string.IsNullOrWhiteSpace(type)) return;
            
            ActivateGroup(GetGroupsOfType(type)
                .Where(x => !x.isActive && x.canRandomize)
                .ToList()
                .TakeRandom());
            
            //var groupsOfType = GetGroupsOfType(type);
            //ActivateGroup(groupsOfType[UnityEngine.Random.Range(0, groupsOfType.Count)]);
        }

        /// <summary>
        /// Activate a group by name
        /// </summary>
        /// <param name="groupName"></param>
        public void ActivateGroup(string groupName)
        {
            var prefabGroup = prefabGroups.FirstOrDefault(x => x.name == groupName);
            if (prefabGroup == null)
            {
                Debug.LogWarning("Warning: No prefab group found with the name " + groupName);
                return;
            }
            
            ActivateGroup(prefabGroup);
        }

        /// <summary>
        /// Activate a group by index
        /// </summary>
        /// <param name="groupIndex"></param>
        public void ActivateGroup(int groupIndex)
        {
            if (prefabGroups.Count < groupIndex || prefabGroups.Count == 0)
            {
                Debug.LogWarning("Warning: Group index (" + groupIndex + ") out of range.");
                return;
            }
            
            ActivateGroup(prefabGroups[groupIndex]);
        }

        /// <summary>
        /// Will activate a group which is passed into this. Will also deactivate others of the same type, if any and
        /// onlyOneGroupActivePerType = true.
        /// </summary>
        /// <param name="group"></param>
        public void ActivateGroup(PrefabGroup group)
        {
            // If we are only allowing one group per type, and the group has a type, then deactivate the other groups
            // of this type. Don't check for default, as we will be activating this group right after.
            if (onlyOneGroupActivePerType && !String.IsNullOrWhiteSpace(group.groupType))
                DeactivateGroupsOfType(group.groupType, false);

            group.isActive = true; // Set this to be true -- group is now active

            // This is where we activate each group. Instantiate prefabs or turn on game objects.
            foreach (var groupObject in group.groupObjects)
            {
                if (groupObject.isPrefab && groupObject.inGameObject == groupObject.objectToHandle)
                    groupObject.inGameObject = null;

                if (!groupObject.render) continue; // Do not render this!

                // This is where we instantiate the prefab
                if (groupObject.isPrefab 
                    && groupObject.objectToHandle 
                    && groupObject.inGameObject == null)
                {
                    // Instantiate the object, set it's transform info, and make sure it's active.
                    groupObject.inGameObject = Instantiate(groupObject.objectToHandle, groupObject.parentTransform.position, groupObject.parentTransform.rotation, groupObject.parentTransform);
                    //groupObject.inGameObject.AddComponent<DeleteMeIfPrefabGroupIsNotActive>();
                    //groupObject.inGameObject.GetComponent<DeleteMeIfPrefabGroupIsNotActive>().prefabGroup = group;
#if UNITY_EDITOR
                    Undo.RegisterCreatedObjectUndo (groupObject.inGameObject, "Created go");
#endif
                    groupObject.inGameObject.SetActive(true);
#if UNITY_EDITOR
                    // If we are in the editor, then we will unpack this prefab
                    if (unpackPrefabs && PrefabUtility.IsAnyPrefabInstanceRoot(groupObject.inGameObject))
                        PrefabUtility.UnpackPrefabInstance(groupObject.inGameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
#endif

                    // TryBlendShapesManager(); // March 13, 2022 -- We can do this once later, we don't have to do it each time.
                    
                }
                else if (!groupObject.isPrefab)
                {
                    // The object is not a prefab, so we will just turn on the object.
                    if (groupObject.objectToHandle)
                    {
                        groupObject.objectToHandle.SetActive(true);
                        groupObject.inGameObject = groupObject.objectToHandle;
                    }
                }
            }

            TryWardrobePrefabManagerAutoRig();
            
            TryWardrobePrefabManagerOnActivate(group);

            TryBlendShapesManager();
        }

        private void TryWardrobePrefabManagerOnActivate(PrefabGroup group)
        {
            if (!WardrobePrefabManager) return;
                            
            WardrobePrefabManager.OnActivate(group.name);
        }

        // If the Wardrobe Prefab Manager is attached, then we may want to auto rig the new objects
        private void TryWardrobePrefabManagerAutoRig()
        {
            if (!WardrobePrefabManager) return; // Return if there is none
            if (!WardrobePrefabManager.autoRigWhenActivated) return; // Return if we aren't auto rigging
             
            // IPBR_CharacterEquip.EquipCharacter(gameObject);  // THIS IS THE OLD METHOD
            EquipObject.Equip(gameObject);
        }

        // If the Blend Shapes Manager is attached, then we will LoadBlendShapeData to make sure the new
        // objects get the right information set.
        private void TryBlendShapesManager()
        {
            if (!BlendShapesManager) return;

            BlendShapesManager.LoadBlendShapeData();
        }
        
        /// <summary>
        /// Deactivate a group by PrefabGroup
        /// </summary>
        /// <param name="group"></param>
        /// <param name="checkForDefault"></param>
        public void DeactivateGroup(PrefabGroup group, bool checkForDefault = true)
        {
            group.isActive = false;
            
            if (WardrobePrefabManager)
                WardrobePrefabManager.OnDeactivate(group.name);

            // ITEMS
            foreach (var groupObject in group.groupObjects)
            {
                if (groupObject.isPrefab)
                {
                    DestroyObject(groupObject.inGameObject);
                    groupObject.inGameObject = null;
                    continue;
                }
                
                if (groupObject.objectToHandle)
                    groupObject.objectToHandle.SetActive(false);
            }

            if (checkForDefault && !String.IsNullOrWhiteSpace(group.groupType))
            {
                Debug.Log($"Check for default is true and group type is {group.groupType}");
                CheckForDefaultGroup(group.groupType);
            }
        }

        /// <summary>
        /// Deactivate a group by name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="checkForDefault"></param>
        public void DeactivateGroup(string groupName, bool checkForDefault = true)
        {
            var prefabGroup = prefabGroups.FirstOrDefault(x => x.name == groupName);
            if (prefabGroup == null)
            {
                Debug.LogWarning("Warning: No prefab group found with the name " + groupName);
                return;
            }

            DeactivateGroup(prefabGroup, checkForDefault);
        }

        /// <summary>
        /// Deactivate a group by the index
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <param name="checkForDefault"></param>
        public void DeactivateGroup(int groupIndex, bool checkForDefault = true)
        {
            if (prefabGroups.Count < groupIndex || prefabGroups.Count == 0)
            {
                Debug.LogWarning("Warning: Group index (" + groupIndex + ") out of range.");
                return;
            }
            
            DeactivateGroup(prefabGroups[groupIndex], checkForDefault);
        }

        /// <summary>
        /// Deactivate all groups of a specific type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="checkForDefault"></param>
        public void DeactivateGroupsOfType(string type, bool checkForDefault = true)
        {
            for (int g = 0; g < prefabGroups.Count; g++)
            {
                if (prefabGroups[g].groupType != type)
                    continue;

                DeactivateGroup(g, checkForDefault);
            }
        }

        /// <summary>
        /// This will activate the default group of the named type, if there is one.
        /// </summary>
        /// <param name="type"></param>
        private void CheckForDefaultGroup(string type)
        {
            int defaultGroupIndex = -1;
            for (int g = 0; g < prefabGroups.Count; g++)
            {
                if (prefabGroups[g].groupType != type) continue; // Continue if the type is wrong
                if (!prefabGroups[g].isDefault) continue; // Continue if this isn't the default
                if (GroupIsActive(g) > 0) return; // If it is the default, and active, return
                
                ActivateGroup(prefabGroups[g]); // Activate the default group
                return;
            }
        }

        public void DestroyObject(GameObject inGameObject)
        {
            if (inGameObject)
            {
#if UNITY_EDITOR
                // July 9, 2022 - Can't do this, as it unpacks the prefab...need to fix the issue another way
                bool wasPrefab = false;
                if (PrefabUtility.IsPartOfAnyPrefab(inGameObject))
                {
                    wasPrefab = true;
                    inGameObject.SetActive(false);
                }
                else
                {
                    DestroyImmediate(inGameObject);
                }
                    Debug.LogWarning($"<color=ffff00>Prefab and Object Manager:</color> One or more object managed " +
                                     $"by the script was saved as a prefab. In edit mode, this keeps the script from destroying " +
                                     $"them as usual. For now, these will be turned off rather than destroyed.\n\nIn play mode, " +
                                     $"objects will be destroyed as expected. This is only a problem if the prefab is continuously saved," +
                                     $" as more and more objects will be turned off and saved with the prefab, requiring manual " +
                                     $"cleanup later.");
#else
                Destroy(inGameObject);
#endif
            }
        }

        public int GroupIsActive(PrefabGroup group)
        {
            var renderableObjects = group.groupObjects.Count(x => x.render);
            var livePrefabObjects = group.groupObjects.Count(x => x.isPrefab && x.inGameObject != null);
            var liveObjectObjects = group.groupObjects.Count(x => !x.isPrefab && x.objectToHandle.activeSelf);
            var liveObjects = liveObjectObjects + livePrefabObjects;

            if (renderableObjects == 0) return 0;
            if (renderableObjects == liveObjects) return 2;
            if (renderableObjects > liveObjects && liveObjects > 0) return 1;
            return 0;
            
            
            
            int prefabs = 0;
            int inGameObjects = 0;
            
            for (int i = 0; i < group.groupObjects.Count; i++)
            {
                if (!group.groupObjects[i].objectToHandle)
                    continue;
                
                // If render is true, then increase the count of prefabs for this group                
                if (group.groupObjects[i].render)
                    prefabs++;
                else if (group.groupObjects[i].isPrefab && group.groupObjects[i].inGameObject)
                    inGameObjects++;
                else if (!group.groupObjects[i].isPrefab && group.groupObjects[i].objectToHandle.activeSelf)
                    inGameObjects++;
            }

            if (prefabs == inGameObjects && (prefabs > 0 || group.isActive))
                return 2;
            if (prefabs > inGameObjects && inGameObjects > 0)
                return 1;

            return 0;
        }

        public int GroupIsActive(int groupIndex) => GroupIsActive(prefabGroups[groupIndex]);
        public int GroupIsActive(string groupName) => GroupIsActive(GetPrefabGroup(groupName));

        public void AddPrefabToGroup(PrefabGroup group, GameObject prefab = null)
        {
            var newGroupObject = new GroupObject
            {
                objectToHandle = prefab == null ? group.newPrefab : prefab, parentTransform = thisTransform, isPrefab = true
            };
            group.groupObjects.Add(newGroupObject);
        }
        
        public void AddGameObjectToGroup(GameObject newObject, PrefabGroup group)
        {
            var newGroupObject = new GroupObject
            {
                objectToHandle = group.newPrefab, isPrefab = false
            };
            group.groupObjects.Add(newGroupObject);
        }

        /// <summary>
        /// Returns a List<string> with the group names of a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<string> GetGroupNamesOfType(string type) => prefabGroups
            .Where(x => x.groupType == type)
            .Select(x => x.name)
            .ToList();

        public List<PrefabGroup> GetGroupsOfType(string type, bool excludeActive = true) => prefabGroups
            .Where(x => x.groupType == type)
            .ToList();
        
        
        public void MarkPrefabs ()
        {
            foreach(PrefabGroup prefabGroup in prefabGroups)
            {
                foreach (GroupObject groupObject in prefabGroup.groupObjects)
                {
                    if (!groupObject.objectToHandle) continue;
                    
                    if (groupObject.isPrefab && groupObject.inGameObject == groupObject.objectToHandle)
                        groupObject.inGameObject = null;

                    groupObject.isPrefab = !groupObject.objectToHandle.transform.IsChildOf(transform); // Not a prefab if it's a child of the prefab manager object
                    
                    if (groupObject.isPrefab && groupObject.inGameObject == groupObject.objectToHandle)
                        groupObject.inGameObject = null;
                }
            }
        }

        /// <summary>
        /// Returns a PrefabGroup of a given name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public PrefabGroup GetPrefabGroup(string groupName)
        {
            for (int i = 0; i < prefabGroups.Count; i++)
            {
                if (prefabGroups[i].name == groupName)
                    return GetPrefabGroup(i);
            }

            Debug.LogWarning("Warning: Could not find prefab group named " + groupName);
            return null;
        }
        
        /// <summary>
        /// Returns a List<string> with all Prefab Group types.
        /// </summary>
        /// <returns></returns>
        public List<string> GetGroupTypeNames()
        {
            if (!cacheTypes) return _groupTypeNames;
            _groupTypeNames = CacheGroupTypeNames();
            cacheTypes = false;
            return _groupTypeNames;
        }
        
        private List<string> CacheGroupTypeNames() => _groupTypeNames = prefabGroups.OrderBy(x => x.groupType).Select(x => x.groupType).Distinct().ToList();

        public PrefabGroup GetPrefabGroup(int index)
        {
            if (prefabGroups.Count < index)
                return prefabGroups[index];

            Debug.Log($"Warning: The index {index} is out of range of the prefab groups {prefabGroups.Count}");
            return null;
        }

        public void RemovePrefabGroup(PrefabGroup prefabGroup) => prefabGroups.RemoveAll(x => x == prefabGroup);

        public void SetUid()
        {
            foreach (var group in prefabGroups)
            {
                if (string.IsNullOrEmpty(group.uid))
                    group.CreateNewUid();
            }
        }

        public void CopyGroup(PrefabGroup group)
        {
            var clone = JsonUtility.FromJson<PrefabGroup>(JsonUtility.ToJson(group));
            clone.isDefault = false; // Copy should not be default
            clone.name = $"{clone.name} Copy"; // Add this to the name
            clone.name = clone.name.Replace("Copy Copy", "Copy"); // Avoid duplicate copy in name
            foreach (var obj in clone.groupObjects)
                obj.inGameObject = null; // Make sure all inGameObjects are null (in case of copying active group)
            prefabGroups.Add(clone); // Add the group
        }

        public bool CanRandomize(string typeName)
        {
            if (prefabGroups.Count(x => x.groupType == typeName) < 2) return false;
            if (prefabGroups.Count(x => x.canRandomize) < 2) return false;

            return true;
        }

        /*
        public void RandomGroup(string typeName, bool excludeActive = true)
        {
            List<PrefabGroup> groups = prefabGroups.Where(x => x.groupType == typeName).ToList();
            if (excludeActive) groups = groups.Where(x => !x.isActive).ToList();
            groups = groups.ToList();

            ActivateGroup(groups[UnityEngine.Random.Range(0, groups.Count)]);
        }
        */
    }

    [System.Serializable]
    public class PrefabGroup
    {
        [HideInInspector] public bool showPrefabs = false;
        [HideInInspector] public bool showShapes = false;
        [HideInInspector] public bool showObjectsOnActivate = false;
        [HideInInspector] public bool showObjectsOnDeactivate = false;
        public string uid;
        public bool isDefault = false;
        public string name;
        public string groupType;
        public bool isActive;

        [FormerlySerializedAs("prefabObjects")] public List<GroupObject> groupObjects = new List<GroupObject>();

        [HideInInspector] public GameObject newPrefab;
        [HideInInspector] public GameObject newGameObject;
        
        // EDITOR SCRIPT FOR SELECTING NEW EQUIPMENT OBJECTS
        [HideInInspector] public List<GameObject> equipmentObjectObjects = new List<GameObject>();
        [HideInInspector] public List<string> equipmentObjectObjectNames = new List<string>();
        [HideInInspector] public int equipmentObjectIndex;
        [HideInInspector] public List<string> equipmentObjectTypes = new List<string>();
        [HideInInspector] public int equipmentObjectTypeIndex;
        [FormerlySerializedAs("excludeFromRandom")] [HideInInspector] public bool canRandomize = true;

        public bool GroupObjectsContain(GameObject obj) => groupObjects.FirstOrDefault(x => x.objectToHandle == obj) != null;
        
        public void RemoveGroupObject(GroupObject groupObject) => groupObjects.Remove(groupObject);

        public void CreateNewUid(bool forceNew = false)
        {
            if (!string.IsNullOrEmpty(uid) && !forceNew) return;
            uid = GetUid();
        }
        
        private string GetUid()
        {
#if UNITY_EDITOR
            return GUID.Generate().ToString();
#endif
            return "ThisShoutNotHappen";
        }
    }

    [System.Serializable]
    public class GroupObject
    {
        [FormerlySerializedAs("prefab")] public GameObject objectToHandle;
        public Transform parentTransform;
        public GameObject inGameObject;
        public bool render = true;
        public MeshRenderer meshRenderer;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        public bool isPrefab = false;
    }
    
    [System.Serializable]
    public class PrefabChildEvent : UnityEvent<string>
    {
        
    }
}

