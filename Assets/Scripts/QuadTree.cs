using System;
using System.Collections.Generic;
using System.Text;
using Vector2 = UnityEngine.Vector2;
using Rect = MyRect;
using UnityEngine;
// 通过Profiler分析Unity的Rect的各种getter有很大的性能问题

public struct MyRect
{
    public float x;
    public float y;
    public float width;
    public float height;
    public MyRect(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public Vector2 size
    {
        get
        {
            return new Vector2(width, height);
        }
    }
    public Vector2 min
    {
        get
        {
            return new Vector2(x, y);
        }
    }
    public Vector2 max
    {
        get
        {
            return new Vector2(x + width, y + height);
        }
    }
    public float xMin
    {
        get
        {
            return x;
        }
    }
    public float yMin
    {
        get
        {
            return y;
        }
    }
    public float xMax
    {
        get
        {
            return x + width;
        }
    }
    public float yMax
    {
        get
        {
            return y + height;
        }
    }
    public override string ToString()
    {
        return $"x={x},y={y},w={width},h={height}";
    }
}



/*
四叉树：
一个Rect归属于与它相交的四叉树节点(可能有多个)
当一个四叉树的节点中的对象数量超过maxObjects的时候就分裂为四个
当四叉树的节点的层数等于maxLevels的时候不再进行分裂，根节点为0层
*/
public class QuadTree
{
    #region TEST
    public static void TestCase()
    {
        Test_RectOverlaps();
        Test_Insert();
    }
    private static void Test_RectOverlaps()
    {
        Rect a;
        Rect b;
        {
            a = new Rect(0, 0, 1, 1);
            b = new Rect(0, 0, 0.5f, 0.5f);
            Test_Assert(RectOverlaps(a, b) == true);//相交
        }
        {
            a = new Rect(0, 0, 1, 1);
            b = new Rect(0, -1, 1, 1);
            Test_Assert(RectOverlaps(a, b) == false);//相切
        }
        {
            a = new Rect(0, 0, 1, 1);
            b = new Rect(-1, -1, 0.5f, 0.5f);
            Test_Assert(RectOverlaps(a, b) == false);//相离
        }
        {
            a = new Rect(0, 0, 1, 1);
            b = new Rect(0, 0, 0.5f, 0.5f);
            Test_Assert(RectOverlaps(a, b) == true);//a包含b
        }
        {
            a = new Rect(0, 0, 0.5f, 0.5f);
            b = new Rect(0, 0, 1, 1);
            Test_Assert(RectOverlaps(a, b) == true);//b包含a
        }
    }
    private static void Test_Insert()
    {
        var root = new QuadTree(new Rect(0, 0, 1, 1), 2, 4, 0);
        var obj = new RectInfo();
        obj.rect = new Rect(0, 0, 0.5f, 0.5f);

        {
            root.Insert(obj);
            Test_Assert(root.objects.Contains(obj) == true);
        }
        {
            root.split();
            Test_Assert(root.objects.Contains(obj) == false);
            Test_Assert(root.children != null);
            Test_Assert(root.children[0].objects.Contains(obj) == false);
            Test_Assert(root.children[1].objects.Contains(obj) == false);
            Test_Assert(root.children[2].objects.Contains(obj) == true);
            Test_Assert(root.children[3].objects.Contains(obj) == false);
        }
        {
            root.Clear();
            Test_Assert(root.objects.Count == 0);
            Test_Assert(root.children == null);
            obj.rect = new Rect(-1, -1, 1, 1);
            root.Insert(obj);
            Test_Assert(root.objects.Count == 0);
        }
        {
            root.Clear();
            var rect0 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            var rect1 = new Rect(0, 0.5f, 0.5f, 0.5f);
            var rect2 = new Rect(0, 0, 0.5f, 0.5f);
            var rect3 = new Rect(0.5f, 0, 0.5f, 0.5f);
            var halfSize = new Vector2(0.5f, 0.5f);

            var obj0 = new RectInfo();
            obj0.rect = rect0;
            var obj1 = new RectInfo();
            obj1.rect = rect1;
            var obj2 = new RectInfo();
            obj2.rect = rect2;
            var obj3 = new RectInfo();
            obj3.rect = rect3;
            root.Insert(obj0);
            root.Insert(obj1);
            root.Insert(obj2);
            root.Insert(obj3);
            Test_Assert(root.objects.Count == 0);
            Test_Assert(root.children != null);

            Test_Assert(root.children[0].objects.Count == 1);
            Test_Assert(root.children[1].objects.Count == 1);
            Test_Assert(root.children[2].objects.Count == 1);
            Test_Assert(root.children[3].objects.Count == 1);
            Test_Assert(root.children[0].objects.Contains(obj0) == true);
            Test_Assert(root.children[1].objects.Contains(obj1) == true);
            Test_Assert(root.children[2].objects.Contains(obj2) == true);
            Test_Assert(root.children[3].objects.Contains(obj3) == true);
            Test_Assert(root.children[0].rect.size == halfSize);
            Test_Assert(root.children[1].rect.size == halfSize);
            Test_Assert(root.children[2].rect.size == halfSize);
            Test_Assert(root.children[3].rect.size == halfSize);

            var reslut0 = root.Query(rect0);
            Test_Assert(reslut0.Count == 1);
            Test_Assert(reslut0.Contains(obj0) == true);

            var reslut1 = root.Query(rect1);
            Test_Assert(reslut1.Count == 1);
            Test_Assert(reslut1.Contains(obj1) == true);

            var reslut2 = root.Query(rect2);
            Test_Assert(reslut2.Count == 1);
            Test_Assert(reslut2.Contains(obj2) == true);

            var reslut3 = root.Query(rect3);
            Test_Assert(reslut3.Count == 1);
            Test_Assert(reslut3.Contains(obj3) == true);

            var reslut4 = root.Query(new Rect(0, 0, 1, 1));
            Test_Assert(reslut4.Count == 4);
            Test_Assert(reslut4.Contains(obj0) == true);
            Test_Assert(reslut4.Contains(obj1) == true);
            Test_Assert(reslut4.Contains(obj2) == true);
            Test_Assert(reslut4.Contains(obj3) == true);
        }
    }

