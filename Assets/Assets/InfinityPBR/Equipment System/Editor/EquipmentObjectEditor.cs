using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using static InfinityPBR.EquipmentSystemStatic;

namespace InfinityPBR
{
    [CustomEditor(typeof(EquipmentObject))]
    [CanEditMultipleObjects]
    [Serializable]
    public class EquipmentObjectEditor : InfinityEditor<EquipmentObject>
    {
        private int _wardrobePrefabManagerIndex = 0;
        private List<GameObject> _wardrobePrefabManagers = new List<GameObject>();
        private List<string> _wardrobePrefabManagerNames = new List<string>();
        
        private EquipmentObject EquipmentObject => GetEquipmentObject();
        private EquipmentObject _equipmentObject;
        private EquipmentObject GetEquipmentObject()
        {
            if (_equipmentObject != null) return _equipmentObject;
            _equipmentObject = (EquipmentObject) target;
            return _equipmentObject;
        }
        
        void OnEnable()
        {
            DoCache();
            AutoPopulate();
            SaveType();
        }

        private void SaveType()
        {
            
        }

        private void DoCache()
        {
            CacheWardrobePrefabManagers();
            CacheWardrobePrefabManagerNames();
        }

        private void CacheWardrobePrefabManagerNames() 
            => _wardrobePrefabManagerNames = _wardrobePrefabManagers
            .Select(x => x.name)
            .ToList();
        
        private void CacheWardrobePrefabManagers() {
            _wardrobePrefabManagers = EquipmentSystemStatic.WardrobePrefabManagers
                .Where(x => !EquipmentObject.wardrobePrefabManagers.Contains(x))
                .ToList();
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            LinkToDocs("https://infinitypbr.gitbook.io/infinity-pbr/equipment-systems/prefab-and-object-manager/equipmentobject.cs");

            ShowHelpBox();
            Space();
            
            ShowRequiredFields();
            Space();
            ShowOptionalFields();

            Space();
            DrawDefaultInspector("Equipment Object Draw Default Inspector");
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(EquipmentObject);
        }

        private void ShowOptionalFields()
        {
            LabelBig("Optional", 14, true);

            BackgroundColor(Color.magenta);
            StartVerticalBox();
            MessageBox(
                $"Use this button to cache all Wardrobe Prefab Manager objects. The process may take a short while, as it will scan your entire project. " +
                $"You should only need to use this once per session, or if scripts are modified and reloaded, as the cache will be removed. There are " +
                $"currently {EquipmentSystemStatic.WardrobePrefabManagers.Count} cached objects.");
            if (Button("Cache Wardrobe Prefab Managers"))
            {
                EquipmentSystemStatic.CacheWardrobePrefabManagers();
                DoCache();
                Debug.Log($"<color=#00ff00>Success!</color> Cached {EquipmentSystemStatic.WardrobePrefabManagers.Count} prefabs with Wardrobe Prefab Manager components!");
                ExitGUI();
            }
            EndVerticalBox();
            ResetColor();
            Space();
            
            StartVerticalBox();
            StartRow();
            StartVertical();
            Label($"Wardrobe Prefab Managers ⓘ", "Add the objects which have a Wardrobe Prefab component on them, which this Equipment Object is intended " +
                                         "to work directly with. Generally this is due to a matched Bone structure.", 200);

            AddWardrobePrefabManagers();
            
            EndVertical();
            
            StartVertical();
            ListWardrobePrefabManagers();
            
            EndVertical();
            
            EndRow();
            EndVerticalBox();
        }

