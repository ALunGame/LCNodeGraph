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
    //版本回退界面
    public class RollbackView : VisualElement
    {
        static readonly string pinnedElementTree = "RollbackElement";
        static readonly string pinnedElementStyle = "RollBack";

        private List<string> rollbackVerList = new List<string>();
        private ScrollView scrollView;

        private double fileSaveTime = 60;
        private double fileSaveTimer = 0;
        private float totalSaveCnt = 15;
        private BaseGraphWindow graphWindow;

        public RollbackView(BaseGraphWindow graphWindow)
        {
            this.graphWindow = graphWindow;
            graphWindow.updateAction += OnUpdate;
            CreateLayout();

            fileSaveTimer = GetNowTotalSeconds() - fileSaveTime;
        }

        private double GetNowTotalSeconds()
        {
            TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
            return timeSpan.TotalSeconds;
        }

        bool isSave = false;
        private void OnUpdate()
        {
            if (graphWindow == null || graphWindow.graphDict == null)
            {
                return;
            }

            double offTime = GetNowTotalSeconds() - fileSaveTimer;
            if (offTime >= fileSaveTime && isSave == false)
            {
                isSave = true;
                TaskHelp.AddTask(() =>
                {
                    RollbackHelper.SaveRollbackVer(graphWindow.BackVerPath, graphWindow.graphDict.Keys.ToList());
                }, () =>
                {
                    Debug.Log("保存版本");
                    fileSaveTimer = GetNowTotalSeconds();
                    AssetDatabase.Refresh();
                    Init(graphWindow);
                    isSave = false;
                });
            }
        }

        private void CreateLayout()
        {
            var xml = NodeGraphDefine.LoadUXML(pinnedElementTree);
            styleSheets.Add(NodeGraphDefine.LoadUSS(pinnedElementStyle));
            xml.CloneTree(this);

            //滑动条
            scrollView = this.Q<ScrollView>(name: "scroll");
        }

        public void Init(BaseGraphWindow graphWindow)
        {
            scrollView.Clear();
            rollbackVerList = RollbackHelper.GetRollbackVerPathList(graphWindow.BackVerPath);
            if (rollbackVerList == null || rollbackVerList.Count <= 0)
                return;
            if (rollbackVerList.Count > totalSaveCnt)
            {
                RollbackHelper.DelTopBackVer(rollbackVerList[0]);
                rollbackVerList.RemoveAt(0);
            }
            for (int i = 0; i < rollbackVerList.Count; i++)
            {
                string filePath = rollbackVerList[i];
                FileInfo backVerFileInfo = new FileInfo(filePath);
                Button btn = new Button(() =>
                {
                    Dictionary<string, BaseGraph> graphDict = RollbackHelper.GetRollbackVer(filePath);
                    graphWindow.RollBack(graphDict);
                });
                btn.text = backVerFileInfo.Name.Replace(backVerFileInfo.Extension, "");
                scrollView.Add(btn);
            }
        }
    }
}
