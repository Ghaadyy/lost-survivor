using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR;
using static InfinityPBR.EquipmentSystemStatic;

namespace InfinityPBR
{
    [CustomEditor(typeof(PrefabAndObjectManager))]
    [CanEditMultipleObjects]
    [Serializable]
    public class PrefabAndObjectManagerEditor : InfinityEditor<PrefabAndObjectManager>
    {
        private Color inactiveColor2 = new Color(0.75f, .75f, 0.75f, 1f);
        private Color activeColor = new Color(0.6f, 1f, 0.6f, 1f);
        private Color activeColor2 = new Color(0.0f, 1f, 0.0f, 1f);
        private Color mixedColor = Color.yellow;
        private Color redColor = new Color(1f, 0.25f, 0.25f, 1f);

        private WardrobePrefabManager WardrobePrefabManager => Manager.WardrobePrefabManager;
        private BlendShapesManager BlendShapesManager => Manager.BlendShapesManager;

        private PrefabAndObjectManager Manager => GetManager();
        private PrefabAndObjectManager _prefabAndObjectManager;
        private List<string> GroupTypeNames => Manager.GetGroupTypeNames();

        private PrefabGroup _activateGroup;
        private PrefabGroup _deactivateGroup;

        private bool _cachedEquipmentObjects = false;
        private List<GameObject> _equipmentObjects = new List<GameObject>();

        private void CacheEquipmentObjects()
        {
            _equipmentObjects = EquipmentObjectObjects();
            Manager.equipmentObjects = _equipmentObjects;
            _cachedEquipmentObjects = true;
            Debug.Log($"<color=#00ff00>Cache Successful!</color> {_equipmentObjects.Count} Equipment Objects found.");
        }

        private PrefabAndObjectManager GetManager()
        {
            if (_prefabAndObjectManager != null) return _prefabAndObjectManager;
            _prefabAndObjectManager = (PrefabAndObjectManager) target;
            return _prefabAndObjectManager;
        }


        public Color ColorSet(int g) => ColorSet(Manager.prefabGroups[g]);

        public Color ColorSet(PrefabGroup group)
        {
            int v = Manager.GroupIsActive(group);
            if (v == 2)
                return activeColor2;
            if (v == 1)
                return mixedColor;
            return Color.white;
        }

        private void DefaultEditorBool(string optionString, bool value = true)
        {
            if (EditorPrefs.HasKey(optionString)) return;
            EditorPrefs.SetBool(optionString, value);
        }

        public void OnEnable()
        {
            _cachedEquipmentObjects = false;
            
            if (GetBool("Auto Find Equipment Objects On Enable"))
                CacheEquipmentObjects();

            RemoveMissingObjects();
            ReloadSources();

            Undo.undoRedoPerformed += UndoCallback;
            SetBool("Reset Since Load Equipment Object", false);
        }

        private void RemoveMissingObjects()
        {
            foreach (var group in Manager.prefabGroups)
            {
                group.groupObjects.RemoveAll(x => x.objectToHandle == null);
            }
        }
        
        private void ReloadSources(){
            EnsureTypeNames();
            DoCache(true);
            if (BlendShapesManager)
                BlendShapesManager.BuildMatchLists();
        }

        void UndoCallback()
        {
            Debug.Log("Undo Performed");
            DoCache();
        }

        private void DoCache(bool cacheGroups = false)
        {
            CacheTypes();
            if (!cacheGroups) return;
            CacheGroups();
        }

        private void CacheGroups()
        {
            if (WardrobePrefabManager == null)
            {
                Debug.LogWarning("Expecting a Wardrobe Prefab Manager component, but none was found.");
                return;
            }

            if (!_cachedEquipmentObjects)
            {
                Debug.LogWarning("<color=#ff0000>EquipmentObjects have not been cached!</color> You need to either push the \"Find EquipmentObjectPrefabs\" button at the top of this component, or toggle " +
                                 "on the \"Cache EquipmentObjects on Enable\" option, to run it automatically whenever an object like this is enabled in the Inspector.");
                return;
            }
            
            foreach (var group in Manager.prefabGroups)
            {
                // July 10, 2022 -  Only do this for groups that are open.
                if (!group.showPrefabs) continue;
                CacheEquipmentObjectTypesForGroup(group);
                CacheEquipmentObjectsForGroup(group);
                CacheEquipmentObjectNamesForGroup(group);

                // Make sure index is within range
                if (group.equipmentObjectIndex >= group.equipmentObjectObjects.Count)
                    group.equipmentObjectIndex = group.equipmentObjectObjects.Count - 1;
                if (group.equipmentObjectTypeIndex >= group.equipmentObjectTypes.Count)
                    group.equipmentObjectTypeIndex = group.equipmentObjectTypes.Count - 1;
            }
        }

        private void CacheEquipmentObjectTypesForGroup(PrefabGroup group)
        {
            group.equipmentObjectTypes = EquipmentObjectTypes(_equipmentObjects)
                .Where(x => CountOfObject(x) > 0).ToList();
        }

        private int CountOfObject(string type) => EquipmentObjectObjects(_equipmentObjects, type)
            .Count(x => x.GetComponent<EquipmentObject>().HasObjectNamed(Manager.gameObject.name));

        private void CacheEquipmentObjectNamesForGroup(PrefabGroup group)
            => group.equipmentObjectObjectNames = group.equipmentObjectObjects
                .Select(x => x.name)
                .ToList();