        private void AddWardrobePrefabManagers()
        {
            if (_wardrobePrefabManagerNames.Count == 0 && EquipmentObject.wardrobePrefabManagers.Count == 0)
            {
                MessageBox($"There are no prefabs with the Wardrobe Prefab Manager. Please add those to your " +
                           $"prefabs, or click the \"Cache Wardrobe Prefab Managers\" button above to find the objects.", MessageType.Warning);
                return;
            }

            if (EquipmentSystemStatic.WardrobePrefabManagers.Count == 0 &&
                EquipmentObject.wardrobePrefabManagers.Count > 0)
            {
                MessageBox($"There are no cached Wardrobe Prefab Managers, yet your list is populated. Please cache " +
                           $"the objects using the button above to make changes.", MessageType.Warning);
                return;
            }

            if (_wardrobePrefabManagerNames.Count == 0)
            {
                LabelGrey("All objects are already in the list");
                return;
            }
            
            Colors(Color.yellow);
            _wardrobePrefabManagerIndex = Popup(_wardrobePrefabManagerIndex, _wardrobePrefabManagerNames.ToArray(), 200);
            if (Button("Add", 200))
                AddWardrobePrefabManagerToAllTargets(_wardrobePrefabManagers[_wardrobePrefabManagerIndex]);
            
            ResetColor();
        }

        private void ListWardrobePrefabManagers()
        {
            if (EquipmentObject.wardrobePrefabManagers.Count == 0)
            {
                MessageBox($"There are no Wardrobe Prefab Managers in the list. Adding Wardrobe Prefab Managers to " +
                           $"this list will make it easier when populating Wardrobe Groups on those objects. You can " +
                           $"select multiple objects at once when managing this list.");
                return;
            }
            
            foreach(GameObject managerObject in EquipmentObject.wardrobePrefabManagers)
            {
                StartRow();
                BackgroundColor(Color.red);
                if (Button($"{symbolX}", 25))
                {
                    RemoveWardrobePrefabManagerFromAllTargets(managerObject);
                    ExitGUI();
                }
                ResetColor();
                if (managerObject == null)
                {
                    Debug.LogWarning($"The Wardrobe Prefab Manager was not found or there was a type mismatch.\n\n" +
                                     $"Expecting type GameObject, got type {managerObject.GetType()}.\n\n" +
                                     $"This should not happen. Will " +
                                     $"remove null entries from list now.");
                    RemoveWardrobePrefabManagerFromAllTargets(null);
                    EndRow();
                    ExitGUI();
                }
                Object(managerObject.gameObject, typeof(GameObject), 200, false);
                EndRow();
            }
        }

        private void RemoveWardrobePrefabManagerFromAllTargets(GameObject managerObject)
        {
            foreach(var obj in targets)
            {
                var targetObj = (EquipmentObject) obj;
                if (targetObj == null) return;
                
                targetObj.RemoveWardrobePrefabManager(managerObject);
            }

            DoCache();
            SetAllTargetsDirty();
            
        }

        private void AddWardrobePrefabManagerToAllTargets(GameObject managerObject)
        {
            foreach(var obj in targets)
            {
                var targetObj = (EquipmentObject) obj;
                if (targetObj == null) return;
                    
                targetObj.AddWardrobePrefabManager(managerObject);
            }

            DoCache();
            SetAllTargetsDirty();
        }

        private void SetAllTargetsDirty()
        {
            foreach (var obj in targets)
                EditorUtility.SetDirty(obj);
        }

        /*
         * Shows the two fields which need to be populated, with tooltips so fancy.
         */
        private void ShowRequiredFields()
        {
            LabelBig("Required", 14, true);
            ShowPopulateHint(EquipmentObject);

            StartRow();
            EditorGUILayout.LabelField(new GUIContent($"Skinned Mesh Renderer ⓘ", "This is the Skinned Mesh " +
                                                                                  "Renderer of this object, and is generally " +
                                                                                  "a child of the main parent object."), GUILayout.Width(150));
            EquipmentObject.skinnedMeshRenderer = EditorGUILayout.ObjectField(EquipmentObject.skinnedMeshRenderer,
                typeof(SkinnedMeshRenderer), true, GUILayout.Width(200)) as SkinnedMeshRenderer;
            EndRow();

            StartRow();
            EditorGUILayout.LabelField(new GUIContent($"Bone Root ⓘ", "This is the root object for the bone structure. " +
                                                                      "Remember the root bone structure must match the parent that " +
                                                                      "this is being attached to!"), GUILayout.Width(150));
            EquipmentObject.boneRoot = EditorGUILayout.ObjectField(EquipmentObject.boneRoot, 
                typeof(Transform), true, GUILayout.Width(200)) as Transform;
            EndRow();
        }

