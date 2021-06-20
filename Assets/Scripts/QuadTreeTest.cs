using System.Collections;
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
        rectInfo.bounds = new Rect(x, y, w, h);
        return rectInfo;
    }
}


[DisallowMultipleComponent]
[ExecuteInEditMode]
public class QuadTreeTest : MonoBehaviour
{
    public static Vector3 ScreenPositionToWorldPotion(Vector3 mousePosition)
    {
        Ray ray = Camera.current.ScreenPointToRay(new Vector3(mousePosition.x,
            -mousePosition.y + Camera.current.pixelHeight));
        Vector3 worldPos = ray.origin;
        return worldPos;
    }

    [Button]
    private void RunTestCase()
    {
        QuadTree.TestCase();
        Debug.Log("TestOk");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    public bool IsMouseMoving = false;
    public Vector3 MouseMoveWorldPos;
    public bool IsMouseLeftClick = false;
    public Vector3 MouseLeftClickWorldPos;
    private void OnSceneGUI(SceneView sceneView)
    {
        sceneView.Repaint();

        IsMouseMoving = false;
        IsMouseLeftClick = false;
        var unityEvent = Event.current;

        if (unityEvent.isMouse && unityEvent.type == EventType.MouseMove)
        {
            var worldPos = ScreenPositionToWorldPotion(unityEvent.mousePosition);
            IsMouseMoving = true;
            MouseMoveWorldPos = worldPos;
            unityEvent.Use();
        }

        // https://docs.unity3d.com/ScriptReference/Event-button.html // unityEvent.button == 0 (Left Click)
        if (unityEvent.isMouse && unityEvent.type == EventType.MouseDown && unityEvent.button == 0)
        {
            var worldPos = ScreenPositionToWorldPotion(unityEvent.mousePosition);
            IsMouseLeftClick = true;
            MouseLeftClickWorldPos = worldPos;
            unityEvent.Use();
        }

        if (unityEvent.isKey && unityEvent.type == EventType.KeyUp && unityEvent.keyCode == KeyCode.Escape)
        {
            unityEvent.Use();
        }
        CheckMouse();
    }

    void Update()
    {
        InitQuadTree();
    }

    private QuadTree root;
    private List<RectInfo> objs = new List<RectInfo>();
    private RectInfo hero;
    private void InitQuadTree()
    {
        if (root == null)
        {
            root = new QuadTree(new Rect(0, 0, 1, 1), 10, 4, 0);
            hero = new RectInfo();
            hero.bounds = new Rect(0, 0, 0.1f, 0.1f);
        }
    }
    private void CheckMouse()
    {
        if (IsMouseMoving && hero != null)
        {
            var pos = MouseMoveWorldPos;
            hero.bounds.center = new Vector2(pos.x, pos.y);
        }
        if (IsMouseLeftClick)
        {
            var pos = MouseLeftClickWorldPos;
            addObj(new Vector2(pos.x, pos.y));
        }
        CheckQuery();
    }

    [ShowInInspector]
    private int ReslutCount
    {
        get
        {
            var reslutCount = 0;
            if (this.result != null)
            {
                reslutCount = this.result.Count;
            }
            return reslutCount;
        }
    }
    private List<RectInfo> result;
    private void CheckQuery()
    {
        if (IsMouseMoving && hero != null && root != null)
        {
            foreach (var obj in objs)
            {
                obj.userData = false;
            }
            var result = root.Query(hero.bounds);
            if (result != null)
            {
                foreach (var obj in result)
                {
                    obj.userData = true;
                }
            }
            this.result = result;
        }
    }

    private void addObj(Vector2 pos)
    {
        if (root == null)
        {
            return;
        }

        var obj = GizmosExt.RandomRectInfo();
        obj.bounds.center = pos;
        if (root.Insert(obj))
        {
            objs.Add(obj);
            Debug.Log(obj.bounds);
            Debug.Log("addObj");
        }
    }

    private void DrawQuadTree(QuadTree tree)
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
                DrawQuadTree(item);
            }
        }
        else
        {
            GizmosExt.DrawRect(tree.bounds);
        }
    }
    private void DrawObjects()
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
            GizmosExt.DrawRect(obj.bounds);
        }
    }
    private void DrawHero(QuadTree tree, RectInfo hero)
    {
        if (tree == null && hero == null)
        {
            return;
        }
        if (QuadTree.RectOverlaps(tree.bounds, hero.bounds))
        {
            Gizmos.color = Color.green;
            GizmosExt.DrawRect(hero.bounds);
        }
    }

    private void OnDrawGizmos()
    {
        DrawQuadTree(root);
        DrawObjects();
        DrawHero(root, hero);

        if (!Application.isPlaying)
        {
            return;
        }
    }

    [Button]
    private void InsertBenchmark()
    {
        Clear();
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
        stopwatch.Stop(); //  停止监视
        var timespan = stopwatch.Elapsed;
        double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数
        Debug.Log($"Insert10000 count = {count}");
        Debug.Log($"Insert10000 time = {milliseconds} ms");
    }

    [Button]
    private void Clear()
    {
        if (root != null)
        {
            root.Clear();
        }
        objs.Clear();
    }
}
