using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Rect = UnityEngine.Rect;
using Vector2 = UnityEngine.Vector2;

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
        obj.bounds = new Rect(0, 0, 0.5f, 0.5f);

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
            obj.bounds = new Rect(-1, -1, 1, 1);
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
            obj0.bounds = rect0;
            var obj1 = new RectInfo();
            obj1.bounds = rect1;
            var obj2 = new RectInfo();
            obj2.bounds = rect2;
            var obj3 = new RectInfo();
            obj3.bounds = rect3;
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
            Test_Assert(root.children[0].bounds.size == halfSize);
            Test_Assert(root.children[1].bounds.size == halfSize);
            Test_Assert(root.children[2].bounds.size == halfSize);
            Test_Assert(root.children[3].bounds.size == halfSize);

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


    public class RectInfo
    {
        public Rect bounds;
        public object userData;
    }

    public static bool RectOverlaps(Rect a, Rect b)
    {
        var aCenter = a.center;
        var bCenter = b.center;
        var xDis = Math.Abs(aCenter.x - bCenter.x);
        var yDis = Math.Abs(aCenter.y - bCenter.y);
        if (xDis < (a.width + b.width) / 2 && yDis < (a.height + b.height) / 2)
        {
            return true;
        }
        return false;
    }

    private uint maxObjects;
    private uint maxLevels;
    private uint level;
    public Rect bounds;
    public int Id;
    public QuadTree Parent;
    public QuadTree(Rect bounds, uint maxObjects, uint maxLevels, uint level)
    {
        this.bounds = bounds;
        this.maxObjects = maxObjects;
        this.maxLevels = maxLevels;
        this.level = level;
    }

    public bool Insert(RectInfo rect)
    {
        if (RectOverlaps(this.bounds, rect.bounds))
        {
            insertImpl(rect);
            return true;
        }
        return false;
    }

    public QuadTree[] children;
    private List<RectInfo> objects = new List<RectInfo>();
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

    // 插入子节点意味着一定与本节点相交
    // 1 0
    // 2 3
    private void insertToChildren(RectInfo obj)
    {
        var center = this.bounds.center;
        var objRect = obj.bounds;
        var objMin = objRect.min;
        var objMax = objRect.max;

        var xMaxOnRight = objMax.x > center.x;
        var xMinOnLeft = objMin.x < center.x;
        var yMaxOnTop = objMax.y > center.y;
        var yMinOnBottom = objMin.y < center.y;

        if (xMaxOnRight && yMaxOnTop)
        {
            this.children[0].insertImpl(obj);
        }
        if (xMinOnLeft && yMaxOnTop)
        {
            this.children[1].insertImpl(obj);
        }
        if (xMinOnLeft && yMinOnBottom)
        {
            this.children[2].insertImpl(obj);
        }
        if (xMaxOnRight && yMinOnBottom)
        {
            this.children[3].insertImpl(obj);
        }
    }

    public List<RectInfo> Query(Rect objRect)
    {
        if (RectOverlaps(this.bounds, objRect))
        {
            var result = new List<RectInfo>();
            queryImpl(objRect, result);
            return result;
        }
        return null;
    }

    // 查询意味着一定与本节点相交
    // 1 0
    // 2 3
    private void queryImpl(Rect objRect, List<RectInfo> result)
    {
        if (this.children == null)
        {
            result.AddRange(this.objects);
        }
        else
        {
            var center = this.bounds.center;
            var objMin = objRect.min;
            var objMax = objRect.max;

            var xMaxOnRight = objMax.x > center.x;
            var xMinOnLeft = objMin.x < center.x;
            var yMaxOnTop = objMax.y > center.y;
            var yMinOnBottom = objMin.y < center.y;

            if (xMaxOnRight && yMaxOnTop)
            {
                this.children[0].queryImpl(objRect, result);
            }
            if (xMinOnLeft && yMaxOnTop)
            {
                this.children[1].queryImpl(objRect, result);
            }
            if (xMinOnLeft && yMinOnBottom)
            {
                this.children[2].queryImpl(objRect, result);
            }
            if (xMaxOnRight && yMinOnBottom)
            {
                this.children[3].queryImpl(objRect, result);
            }
        }
    }

    // 1 0
    // 2 3
    private void split()
    {
        var rect = this.bounds;
        var min = rect.min;
        var max = rect.max;
        var center = rect.center;
        var size = rect.size / 2;

        this.children = new QuadTree[4];

        var topRight = new Rect(center, size);
        this.children[0] = new QuadTree(topRight, maxObjects, maxLevels, level + 1);

        var topLeft = new Rect(new Vector2(min.x, center.y), size);
        this.children[1] = new QuadTree(topLeft, maxObjects, maxLevels, level + 1);

        var bottomLeft = new Rect(min, size);
        this.children[2] = new QuadTree(bottomLeft, maxObjects, maxLevels, level + 1);

        var bottomRight = new Rect(new Vector2(center.x, min.y), size);
        this.children[3] = new QuadTree(bottomRight, maxObjects, maxLevels, level + 1);

        for (int i = 0; i < this.children.Length; i++)
        {
            this.children[i].Id = i;
            this.children[i].Parent = this;
        }

        foreach (var obj in objects)
        {
            this.insertImpl(obj);
        }
        objects.Clear();
    }

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
            parent = parent.Parent;
        }

        if (stack.Count > 0)
        {
            sb.Append(stack.Pop().Id);
        }

        while (stack.Count > 0)
        {
            sb.Append('-');
            sb.Append(stack.Pop().Id);
        }
        var result = sb.ToString();
        sb.Clear();
        return result;
    }

}// class