        private void CacheEquipmentObjectsForGroup(PrefabGroup group)
        {
            if (group.equipmentObjectTypes.Count == 0) return;
            if (group.equipmentObjectTypeIndex >= group.equipmentObjectTypes.Count)
                group.equipmentObjectTypeIndex = 0;

            if (group.equipmentObjectTypeIndex < 0) group.equipmentObjectTypeIndex = 0; // July 10, 2022 -- was getting argument out of range when this was -1
            if (EquipmentObjectObjects(_equipmentObjects, group.equipmentObjectTypes[group.equipmentObjectTypeIndex]).Count == 0)
                return;
            if (EquipmentObjectObjects(_equipmentObjects, group.equipmentObjectTypes[group.equipmentObjectTypeIndex])
                    .OrderBy(x => x.name)
                    .Where(x => x.GetComponent<EquipmentObject>().HasObjectNamed(Manager.gameObject.name))
                    .Count(x => !group.GroupObjectsContain(x)) == 0)
                return;
            /*
            group.equipmentObjectObjects = EquipmentObjectObjects(group.equipmentObjectTypes[group.equipmentObjectTypeIndex])
                .OrderBy(x => x.name)
                .Where(x => !group.GroupObjectsContain(x)) // Exclude if the group already contains this
                .Where(x => x.GetComponent<EquipmentObject>().HasObjectNamed(Manager.gameObject.name))
                .ToList();
                */

            var newList = new List<GameObject>();
            foreach (var obj in EquipmentObjectObjects(_equipmentObjects, group.equipmentObjectTypes[group.equipmentObjectTypeIndex])
                .OrderBy(x => x.name)
                .Where(x => !group.GroupObjectsContain(x)).ToList())
            {
                if (!obj.TryGetComponent(out EquipmentObject script))
                {
                    continue;
                }

                if (!script.HasObjectNamed(Manager.gameObject.name))
                {
                    continue;
                }

                newList.Add(obj);
            }
            group.equipmentObjectObjects =  newList;
        }
        
        private void EnsureTypeNames()
        {
            foreach (var group in Manager.prefabGroups)
            {
                if (!String.IsNullOrWhiteSpace(group.groupType)) continue;
                group.groupType = null;
            }
        }

        private void GroupActivateDeactivate()
        {
            if (_deactivateGroup != null)
                Manager.DeactivateGroup(_deactivateGroup);
            if (_activateGroup != null)
                Manager.ActivateGroup(_activateGroup);

            _deactivateGroup = null;
            _activateGroup = null;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(Manager, "Undo Changes");
            GroupActivateDeactivate();
            
            // Utility actions
            SetDefaultEditorPrefs();

            Manager.MarkPrefabs();
            Manager.SetUid();
            
            LinkToDocs("https://infinitypbr.gitbook.io/infinity-pbr/equipment-systems/prefab-and-object-manager");
            Space();

            HelpBoxMessage("PREFAB AND OBJECT MANAGER\n" +
                           "This inspector script is intended to make it easier to assign groups of prefabs and objects, and" +
                           " activate / deactivate them as a group. This could be helpful for managing modular " +
                           "wardrobe or other objects, such as props inside a room.\n\n" +
                           "The script handles instantiating and destroying prefabs as well as activating and deactivating " +
                           "objects already in your scene. Each group can handle any number of both types of objects.");

            Space();
            ShowUpdate();
            Space();
            
            // Inspector Drawing
            //Undo.RecordObject(Manager, "Undo Button Changes");
            SectionButtons();
            Space();

            Line();
            ShowActionButtons();
            Line();
            Space();
            
            //Undo.RecordObject(Manager, "Undo Setup & Option Changes");
            SetupAndOptions();

            //Undo.RecordObject(Manager, "Undo Object Delete");
            GroupTypes();

            //Undo.RecordObject(Manager, "Show Prefab Group OPtions");
            ShowPrefabGroups();
            
            Space();
            

            
            DrawDefaultInspector("Prefab and Object Manager");
            
            EditorUtility.SetDirty(this);
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Manager);
            /*
            PrefabUtility.RecordPrefabInstancePropertyModifications(Manager.gameObject);
            */
        }

        private void ShowActionButtons()
        {
            if (Button("Randomize All", 100))
            {
                Manager.ActivateRandomAllGroups();
            }
        }

        /*
         * This shows the buttons to load the various panels
         */
        private void SectionButtons()
        {
            // Cache values
            var tempGroups = EditorPrefs.GetBool("Prefab Manager Show Prefab Groups");
            var tempTypes = EditorPrefs.GetBool("Prefab Manager Show Group Types");
            var tempSetup = EditorPrefs.GetBool("Prefab Manager Show Setup And Options");
            
            // Show buttons
            EditorGUILayout.BeginHorizontal();
            SectionButton($"Prefab Groups ({Manager.prefabGroups.Count})", "Prefab Manager Show Prefab Groups");
            SectionButton($"Group Types ({GroupTypeNames.Count})", "Prefab Manager Show Group Types");
            SectionButton("Setup & Options", "Prefab Manager Show Setup And Options");
            EditorGUILayout.EndHorizontal();
            
            // Check for changes -- Ensure others are turned off
            if (!tempGroups && EditorPrefs.GetBool("Prefab Manager Show Prefab Groups"))
            {
                EditorPrefs.SetBool("Prefab Manager Show Group Types", false);
                EditorPrefs.SetBool("Prefab Manager Show Setup And Options", false);
            }
            if (!tempTypes && EditorPrefs.GetBool("Prefab Manager Show Group Types"))
            {
                EditorPrefs.SetBool("Prefab Manager Show Prefab Groups", false);
                EditorPrefs.SetBool("Prefab Manager Show Setup And Options", false);
            }
            if (!tempSetup && EditorPrefs.GetBool("Prefab Manager Show Setup And Options"))
            {
                EditorPrefs.SetBool("Prefab Manager Show Group Types", false);
                EditorPrefs.SetBool("Prefab Manager Show Prefab Groups", false);
            }
            
        }

        private void GroupTypes()
        {
            if (!EditorPrefs.GetBool("Prefab Manager Show Group Types")) return;

            HelpBoxMessage("Organize your groups into types, often used to ensure that only one group " +
                           "of each type is active at a time. An example would be \"Hair\" for characters, or " +
                           "perhaps \"Table Items\" for props on the top of a table. You can update the name of a type" +
                           "here.\n\n" +
                           "To add a new type, simply write it into the \"Type\" field when viewing your prefab groups. " +
                           "Each group starts without a type.\n\n" +
                           "To delete a type, remove all groups of that type, or change all groups to a different type.", MessageType.Info);

            foreach (var typeName in GroupTypeNames)
                DisplayGroupType(typeName);
        }

