using System;
using System.Linq;
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using RectInfo = QuadTree.RectInfo;
using Rect = MyRect;

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
        rectInfo.rect = new Rect(x - w / 2, y - h / 2, w, h);
        return rectInfo;
    }
}

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class QuadTreeTestBase : ListenUnityEditorSceneView
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
    protected QuadTree root;
    protected RectInfo hero;
    protected List<RectInfo> objs = new List<RectInfo>();
    protected virtual void initQuadTree()
    {
        if (root == null)
        {
            var max_objects = 10u;
            var max_levels = 4u;
            root = new QuadTree(new Rect(0, 0, 1, 1), max_objects, max_levels, 0);
            hero = new RectInfo();
            hero.rect = new Rect(0, 0, 0.1f, 0.1f);
            objs.Clear();
        }
    }

    protected virtual void checkMouse()
    {
    }

    protected virtual void addObj(Vector2 pos)
    {
        if (root == null)
        {
            return;
        }
        var obj = GizmosExt.RandomRectInfo();
        {
            obj.rect.x = pos.x - obj.rect.width / 2;
            obj.rect.y = pos.y - obj.rect.height / 2;
        }
        if (root.Insert(obj))
        {
            objs.Add(obj);
            Debug.Log(obj.rect);
            Debug.Log("addObj");
        }
    }
    protected List<RectInfo> queryResult;
    protected virtual void checkQuery()
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
    protected virtual void OnDrawGizmos()
    {
        drawObjects();
        drawQuadTree();
        drawHero();
    }
    protected virtual void drawQuadTree()
    {
        drawQuadTreeImpl(root);
    }
    protected virtual void drawQuadTreeImpl(QuadTree tree)
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
    protected virtual void drawObjects()
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
    protected virtual void drawHero()
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
    private void benchmark_Insert(int insertCount = 10000)
    {
        clearTree();
        var tmpObjs = new List<RectInfo>();
        {
            for (int i = 0; i < insertCount; i++)
            {
                var obj = GizmosExt.RandomRectInfo();
                tmpObjs.Add(obj);
            }
        }
        benchmark_InsertImpl(insertCount, tmpObjs);
    }
    private void benchmark_InsertImpl(int insertCount, List<RectInfo> tmpObjs)
    {
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
        Debug.Log($"Insert count = {count}");
        Debug.Log($"Insert time = {milliseconds} ms");
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