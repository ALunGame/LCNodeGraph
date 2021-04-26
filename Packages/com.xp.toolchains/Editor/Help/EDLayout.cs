using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace XPToolchains.Help
{
    /// <summary>
    /// 布局
    /// </summary>
    public class EDLayout
    {

        /// <summary>
        /// 创建滑动条
        /// </summary>
        public static void CreateScrollView(ref Vector2 pos, string style, float width, float height, Action drawFunc)
        {
            if (style == "")
            {
                pos = GUILayout.BeginScrollView(pos, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                pos = GUILayout.BeginScrollView(pos, style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 创建滑动条
        /// </summary>
        public static void CreateScrollView(ref Vector2 pos, string style, float width, float height, Action<float, float> drawFunc)
        {
            if (style == "")
            {
                pos = GUILayout.BeginScrollView(pos, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndScrollView();
            }
            else
            {
                pos = GUILayout.BeginScrollView(pos, style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 创建垂直布局
        /// </summary>
        public static void CreateVertical(string style, float width, float height, Action drawFunc)
        {
            if (style == "")
            {
                GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 创建垂直布局
        /// </summary>
        public static void CreateVertical(string style, float width, float height, Action<float, float> drawFunc)
        {
            if (style == "")
            {
                GUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 创建水平布局
        /// </summary>
        public static void CreateHorizontal(string style, float width, float height, Action drawFunc)
        {
            if (style == "")
            {
                GUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal(style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 创建水平布局
        /// </summary>
        public static void CreateHorizontal(string style, float width, float height, Action<float, float> drawFunc)
        {
            if (style == "")
            {
                GUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal(style, GUILayout.Width(width), GUILayout.Height(height));
                {
                    drawFunc?.Invoke(width, height);
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 创建垂直按钮列表
        /// </summary>
        public static void CreateVerticalBtnList(ref Vector2 pos, List<string> itemList, float width, float height, Action<string> selCallBack, string style = "box")
        {
            CreateScrollView(ref pos, style, width, height, () =>
             {
                 for (int i = 0; i < itemList.Count; i++)
                 {
                     string item = itemList[i];
                     EDButton.CreateBtn(item, width * 0.9f, 25, () =>
                     {
                         selCallBack(item);
                     });
                 }
             });
        }

        public static void CreateBoxArea(string style, float width, float height, string name = "")
        {
            GUILayout.Box(name, style, GUILayout.Width(width), GUILayout.Height(height));
        }

        /// <summary>
        /// 绘制背景格
        /// </summary>
        /// <param name="gridSpacing">格子间隔</param>
        /// <param name="gridOpacity">格子透明度</param>
        /// <param name="gridColor">格子颜色</param>
        /// <param name="drawRect">绘制范围</param>
        /// <param name="offset">偏移</param>
        /// <param name="drag">鼠标拖移</param>
        public static void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor, Rect drawRect, ref Vector2 offset, Vector2 drag)
        {
            //宽数量
            int widthDivs = Mathf.CeilToInt(drawRect.width / gridSpacing);
            //高数量
            int heightDivs = Mathf.CeilToInt(drawRect.height / gridSpacing);

            //Handles.BeginGUI(new Rect(1,1,1,1));

            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0) + new Vector3(drawRect.position.x, drawRect.position.y);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, drawRect.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(drawRect.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;

            //Handles.EndGUI();
        }

        public static void CreateTableView(ref Vector2 pos, ref int columnPage, ref int rowPage, float width, float height, float boxWidth, float boxHeight, int columnCnt, int rowCnt, Action<int, int> drawFunc)
        {
            int totalColumnCnt = 10;
            int totalRowCnt = 50;

            int showColumnCnt = columnCnt >= totalColumnCnt ? totalColumnCnt : columnCnt;
            int showRowCnt = rowCnt >= totalRowCnt ? totalRowCnt : rowCnt;

            //分页处理
            int startColumnIndex = 0;
            int endColumnIndex = 0;
            int startRowIndex = 0;
            int endRowIndex = 0;

            if (columnPage <= 0)
            {
                columnPage = 0;
            }
            if (rowPage <= 0)
            {
                rowPage = 0;
            }

            //列分页
            if (columnCnt <= showColumnCnt)
            {
                startColumnIndex = 0;
                endColumnIndex = columnCnt;
            }
            else
            {
                startColumnIndex = columnPage * totalColumnCnt;
                //超过了
                if (startColumnIndex >= columnCnt)
                {
                    startColumnIndex = columnCnt - totalColumnCnt;
                    endColumnIndex = columnCnt;
                }
                else
                {
                    endColumnIndex = columnPage * totalColumnCnt + totalColumnCnt;
                }
            }

            //行分页
            if (rowCnt <= showRowCnt)
            {
                startRowIndex = 0;
                endRowIndex = rowCnt;
            }
            else
            {
                startRowIndex = rowPage * totalRowCnt;
                //超过了
                if (startRowIndex >= rowCnt)
                {
                    startRowIndex = rowCnt - totalRowCnt;
                    endRowIndex = rowCnt;
                }
                else
                {
                    endRowIndex = rowPage * totalRowCnt + totalRowCnt;
                }
            }

            CreateScrollView(ref pos, "box", width, height, () =>
            {
                CreateHorizontal("", width, height, () =>
                {
                    for (int i = startColumnIndex; i < endColumnIndex; i++)
                    {
                        CreateVertical("", boxWidth, height, () =>
                        {
                            for (int j = startRowIndex; j < endRowIndex; j++)
                            {
                                CreateVertical("box", boxWidth - 10, boxHeight - 10, () =>
                                {
                                    drawFunc(i, j);
                                });
                            }
                        });
                    }
                });

            });
        }
    }
}