        private void DisplayGroupType(string typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName)) return;
            
            var oldName = typeName;
            var newName = EditorGUILayout.DelayedTextField(typeName, GUILayout.Width(250));
            if (oldName == newName) return;

            if (!UpdateGroupTypeName(oldName, newName))
                return;

            EditorPrefs.SetBool($"Prefab Manager Show Type {newName}", true);
        }

        /*
         * This will check to see if we can update the type name. If so, will change all the groups of the type to
         * the new name.
         */
        private bool UpdateGroupTypeName(string oldName, string newName)
        {
            newName = newName.Trim(); // Remove any whitespace before / after the content
            if (String.IsNullOrWhiteSpace(newName)) return false; // If it's empty, return
            if (oldName == newName) return false; // If we didn't change anything, return
            if (GroupTypeNames.Count(x => x == newName) > 0) // If we already have a type of that name, return
            {
                Debug.LogWarning($"Error: {newName} already exists!");
                return false;
            }

            // Make the update on all existing prefab groups
            foreach (var group in Manager.prefabGroups)
            {
                if (group.groupType != oldName) continue;
                group.groupType = newName;
            }

            CacheTypes(); // force the types cache to reload
            
            return true;
        }

        private void CacheTypes(bool value = true) => Manager.cacheTypes = value;

        
        
        private void DefaultInspector()
        {
            if (!EditorPrefs.GetBool("Prefab Manager Show Full Inspector")) return;
            
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        private void ShowPrefabGroups()
        {
            EditorGUILayout.BeginHorizontal();
            if (!EditorPrefs.GetBool("Prefab Manager Show Prefab Groups"))
            {
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Create New Group"))
            {
                Manager.CreateNewPrefabGroup();
                DoCache();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            Space();

            foreach(var typeName in GroupTypeNames)
            {
                StartVerticalBox();
                ShowGroupsOfType(typeName);
                EndVerticalBox();
                Space();
            }
        }

        private List<PrefabGroup> GroupsOfType(string groupType) => String.IsNullOrWhiteSpace(groupType) ? Manager.prefabGroups.Where(x => String.IsNullOrWhiteSpace(x.groupType)).ToList() : Manager.prefabGroups.Where(x => x.groupType == groupType).ToList();

        private void ShowGroupsOfType(string typeName = "")
        {
            var groupsOfType = GroupsOfType(typeName);
            var typeDetails = $"{groupsOfType.Count} groups";
            var prefsString = $"Prefab Manager Show Type {typeName}";
            
            
            StartRow();
            ColorsIf(GetBool(prefsString), Color.green, Color.black, Color.white, Color.white);
            if (Button($"{(GetBool(prefsString) ? symbolCircleOpen : symbolDash)}", 25))
            {
                ToggleBool(prefsString);
            }
            ContentColor(Color.white);
            LabelBig($"{(!String.IsNullOrWhiteSpace(typeName) ? $"{typeName}" : "[No type]")} ({typeDetails})", 200, 14,true);
            ContentColorIf(Manager.CanRandomize(typeName), Color.white, Color.grey);
            if (Button("Random", 60) && Manager.CanRandomize(typeName))
            {
                Manager.ActivateRandomGroup(typeName);
            }
            EndRow();
            ColorsIf(GetBool(prefsString), Color.green, Color.black, Color.white, Color.white);
            ContentColor(Color.white);
            
            if (!GetBool(prefsString)) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10)); // Indent
            ShowDefaultToggle(null, true);
            ShowRandomToggle(null, true);
            ShowObjectsButton(null, true);
            ShowShapesButton(null, true);
            ShowGroupName(null, true);
            ShowGroupType(null, true);
            ShowGroupActivateDeactivate(null, true);
            ShowCopy(null, true);
            ShowRemovePrefabGroup(null, true);
            EditorGUILayout.EndHorizontal();
            
            foreach (var group in groupsOfType)
                ShowPrefabGroupRow(group);
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button($"Create New{(string.IsNullOrWhiteSpace(typeName) ? "" : $" {typeName}")} Group", GUILayout.Width(250)))
            {
                Manager.CreateNewPrefabGroup(typeName);
                DoCache();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ShowPrefabGroupRow(PrefabGroup group)
        {
            GUI.backgroundColor = ColorSet(group);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10)); // Indent
            
            ShowDefaultToggle(group);
            ShowRandomToggle(group);
            ShowObjectsButton(group);
            ShowShapesButton(group);
            ShowGroupName(group);
            ShowGroupType(group);
            ShowGroupActivateDeactivate(group);
            ShowCopy(group);
            ShowRemovePrefabGroup(group);
            
            EditorGUILayout.EndHorizontal();

            ShowObjects(group);
            ShowShapes(group);
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        private void ShowCopy(PrefabGroup group, bool header = false)
        {
            if (header)
            { 
                Label("", 60);
                return;
            }

            if (Button($"Copy", 60))
            {
                Manager.CopyGroup(group);
            }
            ResetColor();
        }

        private void ShowObjects(PrefabGroup group)
        {
            if (!group.showPrefabs) return; // return if we haven't toggled this on
            
            // Show each of the objects attached to this Prefab Group
            foreach (var groupObject in group.groupObjects)
            {
                ShowGroupObject(group, groupObject);
                BackgroundColor(Color.white);
            }

            StartRow();
            ShowAddNewObjectField(group);
            ShowSelectEquipmentObject(group);
            EndRow();
        }

        private void ShowUpdate()
        {
            if (WardrobePrefabManager == null) return;
            
            BackgroundColor(Color.magenta);
            StartVerticalBox();
            if (!_cachedEquipmentObjects)
            {
                if (Button("Find EquipmentObject Prefabs"))
                {
                    CacheEquipmentObjects();
                    DoCache(true);
                }

                ResetColor();

                MessageBox(
                    "Click this button to find all prefabs with EquipmentObject components which match this Group. This process can take some time, and can be run manually, or automatically " +
                    $"if you don't mind the wait. This is required prior to adding any objects below, when the Wardrobe Prefab Manager is in use.\n\n Note: Progress bar will not run, as the method is much faster without showing the bar!");

                Space();
            }
            
            LeftCheckSetBool("Auto Find Equipment Objects On Enable", 
                $"Cache EquipmentObjects on Enable {symbolInfo}", 
                "When on, the system will cache all objects with EquipmentObject components on them whenever " +
                "an object with a Wardrobe Prefab Manager is viewed in the Inspector. This should be run before adding " +
                "Equipment Objects, but can be done manually via a button push, if this is toggled off.");
            EndVerticalBox();
        }
        
        private void ShowShapes(PrefabGroup group)
        {
            if (!group.showShapes) return; // return if we haven't toggled this on
            if (!WardrobePrefabManager)
            {
                Debug.Log("No Wardrobe Prefab Manager");
                return;
            }

            var blendShapeGroup = WardrobePrefabManager.GetGroup(group);
            if (blendShapeGroup == null)
            {
                Debug.Log($"Group {group.name} is null??");
                return;
            }
            
            string[] blendShapeNames = blendShapeGroup.blendShapeNames.ToArray();

            // ON ACTIVATE
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent($"On Activate {symbolInfo}", $"These shapes will be modified when this Prefab Group is " +
                                                                                   $"activated. This is how you can set shapes to make sure they fit with the " +
                                                                                   $"wardrobe in the Prefab Group. Select a shape, and then click \"Add To List\"."), GUILayout.Width(120));

            if (blendShapeGroup.blendShapeNames.Count == 0)
            {
                EditorGUILayout.LabelField("No Blend Shapes Available");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                blendShapeGroup.shapeChoiceIndex = EditorGUILayout.Popup(blendShapeGroup.shapeChoiceIndex, blendShapeNames);
                if (GUILayout.Button("Add To List"))
                {
                    WardrobePrefabManager.AddToList("Activate", blendShapeGroup);
                    SetAllDirty();
                }
                EditorGUILayout.EndHorizontal();
            }

            // ON ACTIVATE GLOBAL
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent($"Global Shapes {symbolInfo}", $"The top list contains only the shapes assigned to " +
                                                                                     $"the objects in the Prefab Group. \"Global Shapes\" will list all " +
                                                                                     $"shapes available."), GUILayout.Width(120));

            if (BlendShapesManager.matchList.Count == 0)
            {
                EditorGUILayout.LabelField("No Global Blend Shapes Available");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                BlendShapesManager.matchListIndex = EditorGUILayout.Popup(BlendShapesManager.matchListIndex,
                    BlendShapesManager.matchListNames);
                if (GUILayout.Button("Add To List"))
                {
                    WardrobePrefabManager.AddToList("Activate",
                        BlendShapesManager.matchList[BlendShapesManager.matchListIndex], blendShapeGroup);
                    SetAllDirty();
                }
                EditorGUILayout.EndHorizontal();
            }

            // ON ACTIVATE LIST
            for (int i = 0; i < blendShapeGroup.onActivate.Count; i++)
            {
                BlendShapeItem item = blendShapeGroup.onActivate[i];
                WardrobePrefabManagerDisplayItem(blendShapeGroup, item, i, "Activate", WardrobePrefabManager);
            }

            EditorGUILayout.EndVertical();
            

            // ON DEACTIVATE
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent($"On Deactivate {symbolInfo}", $"These shapes will be triggered when the Prefab Group is " +
                                                                                     $"deactivated. Often the shapes will match those in \"On Activate\"."), GUILayout.Width(120));

            if (blendShapeGroup.blendShapeNames.Count == 0)
            {
                EditorGUILayout.LabelField("No Blend Shapes Available");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                blendShapeGroup.shapeChoiceIndex = EditorGUILayout.Popup(blendShapeGroup.shapeChoiceIndex, blendShapeNames);
                if (GUILayout.Button("Add To List"))
                {
                    WardrobePrefabManager.AddToList("Deactivate", blendShapeGroup);
                    SetAllDirty();
                }
                if (GUILayout.Button("Revert Back"))
                {
                    WardrobePrefabManager.AddToList("Revert Back", blendShapeGroup);
                    SetAllDirty();
                }
                EditorGUILayout.EndHorizontal();
            }

            // ON DEACTIVATE GLOBAL
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent($"Global Shapes {symbolInfo}", $"The top list contains only the shapes assigned to " +
                                                                                     $"the objects in the Prefab Group. \"Global Shapes\" will list all " +
                                                                                     $"shapes available."), GUILayout.Width(120));

            if (BlendShapesManager.matchList.Count == 0)
            {
                EditorGUILayout.LabelField("No Global Blend Shapes Available");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                BlendShapesManager.matchListIndex = EditorGUILayout.Popup(BlendShapesManager.matchListIndex,
                    BlendShapesManager.matchListNames);
                if (GUILayout.Button("Add To List"))
                {
                    WardrobePrefabManager.AddToList("Deactivate",
                        BlendShapesManager.matchList[BlendShapesManager.matchListIndex],
                        blendShapeGroup);
                    SetAllDirty();
                }
                if (GUILayout.Button("Revert Back"))
                {
                    WardrobePrefabManager.AddToList("Revert Back",
                        BlendShapesManager.matchList[BlendShapesManager.matchListIndex], blendShapeGroup);
                    SetAllDirty();
                }
                EditorGUILayout.EndHorizontal();
            }

            // ON ACTIVATE LIST
            for (int i = 0; i < blendShapeGroup.onDeactivate.Count; i++)
            {
                BlendShapeItem item = blendShapeGroup.onDeactivate[i];
                WardrobePrefabManagerDisplayItem(blendShapeGroup, item, i, "Deactivate", WardrobePrefabManager);
            }

            EditorGUILayout.EndVertical();
        }

        private void SetAllDirty(){
            EditorUtility.SetDirty(this);
        }
        
        private void WardrobePrefabManagerDisplayItem(BlendShapeGroup group, BlendShapeItem item, int itemIndex, string type, WardrobePrefabManager wardrobePrefabManager)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = redColor;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                EditorGUILayout.EndHorizontal();

                if (type == "Activate")
                    group.onActivate.RemoveAt(itemIndex);
                if (type == "Deactivate")
                    group.onDeactivate.RemoveAt(itemIndex);

            }
            else
            {
                GUI.backgroundColor = Color.white;
                EditorGUILayout.LabelField(item.objectName + " " + item.triggerName);

                if (item.revertBack)
                {
                    EditorGUILayout.LabelField("This value will revert to pre-activation value");
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent($"{symbolInfo}", $"\"Explicit\" will set the shape to the value specified, while \"Less than\" will set the shape to ensure it is " +
                                                                               $"less than or equal to the value specified. \"Greater than\" will set it to make sure it is greater than or equal to the " +
                                                                               $"specified value.\n\nUse the \"Set\" button to set the value selected and visualize the outcome.\n\n" +
                                                                               $"[On Deactivate] Use \"Revert Back\" to make this shape revert back to it's previous value that was active when the " +
                                                                               $"Prefab Group was activated."), GUILayout.Width(25));
                    item.actionTypeIndex = EditorGUILayout.Popup(item.actionTypeIndex, WardrobePrefabManager.actionTypes);
                    item.actionType = WardrobePrefabManager.actionTypes[item.actionTypeIndex];
                    item.value = EditorGUILayout.Slider(item.value, item.min, item.max);
                    if (GUILayout.Button("Set"))
                        wardrobePrefabManager.TriggerBlendShape(item.triggerName, item.value);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        
        /*
         * This is where we add new objects to the Prefab Group, users can drag/drop or select an object to add it to the
         * list.
         */
        private void ShowSelectEquipmentObject(PrefabGroup group)
        {
            if (Manager.WardrobePrefabManager == null) return; // Don't show this if it doesn't have a Wardrobe Prefab Manager

            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();
            Label($"Select Equipment Object {symbolInfo}", 
                $"Any prefab with an Equipment Object component attached, which has this prefab selected as a " +
                $"Wardrobe Prefab Manager, will show up here, for easy selecting & adding.\n\nClick \"Reload Sources\" " +
                $"if objects aren't showing up as they should.", 200);

            Colors(Color.yellow);
            
            EndRow();
            //StartRow();
            var tempType = group.equipmentObjectTypeIndex;
            group.equipmentObjectTypeIndex = Popup(group.equipmentObjectTypeIndex, group.equipmentObjectTypes.ToArray(), 250);
            if (tempType != group.equipmentObjectTypeIndex)
            {
                // The type was changed, so we need to re-do the cache here.
                CacheEquipmentObjectsForGroup(group);
                CacheEquipmentObjectNamesForGroup(group);
                group.equipmentObjectIndex = 0;
            }

            group.equipmentObjectIndex = Popup(group.equipmentObjectIndex, group.equipmentObjectObjectNames.ToArray(), 250);

            StartRow();
            
            if (Button("Add", 125))
            {
                //Undo.RecordObject(Manager, "Undo Add");
                AddObjectFromList(group, group.equipmentObjectObjects[group.equipmentObjectIndex]);
            }
            if (Button("Add All", 125))
            {
                //Undo.RecordObject(Manager, "Undo Add All");
                AddAllObjectsFromList(group);
            }
            EndRow();
            
            if (Button("Add each to new group", 250))
            {
                //Undo.RecordObject(Manager, "Undo Add All");
                AddEachFromListToNewGroup(group);
            }
            
            EndVerticalBox();
            ResetColor();
        }

        private void AddEachFromListToNewGroup(PrefabGroup group)
        {
            foreach (var obj in group.equipmentObjectObjects)
            {
                // Create the new group
                PrefabGroup newGroup = Manager.CreateNewPrefabGroup(group.groupType);
                newGroup.name = obj.name;
                
                // Add the object to the new group
                AddObjectFromList(newGroup, obj, false);
            }
        }

        private void AddAllObjectsFromList(PrefabGroup group)
        {
            foreach (var obj in group.equipmentObjectObjects)
                AddObjectFromList(group, obj, false);
            
            // Activate the preset in the scene
            if (Manager.instantiatePrefabsAsAdded)
                //_activateGroup = group;
                Manager.ActivateGroup(group);
                
            DoCache(true); // Update the lists!
        }

        private void AddObjectFromList(PrefabGroup group, GameObject objectToAdd, bool doUpdateAndCache = true, bool noDuplicates = true)
        {
            if (noDuplicates && group.groupObjects.FirstOrDefault(x => x.objectToHandle == objectToAdd) != null)
            {
                Debug.LogWarning($"{objectToAdd.name} was a duplicate");
                return;
            }
            
            // Add the prefab to the list
            Manager.AddPrefabToGroup(group, objectToAdd);
                
            // Reduce the index value if needed
            group.equipmentObjectIndex = Mathf.Clamp(group.equipmentObjectIndex, 0, group.equipmentObjectObjects.Count - 1);

            if (!doUpdateAndCache) return;
            
            // Activate the preset in the scene
            if (Manager.instantiatePrefabsAsAdded)
                //_activateGroup = group;
                Manager.ActivateGroup(group);

            DoCache(true); // Update the lists!
        }

        /*
         * This is where we add new objects to the Prefab Group, users can drag/drop or select an object to add it to the
         * list.
         */
        private void ShowAddNewObjectField(PrefabGroup group)
        {
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            Label($"Add Prefab or Child Object to Group {symbolInfo}",
                $"Drag or select a Prefab from your project or a Game Object from the scene to add it to {group.name}. " +
                $"You can mix both types in each group. Prefabs will be instantiated and destroyed, and Game Objects will " +
                $"be turned on and off when this group is activated or deactivated.", 250);
            Colors(Color.yellow);
            group.newPrefab = Object(group.newPrefab, typeof(GameObject), 250, true) as GameObject;
            
            if (group.newPrefab)
            {
                if (group.newPrefab != null && group.newPrefab.transform.IsChildOf(Manager.transform))
                    Manager.AddGameObjectToGroup(group.newGameObject, group);
                else if (PrefabUtility.IsPartOfAnyPrefab(group.newPrefab))
                    Manager.AddPrefabToGroup(group);
                else if (group.newPrefab != null)
                    Debug.LogError("Error: " + group.newPrefab.name +
                                   " isn't a prefab that can be added, or isn't a child of the parent object.");

                group.newPrefab = null;

                if (Manager.instantiatePrefabsAsAdded)
                    //_activateGroup = group;
                    Manager.ActivateGroup(group);
                
                DoCache(true);
            }
            EndVerticalBox();
            ResetColor();
        }

        private void ShowGroupObject(PrefabGroup group, GroupObject groupObject)
        {
            StartRow();
            Label("", 30);

            ShowObjectDelete(group, groupObject);
            ShowObjectRender(group, groupObject);
            ShowObjectFields(group, groupObject);

            CheckOptionsAreSet(group, groupObject);

            EndRow();
        }

        private void ShowObjectRender(PrefabGroup group, GroupObject groupObject)
        {
            ColorsIf(groupObject.render, Color.grey, Color.black, Color.white, Color.grey);
            if (Button(symbolCheck, 25))
            {
                groupObject.render = !groupObject.render;
                Manager.ActivateGroup(group);
            }
            ResetColor();
        }

        private void ShowObjectFields(PrefabGroup group, GroupObject groupObject)
        {
            GameObject oldObject = groupObject.objectToHandle;
            groupObject.objectToHandle =
                Object(groupObject.objectToHandle, typeof(GameObject), 250, !groupObject.isPrefab) as GameObject;
            if (oldObject != groupObject.objectToHandle)
            {
                if (groupObject.isPrefab)
                {
                    if (!PrefabUtility.IsPartOfAnyPrefab(groupObject.objectToHandle))
                    {
                        groupObject.objectToHandle = oldObject;
                        Debug.LogError("Error: This isn't a prefab that can be added.");
                    }
                    else
                    {
                        Event e = Event.current;
                        if (e.shift)
                            UpdateAllObjects(oldObject, groupObject.objectToHandle);
                    }
                }
                else
                {
                    if (!groupObject.objectToHandle.transform.IsChildOf(Manager.transform))
                    {
                        groupObject.objectToHandle = oldObject;
                        Debug.LogError("Error: This isn't a GameObject that can be added.");
                    }
                    else
                    {
                        Event e = Event.current;
                        if (e.shift)
                            UpdateAllObjects(oldObject, groupObject.objectToHandle);
                    }
                }

                if (Manager.instantiatePrefabsAsAdded)
                {
                    //_deactivateGroup = group;
                    //_activateGroup = group;
                    Manager.DeactivateGroup(group);
                    Manager.ActivateGroup(group);
                }

            }

            if (groupObject.isPrefab)
            {
                Transform oldTransformObject = groupObject.parentTransform;
                groupObject.parentTransform = Object(groupObject.parentTransform, typeof(Transform), 200, true) as
                    Transform;
                if (oldTransformObject != groupObject.parentTransform)
                {

                    if (!groupObject.parentTransform.IsChildOf(Manager.thisTransform))
                    {
                        groupObject.parentTransform = oldTransformObject;
                        Debug.LogError("Error: Transform must be the parent transform or a child of " +
                                       Manager.thisTransform.name);
                    }
                    else
                    {
                        Event e = Event.current;
                        if (e.shift)
                            UpdateAllTransforms(groupObject.parentTransform);
                    }
                }
            }
        }

        private void ShowObjectDelete(PrefabGroup group, GroupObject groupObject)
        {
            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                RemoveObject(group, groupObject);
                DoCache(true);
                ExitGUI();
            }
            ResetColor();
        }

        private void ShowObjectsButton(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 80;
            if (header)
            { 
                Label("", fieldWidth + 3);
                return;
            }
            
            ColorsIf(group.showPrefabs, Color.green, Color.black, Color.white, Color.white);
            bool tempShowPrefabs = group.showPrefabs;
            if (Button($"Objects", fieldWidth))
            {
                group.showPrefabs = !group.showPrefabs;
                group.showShapes = !group.showPrefabs && group.showShapes;
            }

            // We are turning this group on, so do the cache here.
            if (!tempShowPrefabs && group.showPrefabs)
            {
                DoCache(true);
            }
            ResetColor();
        }
        
        private void ShowShapesButton(PrefabGroup group, bool header = false)
        {
            if (!BlendShapesManager || !WardrobePrefabManager) return;
            var fieldWidth = 80;
            
            if (header)
            { 
                Label("", fieldWidth + 3);
                return;
            }

            ColorsIf(group.showShapes, Color.green, Color.black, Color.white, Color.white);
            if (Button($"Shapes", fieldWidth))
            {
                group.showShapes = !group.showShapes;
                group.showPrefabs = !group.showShapes && group.showPrefabs;
            }
            ResetColor();
        }

        private void ShowGroupActivateDeactivate(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 160;
            if (header)
            { 
                Label("", fieldWidth);
                return;
            }

            var groupIsActive = Manager.GroupIsActive(group) == 2;
            BackgroundColor(groupIsActive ? Color.green : Color.black);
            if (group.isDefault && groupIsActive)
                ContentColor(Color.grey);
            if (Button($"Turn {(groupIsActive ? "off" : "on")}", 60))
            {
                if (groupIsActive)
                    Manager.DeactivateGroup(group);
                else
                    Manager.ActivateGroup(group);
            }
            ResetColor();
        }

        private void ShowRandomToggle(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 30;
            if (header)
            { 
                EditorGUILayout.LabelField(new GUIContent($"{symbolArrowCircleRight} {symbolInfo}", $"If off, this will not be included in the \"Random\" method."), GUILayout.Width(fieldWidth));
                return;
            }

            ColorsIf(group.canRandomize, Color.grey, Color.black, Color.white, Color.grey);
            if (String.IsNullOrWhiteSpace(group.groupType))
            {
                group.isDefault = false;
                ContentColor(Color.grey);
                EditorGUILayout.LabelField($"N/A", GUILayout.Width(fieldWidth));
                ResetColor();
                return;
            }
            
            group.canRandomize = ButtonToggle(group.canRandomize, $"{symbolArrowCircleRight}", fieldWidth);
        }

        private void ShowDefaultToggle(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 30;
            if (header)
            { 
                EditorGUILayout.LabelField(new GUIContent($"{symbolStarClosed} {symbolInfo}", $"Optional. Toggle one group to " +
                                                                                   $"be the default group. When a group of this " +
                                                                                   $"type is deactivated, the default group will " +
                                                                                   $"automatically be activated.\n\nThis option is " +
                                                                                   $"only available for groups with a \"Type\"."), GUILayout.Width(fieldWidth));
                return;
            }

            if (String.IsNullOrWhiteSpace(group.groupType))
            {
                group.isDefault = false;
                ContentColor(Color.grey);
                EditorGUILayout.LabelField($"N/A", GUILayout.Width(fieldWidth));
                ResetColor();
                return;
            }

            var cacheToggle = group.isDefault;

            ColorsIf(group.isDefault, Color.green, Color.black, Color.white, Color.grey);
            group.isDefault = ButtonToggle(group.isDefault, $"{symbolStarClosed}", fieldWidth);
            //group.isDefault = EditorGUILayout.Toggle(group.isDefault, GUILayout.Width(fieldWidth));

            // Set all the other ones to false if this is now true
            if (cacheToggle != group.isDefault && group.isDefault)
            {
                foreach (var groupOfType in GroupsOfType(group.groupType))
                {
                    if (groupOfType == group) continue;
                    groupOfType.isDefault = false;
                }
            }
        }

        private void ShowGroupType(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 100;
            if (header)
            { 
                EditorGUILayout.LabelField(new GUIContent($"Type {symbolInfo}", $"You can group Prefab Groups by type, making " +
                                                                   $"it easier to have only one active at a time, or simply for " +
                                                                   $"organization purposes."), GUILayout.Width(fieldWidth));
                return;
            }
            
            var cachedType = group.groupType;
            group.groupType = EditorGUILayout.DelayedTextField(group.groupType, GUILayout.Width(100));
            if (cachedType != group.groupType)
            {
                EnsureTypeNames();
                DoCache();
                EditorPrefs.SetBool($"Prefab Manager Show Type {group.groupType}", true);
            }
        }

        private void ShowGroupName(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 180;
            if (header)
            { 
                EditorGUILayout.LabelField(new GUIContent($"Group Name {symbolInfo}", $"The name of the group must " +
                                                                                      $"be unique, and can be used to activate and " +
                                                                                      $"deactivate the group at runtime."), GUILayout.Width(fieldWidth));
                return;
            }
            
            var cachedName = group.name;
            group.name = EditorGUILayout.DelayedTextField(group.name, GUILayout.Width(fieldWidth));
            if (cachedName != group.name)
            {
                if (String.IsNullOrEmpty(group.name))
                {
                    Debug.LogWarning("Error: Group names can not be empty.");
                    group.name = cachedName;
                }
                if (Manager.prefabGroups.Count(x => x.name == group.name) > 1)
                {
                    Debug.LogWarning("Error: Group names must be unique.");
                    group.name = cachedName;
                }
            }
        }

        private void ShowRemovePrefabGroup(PrefabGroup group, bool header = false)
        {
            var fieldWidth = 25;
            if (header)
            {
                Label("", fieldWidth);
                return;
            }
            
            BackgroundColor(Color.red);
            if (Button(symbolX, fieldWidth))
            {
                if (Dialog("Remove Group?", "Are you sure you want to do this?"))
                {
                    //foreach (var t in group.groupObjects)
                    //   RemoveObject(group, t);
                    //RemoveGroup(group);

                    Manager.RemovePrefabGroup(group);
                    DoCache();
                    ExitGUI();
                }
            }
            ResetColor();
        }

        private void RemoveGroup(PrefabGroup group) => Manager.prefabGroups.RemoveAll(x => x == group);

        private void SectionButton(string button, string prefs, int width = -1)
        {
            BackgroundColor(GetBool(prefs) ? Color.green : Color.black);
            if (Button(button))
                SetBool(prefs, !GetBool(prefs));
            ResetColor();
        }
        
        private void SetupAndOptions()
        {
            if (!EditorPrefs.GetBool("Prefab Manager Show Setup And Options")) return;
            
            EditorGUI.indentLevel++;
            EditorPrefs.SetBool("Prefab Manager Show Help Boxes", 
                EditorGUILayout.Toggle(new GUIContent($"Show Help Boxes {symbolInfo}", "Toggles help boxes in the Inspector"), 
                    EditorPrefs.GetBool("Prefab Manager Show Help Boxes")));
            EditorPrefs.SetBool("Prefab Manager Show Full Inspector", 
                EditorGUILayout.Toggle(new GUIContent($"Show Full Inspector {symbolInfo}", "If true, will show the full default inspector at the bottom" +
                                                                               "of the window. Use for debugging, not for editing data!"), 
                    EditorPrefs.GetBool("Prefab Manager Show Full Inspector")));
            Manager.instantiatePrefabsAsAdded =  
                EditorGUILayout.Toggle(new GUIContent($"Instantiate Prefabs when Added to Group {symbolInfo}", "If true, prefabs that are added to a group " +
                    "will be instantiated into the scene."), Manager.instantiatePrefabsAsAdded);
            Manager.onlyOneGroupActivePerType =  
                EditorGUILayout.Toggle(new GUIContent($"Only one group active per type {symbolInfo}", "If true, only one group per named \"type\" can be active " +
                          "at a time, and any active group will be deactivated when a new one is " +
                          "activated. This means you only have to call the \"Activate\" method, and " +
                          "the rest is handled for you."), Manager.onlyOneGroupActivePerType);
            Manager.unpackPrefabs = 
                EditorGUILayout.Toggle(new GUIContent($"Unpack Prefabs when Instantiated {symbolInfo}", "If true, prefabs that are instantiated will be unpacked."), 
                    Manager.unpackPrefabs);
            
            EditorGUILayout.Space();
            HelpBoxMessage("Use the option below to set all InGameObject values to null. This is useful " +
                                    "if you've copied the component values from another character, to clean it up.");
            if (GUILayout.Button("Make all \"In Game Objects\" null"))
            {
                RemoveInGameObjectLinks();
            }
            
            HelpBoxMessage("If you've copied another objects data or added the component from another object " +
                           "as new to this object, use this option to relink all the available objects to the new object.\n\n " +
                           "HINT: If you hold shift while you replace the transform in the list, all transforms will be updated to " +
                           "the new selection.");
            if (GUILayout.Button("Relink objects to this parent object"))
            {
                RelinkObjects();
            }
            
            EditorGUI.indentLevel--;
            EditorUtility.SetDirty(this);
        }

        private void HelpBoxMessage(string message, MessageType messageType = MessageType.None)
        {
            if (!EditorPrefs.GetBool("Prefab Manager Show Help Boxes")) return;
            EditorGUILayout.HelpBox(message,messageType);
        }

        private void SetDefaultEditorPrefs()
        {
            DefaultEditorBool("Prefab Manager Show Help Boxes", true);
            DefaultEditorBool("Prefab Manager Show Full Inspector", false);
            DefaultEditorBool("Prefab Manager Instantiate When Added", true);
            DefaultEditorBool("Prefab Manager One Active Group Per Type", true);
            DefaultEditorBool("Prefab Manager Unpack Prefabs", true);
        }

        private void RemoveInGameObjectLinks()
        {
            foreach (PrefabGroup group in Manager.prefabGroups)
            {
                foreach (GroupObject obj in group.groupObjects)
                {
                    obj.inGameObject = null;
                }
            }
        }

        private void UpdateAllObjects(GameObject oldObject, GameObject newObject)
        {
            foreach (PrefabGroup group in Manager.prefabGroups)
            {
                foreach (GroupObject obj in group.groupObjects)
                {
                    if (obj.objectToHandle == oldObject)
                        obj.objectToHandle = newObject;
                }
            }
        }

        private void UpdateAllTransforms(Transform transform)
        {
            foreach (PrefabGroup group in Manager.prefabGroups)
            {
                foreach (GroupObject obj in group.groupObjects)
                {
                    obj.parentTransform = transform;
                }
            }
        }
        
        private void RemoveObject(PrefabGroup prefabGroup, GroupObject groupObject)
        {
            GameObject inGameObject = groupObject.inGameObject;
            if (groupObject.isPrefab && inGameObject)
                Manager.DestroyObject(inGameObject);
            else if (!groupObject.isPrefab && inGameObject)
                inGameObject.SetActive(false);

            prefabGroup.RemoveGroupObject(groupObject);
        }

        private void CheckOptionsAreSet(int g, int i) => CheckOptionsAreSet(Manager.prefabGroups[g], Manager.prefabGroups[g].groupObjects[i]);

        private void CheckOptionsAreSet(PrefabGroup group, GroupObject groupObject)
        {
            if (groupObject.parentTransform == null)
                groupObject.parentTransform = Manager.thisTransform;
        }
        
        private void RelinkObjects()
        {
            Debug.Log("Begin Relink");
            for (int g = 0; g < Manager.prefabGroups.Count; g++)
            {
                PrefabGroup prefabGroup = Manager.prefabGroups[g];
                
                for (int i = 0; i < prefabGroup.groupObjects.Count; i++)
                {
                    GroupObject groupObject = prefabGroup.groupObjects[i];

                    // If this is a prefab, do a different operation
                    if (groupObject.isPrefab)
                    {
                        Debug.Log($"Prefab relink from {groupObject.parentTransform}");
                        GameObject foundObject = FindGameObject(groupObject.parentTransform.name);
                        if (foundObject == null) continue;
                        
                        Debug.Log("Found the object " + foundObject.name);
                        groupObject.parentTransform = foundObject.transform;
                        continue;
                    }
                    
                    if (groupObject.objectToHandle.transform.IsChildOf(groupObject.parentTransform))
                    {
                        var prefabName = groupObject.objectToHandle.name;
                        Debug.Log($"In-Game object relink for {prefabName}");
                        
                        GameObject foundObject = FindGameObject(prefabName);
                        if (foundObject == null) continue;
                        
                        Debug.Log("Found the object " + foundObject.name);
                        
                        groupObject.parentTransform = Manager.gameObject.transform;
                    }
                }
            }
        }
        
        private GameObject FindGameObject(string lookupName)
        {
            if (Manager.gameObject.name == lookupName)
                return Manager.gameObject;
            
            Transform[] gameObjects = Manager.gameObject.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform child in gameObjects)
            {
                if (child.name == lookupName)
                    return child.gameObject;
            }

            Debug.Log($"Warning: Did not find a child named {lookupName}! This re-link will be skipped.");
            return null;
        }
    }
}
