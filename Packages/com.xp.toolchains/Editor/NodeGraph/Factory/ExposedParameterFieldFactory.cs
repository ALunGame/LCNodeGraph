using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    //网上看到的，为了解决Unity序列化绑定属性很慢的问题
    //这边都不太懂，要重新看
    public class ExposedParameterFieldFactory : IDisposable
    {
        BaseGraph graph;
        [SerializeField]
        ExposedParameterWorkaround exposedParameterObject;
        SerializedObject serializedObject;
        SerializedProperty serializedParameters;

        Dictionary<ExposedParameter, object> oldParameterValues = new Dictionary<ExposedParameter, object>();
        Dictionary<ExposedParameter, ExposedParameter.Settings> oldParameterSettings = new Dictionary<ExposedParameter, ExposedParameter.Settings>();

        public ExposedParameterFieldFactory(BaseGraph graph, List<ExposedParameter> customParameters = null)
        {
            this.graph = graph;

            exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
            exposedParameterObject.graph = graph;
            exposedParameterObject.hideFlags = HideFlags.HideAndDontSave;
            serializedObject = new SerializedObject(exposedParameterObject);
            UpdateSerializedProperties(customParameters);
        }

        public void UpdateSerializedProperties(List<ExposedParameter> parameters = null)
        {
            if (parameters != null)
                exposedParameterObject.parameters = parameters;
            else
                exposedParameterObject.parameters = graph.exposedParameters;
            serializedObject.Update();
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
        }

        public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
            field.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(field);

            oldParameterValues[parameter] = parameter.value;
            view.Add(new IMGUIContainer(() =>
            {
                if (oldParameterValues.TryGetValue(parameter, out var value))
                {
                    if (parameter.value != null && !parameter.value.Equals(value))
                        valueChangedCallback(parameter.value);
                }
                oldParameterValues[parameter] = parameter.value;
            }));
            return view;
        }

        public VisualElement GetParameterSettingsField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var serializedParameter = serializedParameters.GetArrayElementAtIndex(propIndex);
            serializedParameter.managedReferenceValue = exposedParameterObject.parameters[propIndex];
            var serializedSettings = serializedParameter.FindPropertyRelative(nameof(ExposedParameter.settings));
            serializedSettings.managedReferenceValue = exposedParameterObject.parameters[propIndex].settings;
            var settingsField = new PropertyField(serializedSettings);
            settingsField.Bind(serializedObject);

            VisualElement view = new VisualElement();
            view.Add(settingsField);

            // TODO: see if we can replace this with an event
            oldParameterSettings[parameter] = parameter.settings;
            view.Add(new IMGUIContainer(() =>
            {
                if (oldParameterSettings.TryGetValue(parameter, out var settings))
                {
                    if (!settings.Equals(parameter.settings))
                        valueChangedCallback(parameter.settings);
                }
                oldParameterSettings[parameter] = parameter.settings;
            }));

            return view;
        }

        public void ResetOldParameter(ExposedParameter parameter)
        {
            oldParameterValues.Remove(parameter);
            oldParameterSettings.Remove(parameter);
        }

        int FindPropertyIndex(ExposedParameter param) => exposedParameterObject.parameters.FindIndex(p => p == param);

        public void Dispose()
        {
            GameObject.DestroyImmediate(exposedParameterObject);
        }
    }
}
