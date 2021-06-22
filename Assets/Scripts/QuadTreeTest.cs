using System;
using System.Linq;
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using RectInfo = QuadTree.RectInfo;

public static class GizmosExt
{
    public static void DrawRect(Rect rect)
    {
        Gizmos.DrawLine(rect.min, new Vector2(rect.xMax, rect.yMin));
        Gizmos.DrawLine(rect.min, new Vector2(rect.xMin, rect.yMax));
        Gizmos.DrawLine(rect.max, new Vector2(rect.xMax, rect.yMin));
        Gizmos.DrawLine(rect.max, new Vector2(rect.xMin, rect.yMax));
    }

    public static RectInfo RandomRectInfo()
    {
        var rectInfo = new RectInfo();
        var x = Random.Range(0f, 1f);
        var y = Random.Range(0f, 1f);
        var w = Random.Range(0.005f, 0.08f);
        var h = Random.Range(0.005f, 0.08f);
        rectInfo.rect = new Rect(x, y, w, h);
        return rectInfo;
    }
}


[ExecuteInEditMode]
[DisallowMultipleComponent]
public class QuadTreeTest : ListenUnityEditorSceneView
{
    [Button]
    private void RunTestCase()
    {
        QuadTree.TestCase();
        Debug.Log("TestOk");
    }

    public override void OnSceneGUI(SceneView sceneView)
    {
        checkMouse();
    }
    public void Update()
    {
        initQuadTree();
    }

    #region  QuadTree Operate
    private QuadTree root;
    private RectInfo hero;
    private List<RectInfo> objs = new List<RectInfo>();
    private void initQuadTree()
    {
        if (root == null)
        {
            root = new QuadTree(new Rect(0, 0, 1, 1), 10, 4, 0);
            hero = new RectInfo();
            hero.rect = new Rect(0, 0, 0.1f, 0.1f);
            objs.Clear();
        }
    }

    private void checkMouse()
    {
        if (IsMouseMove && hero != null)
        {
            var pos = MouseMoveWorldPos;
            hero.rect.center = new Vector2(pos.x, pos.y);
        }
        if (IsMouseClick)
        {
            var pos = MouseClickWorldPos;
            addObj(new Vector2(pos.x, pos.y));
        }
        if (IsMouseMove)
        {
            checkQuery();
        }
    }
    private void addObj(Vector2 pos)
    {
        if (root == null)
        {
            return;
        }
        var obj = GizmosExt.RandomRectInfo();
        obj.rect.center = pos;
        if (root.Insert(obj))
        {
            objs.Add(obj);
            Debug.Log(obj.rect);
            Debug.Log("addObj");
        }
    }
    private List<RectInfo> queryResult;
    private void checkQuery()
    {
        if (root == null || hero == null)
        {
            return;
        }
        foreach (var obj in objs)
        {
            obj.userData = false;
        }
        var result = root.Query(hero.rect);
        if (result != null)
        {
            result = result.Distinct().ToList();
            foreach (var obj in result)
            {
                obj.userData = true;
            }
        }
        this.queryResult = result;
    }
    #endregion // QuadTree Operate

    #region  OnDrawGizmos
    private void OnDrawGizmos()
    {
        drawQuadTree();
        drawObjects();
        drawHero();
    }
    private void drawQuadTree()
    {
        drawQuadTreeImpl(root);
    }
    private void drawQuadTreeImpl(QuadTree tree)
    {
        if (tree == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        if (tree.children != null)
        {
            foreach (var item in tree.children)
            {
                drawQuadTreeImpl(item);
            }
        }
        else
        {
            GizmosExt.DrawRect(tree.rect);
        }
    }
    private void drawObjects()
    {
        Gizmos.color = Color.white;
        foreach (var obj in objs)
        {
            if (obj.userData != null)
            {
                var collid = (bool)obj.userData;
                if (collid)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
            }
            GizmosExt.DrawRect(obj.rect);
        }
    }
    private void drawHero()
    {
        if (root == null && hero == null)
        {
            return;
        }
        if (QuadTree.RectOverlaps(root.rect, hero.rect))
        {
            Gizmos.color = Color.green;
            GizmosExt.DrawRect(hero.rect);
        }
    }
    #endregion // OnDrawGizmos

    #region Show Infos
    [ShowInInspector]
    private int checkCount
    {
        get { return this.queryResult != null ? this.queryResult.Count : 0; }
    }
    [ShowInInspector]
    private int totleCount
    {
        get { return objs.Count; }
    }
    [ShowInInspector]
    private string checkPercent
    {
        get
        {
            var totleCount = this.totleCount;
            if (this.totleCount != 0)
            {
                var percent = (float)checkCount / (float)totleCount;
                percent = percent * 100;
                return $"{percent:F1}%";
            }
            return "0%";
        }
    }
    #endregion // Show Infos

    [Button]
    private void benchmark_Insert()
    {
        clearTree();
        var insertCount = 10000;
        var tmpObjs = new List<RectInfo>();
        {
            for (int i = 0; i < insertCount; i++)
            {
                var obj = GizmosExt.RandomRectInfo();
                tmpObjs.Add(obj);
            }
        }

        // https://blog.csdn.net/suifcd/article/details/44175027
        var stopwatch = new Diagnostics.Stopwatch();
        stopwatch.Start(); //  开始监视代码运行时间
        var count = 0;
        {
            for (int i = 0; i < insertCount; i++)
            {
                var obj = tmpObjs[i];
                if (root.Insert(obj))
                {
                    objs.Add(obj);
                    count++;
                }
            }
        }
        stopwatch.Stop();
        var timespan = stopwatch.Elapsed;
        double milliseconds = timespan.TotalMilliseconds;
        Debug.Log($"Insert 10000 count = {count}");
        Debug.Log($"Insert 10000 time = {milliseconds} ms");
    }

    [Button]
    private void clearTree()
    {
        if (root != null)
        {
            root.Clear();
        }
        objs.Clear();
    }
}