    private static void Test_Assert(bool isTrue)
    {
        if (!isTrue)
        {
            throw new Exception("TEST NOT OK!");
        }
    }
    #endregion // TEST


    #region Util
    public class RectInfo
    {
        public Rect rect;
        public object userData;
    }

    public static bool RectOverlaps(Rect a, Rect b)
    {
        var aHalfW = a.width / 2;
        var aHalfH = a.height / 2;
        var bHalfW = b.width / 2;
        var bHalfH = b.height / 2;

        var aCenterX = a.x + aHalfW;
        var aCenterY = a.y + aHalfH;
        var bCenterX = b.x + bHalfW;
        var bCenterY = b.y + bHalfH;

        var xAbsDis = aCenterX > bCenterX ? aCenterX - bCenterX : bCenterX - aCenterX;
        var yAbsDis = aCenterY > bCenterY ? aCenterY - bCenterY : bCenterY - aCenterY;

        if (xAbsDis < (aHalfW + bHalfW) && yAbsDis < (aHalfH + bHalfH))
        {
            return true;
        }
        return false;
    }
    #endregion // Util


    #region Constructor
    private uint maxObjects;
    private uint maxLevels;
    private uint level;

    public int id;
    public Rect rect;
    public QuadTree parent;
    public QuadTree[] children;
    public List<RectInfo> objects;
    public QuadTree(Rect rect, uint maxObjects, uint maxLevels, uint level)
    {
        this.rect = rect;
        this.maxObjects = maxObjects;
        this.maxLevels = maxLevels;
        this.level = level;
        objects = new List<RectInfo>();
    }
    #endregion // Constructor