        /*
         * Shows a box if either of the two are NOT populated, so helpful.
         */
        private void ShowPopulateHint(EquipmentObject equipmentObject)
        {
            // March 19, 2022 -- When this is under a prefab, the set dirty doesn't work!
            //if (equipmentObject.skinnedMeshRenderer != null && equipmentObject.boneRoot != null) return;

            CheckRootBoneName();
            
            MessageBox($"Try opening the prefab to populate them. Note: " +
                                    $"Unity bug, I think, when adding this script to a child of a prefab (in the \"Prefab\" management " +
                                    $"view), turn off \"Auto Save\", and manually save after adding the script, or the settings won't " +
                                    $"actually be saved!");
            StartRow();
            Label($"Rootbone Name ⓘ", "Set the name of the root bone used for your character(s). The " +
                                                                          "\"Populate\" button will attempt to set the two required values.", 120);
            SetString("Infinity PBR Equipment Object Root Bone", TextField(EditorPrefs.GetString("Infinity PBR Equipment Object Root Bone"), 100));
            Colors(Color.yellow);
            if (Button("Populate", 100))
            {
                AutoPopulate();
            }
            EndRow();
            ResetColor();
        }

        private void CheckRootBoneName()
        {
            if (EditorPrefs.HasKey("Infinity PBR Equipment Object Root Bone") 
                && !String.IsNullOrWhiteSpace(EditorPrefs.GetString("Infinity PBR Equipment Object Root Bone"))) return;

            EditorPrefs.SetString("Infinity PBR Equipment Object Root Bone", "BoneRoot");
        }

        private void AutoPopulate()
        {
            PopulateRootBone();
            PopulateSkinnedMeshRenderer();
        }

        private void PopulateSkinnedMeshRenderer()
        {
            EquipmentObject.PopulateSkinnedMeshRenderer();
            /*
              foreach (Transform child in EquipmentObject.gameObject.transform)
            {
                if (!child.TryGetComponent(out SkinnedMeshRenderer smr)) continue;
                EquipmentObject.skinnedMeshRenderer = smr;
                return;
            }
             */
        }

        private void PopulateRootBone()
        {
            EquipmentObject.PopulateRootBone(EditorPrefs.GetString("Infinity PBR Equipment Object Root Bone"));
            /*
            foreach (Transform child in EquipmentObject.gameObject.transform)
            {
                if (child.gameObject.name != EditorPrefs.GetString("Infinity PBR Equipment Object Root Bone")) continue;
                EquipmentObject.boneRoot = child;
                return;
            }
            */
        }

        /*
         * Main helpbox at the top explaining what this does and the two ways to utilize it.
         */
        private void ShowHelpBox()
        {
            MessageBox($"EQUIPMENT OBJECT\n" +
                        "This script implies that this object can be equipped onto another object, often a " +
                        "character -- any object with a SkinnedMeshRenderer. During the equip process, the" +
                        "object will be linked to the bones of the parent object, so that it can move with the " +
                        "parent object animations.\n\n" +
                        "1. [Edit time only] Add this object as a child of the target object (which will be " +
                        "\"equipping\" this), and run the menu option Window/Infinity PBR/Equip Objects. This can " +
                        "be done with any number of Equipment Objects at once.\n\n" +
                        "2. [Runtime or Edit time] Use the \"Prefab Child Manager\" script to manage your equipment " +
                        "objects. This script, when attached to the parent, makes it much easier to manage large " +
                        "groups of equipment, including objects like this one, objects which do not have a " +
                        "SkinnedMeshRenderer (i.e. weapons etc), and objects that are not prefabs at all, but are " +
                        "already attached as a child of the parent object. This is the preferred method to use " +
                        "at runtime for customization and randomization.");
        }
    }
}
