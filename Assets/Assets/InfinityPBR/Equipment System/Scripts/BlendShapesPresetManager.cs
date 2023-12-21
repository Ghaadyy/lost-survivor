using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR
{
    [RequireComponent(typeof(BlendShapesManager))]
    [System.Serializable]
    public class BlendShapesPresetManager : MonoBehaviour
    {
        public List<BlendShapePreset> presets = new List<BlendShapePreset>();
        [HideInInspector] public string[] onTriggerMode = new[] {"Explicit", "Random"};
        
        [HideInInspector] public List<Shape> shapeList = new List<Shape>();
        [HideInInspector] public string[] shapeListNames;
        [HideInInspector] public int shapeListIndex = 0;
        
        [HideInInspector] public bool showHelpBoxes = true;
        [HideInInspector] public bool showFullInspector = false;
        [HideInInspector] public bool showSetup = true;
        [HideInInspector] public bool lerp = true;
        [HideInInspector] public float lerpSeconds = 2;

        private float _lerpValue;
        private float _lerpTime;
        private bool _isLerping;
        

        public BlendShapesManager BlendShapesManager => GetBlendShapesManager();
        private BlendShapesManager _blendShapesManager;
        private BlendShapesManager GetBlendShapesManager()
        {
            if (_blendShapesManager != null) return _blendShapesManager;
            if (TryGetComponent(out BlendShapesManager foundManager))
                _blendShapesManager = foundManager;
            return _blendShapesManager;
        }

        public void StartTransitionToPreset(BlendShapePreset preset)
        {
            Debug.Log($"Start Transition to Preset {preset.name}");
            SetToAndFromValues(preset); // save the current value and the "to" value on this preset
            _lerpValue = 0f; // reset this
            _lerpTime = 0f; // reset this
            _isLerping = true; // set this on
            StartCoroutine(nameof(LerpToPreset)); // Start the coroutine
        }

        public void StartTransitionToPreset(int presetIndex)
        {
            if (presetIndex < 0 || presets.Count <= presetIndex)
            {
                Debug.LogError($"Index {presetIndex} is out of range");
                return;
            }   
            
            StartTransitionToPreset(presets[presetIndex]);
        }

        public void StartTransitionToPreset(string presetName)
        {
            var foundPreset = presets.FirstOrDefault(x => x.name == presetName);
            if (foundPreset == null)
            {
                Debug.LogError($"No preset found named {presetName}");
                return;
            }

            StartTransitionToPreset(foundPreset);
        }

        IEnumerator LerpToPreset()
        {
            while (_isLerping)
            {
                _lerpTime += Time.deltaTime / lerpSeconds;
                _lerpValue = Mathf.Lerp(0, 1, _lerpTime);
                BlendShapesManager.LerpToValue(_lerpValue);

                if (_lerpValue >= 1)
                    break;

                yield return null;
            }
            
        }

        public void ValueBetweenTwoPresets(float value, BlendShapePreset from, BlendShapePreset to)
        {
            
        }
        
        /// <summary>
        /// Activates an individual preset
        /// </summary>
        /// <param name="index"></param>
        public void ActivatePreset(int index)
        {
            for (int v = 0; v < presets[index].presetValues.Count; v++)
            {
                BlendShapePresetValue presetValue = presets[index].presetValues[v];
                BlendShapeGameObject obj = BlendShapesManager.GetBlendShapeObject(presetValue.objectName);
                BlendShapeValue value = BlendShapesManager.GetBlendShapeValue(obj, presetValue.valueTriggerName);

                value.value = presetValue.onTriggerMode == "Explicit"
                    ? presetValue.shapeValue
                    : Random.Range(presetValue.limitMin, presetValue.limitMax);
                BlendShapesManager.TriggerShape(obj,value);
            }
        }

        public void SetToAndFromValues(BlendShapePreset preset)
        {
            BlendShapesManager.SetToAndFromValues(preset);
        }

        /// <summary>
        /// Activates an individual named preset
        /// </summary>
        /// <param name="name"></param>
        public void ActivatePreset(string name)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].name != name)
                    continue;
                
                ActivatePreset(i);
                return;
            }
        }
    }

    [System.Serializable]
    public class BlendShapePreset
    {
        public string name;
        public List<BlendShapePresetValue> presetValues = new List<BlendShapePresetValue>();
        [HideInInspector] public bool showValues = false;
    }

    [System.Serializable]
    public class BlendShapePresetValue
    {
        public string objectName;
        public string valueTriggerName;
        public string onTriggerMode;
        [HideInInspector] public int onTriggerModeIndex = 0;
        public float shapeValue;

        public float limitMin;
        public float limitMax;
        public float min;
        public float max;
    }

    [System.Serializable]
    public class Shape
    {
        public BlendShapeGameObject obj;
        public BlendShapeValue value;
    }

}