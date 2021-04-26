﻿using UnityEngine.UIElements;

namespace XPToolchains.NodeGraph
{
    public class ExposedParameterPropertyView : VisualElement
    {
        protected BaseGraphView baseGraphView;

        public ExposedParameter parameter { get; private set; }

        public Toggle hideInInspector { get; private set; }

        public ExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param)
        {
            baseGraphView = graphView;
            parameter = param;

            var field = graphView.exposedParameterFactory.GetParameterSettingsField(param, (newValue) =>
            {
                param.settings = newValue as ExposedParameter.Settings;
            });

            Add(field);
        }
    }
}
