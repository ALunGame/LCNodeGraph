using System.Collections.Generic;

namespace XPToolchains.NodeGraph
{
    [System.Serializable]
    public class CopyPasteHelper
    {
        public List<JsonElement> copiedNodes = new List<JsonElement>();

        public List<JsonElement> copiedGroups = new List<JsonElement>();

        public List<JsonElement> copiedEdges = new List<JsonElement>();
    }
}
