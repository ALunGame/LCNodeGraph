using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XPToolchains.Help;

namespace XPToolchains.NodeGraph
{
    public abstract class BaseGraphWindow : EditorWindow
    {
        readonly string graphWindowStyle = "BaseGraphView";

        public VisualElement rootView;
        protected BaseGraphView graphView;
        public GraphListView graphListView;
        public ToolbarView toolbarView;
        public RollbackView rollbackView;

        public bool showGraphList = false;
        public bool showRollback = false;

        //当前视图数据
        public BaseGraph selGraph;
        public Dictionary<string, BaseGraph> graphDict = new Dictionary<string, BaseGraph>();
        //当前目录下所有的视图
        public string selGraphPath = "";

        //保存读取路径
        public abstract string BackVerPath { get; }
        //版本回退路径
        public abstract string SavePath { get; }

        //节点代码的命名空间
        public List<string> nodeNameSpaces;

        public Action updateAction;

        public bool isGraphLoaded
        {
            get { return graphView != null && graphView.graph != null; }
        }

        void InitRootView()
        {
            rootView = base.rootVisualElement;

            rootView.name = "graphRootView";

            rootView.styleSheets.Add(NodeGraphDefine.LoadUSS(graphWindowStyle));
        }

        void InitListView()
        {
            graphListView = new GraphListView();
            //查找所有文件
            List<string> graphFilePathList = new List<string>();
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
            graphFilePathList.AddRange(Directory.GetFiles(SavePath, "*" + NodeGraphDefine.GraphAssetExNam, SearchOption.AllDirectories));

            //重置
            graphDict.Clear();
            selGraph = null;
            selGraphPath = "";
            graphView = null;

            TaskHelp.AddTaskOneParam<List<string>, Dictionary<string, BaseGraph>>(graphFilePathList, RollbackHelper.GetGraphDict, (ver) =>
              {
                  AddOtherViews();
                  rollbackView.visible = false;
                  graphDict = ver;
                  graphListView.Init(this);
                  if (graphDict != null && graphDict.Count > 0)
                      ChangeGraph(graphDict.Keys.ToList()[0]);
                  RefreshOtherView();
              });
            //graphDict = RollbackHelper.GetGraphDict(graphFilePathList);
            //graphListView.Init(this);
        }

        void InitToolbarView()
        {
            toolbarView = new ToolbarView();
        }

        void InitRollbackView()
        {
            rollbackView = new RollbackView(this);
        }

        public void InitGraph(List<string> nodeNameSpaces = null)
        {
            this.nodeNameSpaces = nodeNameSpaces;
            rootView.Clear();
            InitToolbarView();
            InitRollbackView();
            InitListView();
        }

        public void ChangeGraph(string graphFilePath)
        {
            if (string.IsNullOrEmpty(graphFilePath) || !graphDict.ContainsKey(graphFilePath))
                return;
            if (graphFilePath==selGraphPath)
                return;

            if (selGraph != null)
                SaveGraph();

            BaseGraph newGraph = graphDict[graphFilePath];
            if (newGraph == null)
                return;
            newGraph.Init(GetNodeId);
            selGraph = newGraph;

            //创建视图显示
            if (graphView != null)
            {
                rootView.Remove(graphView);
            }
            if (graphView == null)
                graphView = new BaseGraphView(this);
            rootView.Add(graphView);

            graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;
            if (graphView == null)
            {
                Debug.LogError("打开界面出错，没有 BaseGraphView");
                return;
            }
            graphView.Init(selGraph);
            graphView.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.S && e.actionKey)
                    SaveGraph();
            });
            graphView.onGraphUpdate += SaveGraph;
            showGraphList = true;
            selGraphPath = graphFilePath;

            //其他界面
            RemoveOtherViews();
            AddOtherViews();
            RefreshOtherView();
            rollbackView.visible = false;
            showRollback = false;
        }

        private void RemoveOtherViews()
        {
            rootView.Remove(toolbarView);
            rootView.Remove(graphListView);
            rootView.Remove(rollbackView);
        }

        private void AddOtherViews()
        {
            rootView.Add(toolbarView);
            rootView.Add(graphListView);
            rootView.Add(rollbackView);
        }

        public void RefreshOtherView()
        {
            toolbarView.Init(this, graphView);
            graphListView.Init(this);
            rollbackView.Init(this);
        }

        #region 子类重写

        protected virtual void OnEnable()
        {
            InitRootView();
        }

        protected virtual void Update()
        {
            updateAction?.Invoke();
        }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy()
        {
            graphView?.Dispose();
            updateAction = null;
            if (EditorUtility.DisplayDialog("保存", "是否保存当前视图", "确认", "取消"))
            {
                SaveGraph();
            }
        }

        public virtual void OnGraphDeleted()
        {
            if (selGraph != null)
                rootView.Remove(graphView);

            graphView = null;
        }

        protected abstract void GetNodeId(BaseGraph graph, BaseNode node);

        public abstract void SerializeGraph(Dictionary<string, BaseGraph> graphDict);

        #endregion

        #region 数据操作

        public void RollBack(Dictionary<string, BaseGraph> verDict)
        {
            graphDict = verDict;
            ChangeGraph(selGraphPath);
        }

        public void DelGraph()
        {
            if (string.IsNullOrEmpty(selGraphPath))
                return;
            graphDict.Remove(selGraphPath);
            File.Delete(selGraphPath);
            if (graphDict.Count > 0)
            {
                ChangeGraph(graphDict.Keys.ToList()[0]);
            }
            else
            {
                rootView.Remove(graphView);
            }
            RefreshOtherView();
        }

        public void AddGraph()
        {
            EDPopPanel.PopWindow("输入视图名:", (string newName) =>
            {
                string savePath = GetGraphSavePath(newName);
                if (graphDict.ContainsKey(savePath))
                {
                    Debug.LogError($"视图名重复>>>>{newName}");
                    return;
                }

                BaseGraph newGraph = new BaseGraph(newName);
                newGraph.Init(GetNodeId);
                selGraph = newGraph;
                SaveGraph();
                graphDict.Add(savePath, newGraph);

                ChangeGraph(savePath);
                RefreshOtherView();
            });
        }

        public string GetGraphSavePath(string graphName)
        {
            return string.Format("{0}/{1}", SavePath, graphName + NodeGraphDefine.GraphAssetExNam);
        }

        public void SaveGraph()
        {
            NodeGraphToJson.ToJson(selGraph, GetGraphSavePath(selGraph.displayName));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
