using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * EquipmentObject
 *
 * Attach this object to any equipment -- wardrobe etc -- that will be equipped onto another object or character,
 * either at run time or edit time.
 *
 * Provided by Infinity PBR - www.InfinityPBR.com
 *
 * Scripting Docs and Tutorials: https://infinitypbr.gitbook.io/infinity-pbr/equipment-systems/prefab-and-object-manager/equipmentobject.cs
 */

namespace InfinityPBR
{
    [System.Serializable]
    public class EquipmentObject : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer; // SkinnedMeshRenderer for this object
        public Transform boneRoot; // BoneRoot -- Must match the parent this object is equipped on!!!

        public List<GameObject> wardrobePrefabManagers = new List<GameObject>();

        public bool HasObjectNamed(string nameToCheck)
        {
            return wardrobePrefabManagers.FirstOrDefault(x => x.name == nameToCheck) != null;
        }

        // This script will remove itself from the object when it is equipped, at run time or edit time.
        public void SelfDestruct()
        {
#if UNITY_EDITOR
            DestroyImmediate(this);
#else
            Destroy(this);
#endif
        }
        

        // If this component is not removed, this warning will be provided. It is possible to use this prefab as an 
        // object in the game of course. If that is the case, best practice would be to create a new prefab variant, or
        // a new prefab object from this one, rather than using this specific prefab object.
        public void Start()
        {
            Debug.LogWarning($"Uh oh! The EquipmentObject component implies that {gameObject.name} is meant to " +
                             $"be equipped onto a character. That process should have deleted this script. Did you forget " +
                             $"to run the \"Equip Object\" option in Window/Infinity PBR/Equip Object?");
        }
        
        // Used in the editor script, this will remove a wardrobe prefab manager from the list
        public void RemoveWardrobePrefabManager(GameObject managerGameObject) => wardrobePrefabManagers.RemoveAll(x => x == managerGameObject);

        // Used in the editor script, this will add a wardrobe prefab manager, and order alphabetically
        public void AddWardrobePrefabManager(GameObject managerGameObject)
        {
            Debug.Log($"Should add {managerGameObject.name} type of {managerGameObject.GetType()}");
            if (wardrobePrefabManagers.Contains(managerGameObject)) return;
            wardrobePrefabManagers.Add(managerGameObject);
            wardrobePrefabManagers = wardrobePrefabManagers
                .OrderBy(x => x.name)
                .ToList();
        }
        
        public void PopulateRootBone(string rootBoneName)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.name != rootBoneName) continue;
                boneRoot = child;
                return;
            }
        }

        public void PopulateSkinnedMeshRenderer()
        {
            foreach (Transform child in gameObject.transform)
            {
                if (!child.TryGetComponent(out SkinnedMeshRenderer smr)) continue;
                skinnedMeshRenderer = smr;
                return;
            }
        }
    }
}