    #region Insert
    // 确保obj与本节点相交再插入
    public bool Insert(RectInfo obj)
    {
        if (RectOverlaps(this.rect, obj.rect))
        {
            insertImpl(obj);
            return true;
        }
        else
        {
            // Debug.Log(obj.rect);
            return false;
        }
    }
    private void insertImpl(RectInfo obj)
    {
        if (this.children != null)
        {
            insertToChildren(obj);
        }
        else
        {
            objects.Add(obj);
            if (objects.Count > maxObjects && this.level < this.maxLevels)
            {
                split();
            }
        }
    }
    // 插入子节点意味着obj一定与本节点相交
    // 1 0
    // 2 3
    private void insertToChildren(RectInfo obj)
    {
        var queryRect = obj.rect;
        var bitmap = queryChildOverlap(queryRect);
        for (int i = 0; i < 4; i++)
        {
            if ((bitmap & (1 << i)) != 0)
            {
                this.children[i].insertImpl(obj);
            }
        }
    }
    // 1 0
    // 2 3
    private void split()
    {
        var nodeRect = this.rect;

        var nodeMinX = nodeRect.x;
        var nodeMinY = nodeRect.y;
        var nodeHalfW = nodeRect.width / 2;
        var nodeHalfH = nodeRect.height / 2;
        var nodeCenterX = nodeRect.x + nodeRect.width / 2;
        var nodeCenterY = nodeRect.y + nodeRect.height / 2;

        this.children = new QuadTree[4];
        var childLevel = this.level + 1;
        {
            var topRight = new Rect(nodeCenterX, nodeCenterY, nodeHalfW, nodeHalfH);
            this.children[0] = new QuadTree(topRight, maxObjects, maxLevels, childLevel);
            var topLeft = new Rect(nodeMinX, nodeCenterY, nodeHalfW, nodeHalfH);
            this.children[1] = new QuadTree(topLeft, maxObjects, maxLevels, childLevel);
            var bottomLeft = new Rect(nodeMinX, nodeMinY, nodeHalfW, nodeHalfH);
            this.children[2] = new QuadTree(bottomLeft, maxObjects, maxLevels, childLevel);
            var bottomRight = new Rect(nodeCenterX, nodeMinY, nodeHalfW, nodeHalfH);
            this.children[3] = new QuadTree(bottomRight, maxObjects, maxLevels, childLevel);
        }

        for (int i = 0; i < this.children.Length; i++)
        {
            this.children[i].id = i;
            this.children[i].parent = this;
        }
        foreach (var obj in objects)
        {
            this.insertImpl(obj);
        }
        objects.Clear();
    }
    #endregion // Insert

    #region Common
    // 该函数调用前,queryRect一定是与本节点相交的
    // 1 0
    // 2 3
    private int queryChildOverlap(Rect queryRect)
    {
        var result = 0;

        var nodeRect = this.rect;

        var nodeCenterX = nodeRect.x + nodeRect.width / 2;
        var nodeCenterY = nodeRect.y + nodeRect.height / 2;

        var xMaxOnRight = (queryRect.x + queryRect.width) > nodeCenterX;
        var xMinOnLeft = queryRect.x < nodeCenterX;
        var yMaxOnTop = (queryRect.y + queryRect.height) > nodeCenterY;
        var yMinOnBottom = queryRect.y < nodeCenterY;

        if (xMaxOnRight && yMaxOnTop)
        {
            result |= 1 << 0;
        }
        if (xMinOnLeft && yMaxOnTop)
        {
            result |= 1 << 1;
        }
        if (xMinOnLeft && yMinOnBottom)
        {
            result |= 1 << 2;
        }
        if (xMaxOnRight && yMinOnBottom)
        {
            result |= 1 << 3;
        }
        return result;
    }
    #endregion // Common

    #region Query
    public List<RectInfo> Query(Rect queryRect)
    {
        if (RectOverlaps(this.rect, queryRect))
        {
            var result = new List<RectInfo>();
            queryImpl(queryRect, result);
            return result;
        }
        return null;
    }
    // 查询意味着一定与本节点相交
    // 1 0
    // 2 3
    private void queryImpl(Rect queryRect, List<RectInfo> result)
    {
        if (this.children == null)
        {
            result.AddRange(this.objects);
        }
        else
        {
            var bitmap = queryChildOverlap(queryRect);
            for (int i = 0; i < 4; i++)
            {
                if ((bitmap & (1 << i)) != 0)
                {
                    this.children[i].queryImpl(queryRect, result);
                }
            }
        }
    }
    #endregion // Query


    #region Helper
    public void Clear()
    {
        objects.Clear();
        if (this.children != null)
        {
            for (int i = 0; i < this.children.Length; i++)
            {
                this.children[i].Clear();
            }
            this.children = null;
        }
    }

    private static Stack<QuadTree> stack = new Stack<QuadTree>(16);
    private static StringBuilder sb = new StringBuilder(16);
    public string GetPath()
    {
        var parent = this;
        while (parent != null)
        {
            stack.Push(parent);
            parent = parent.parent;
        }

        if (stack.Count > 0)
        {
            sb.Append(stack.Pop().id);
        }

        while (stack.Count > 0)
        {
            sb.Append('-');
            sb.Append(stack.Pop().id);
        }
        var result = sb.ToString();
        sb.Clear();
        return result;
    }
    #endregion // Helper
} // class