using System;
using System.Collections.Generic;
using UnityEngine;

namespace XPToolchains.NodeGraph
{
    [Serializable]
    public class ExposedParameterWorkaround : ScriptableObject
    {
        [SerializeReference]
        public List<ExposedParameter> parameters = new List<ExposedParameter>();
        public BaseGraph graph;
    }
}
