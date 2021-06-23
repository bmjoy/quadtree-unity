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

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class QuadTreeTestMany : QuadTreeTestBase
{
    protected override void initQuadTree()
    {
        if (root == null)
        {
            base.initQuadTree();
            createObjects();
        }
    }

    [Button]
    private void createObjects(int count = 100)
    {
        for (var i = 0; i < count; i++)
        {
            var rectInfo = GizmosExt.RandomRectInfo();
            if (root.Insert(rectInfo))
            {
                objs.Add(rectInfo);
            }
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        updateRectInfoMove(deltaTime);
    }

    protected virtual void updateRectInfoMove(float deltaTime)
    {
        if (root == null)
        {
            return;
        }
        root.Clear();
        for (var i = 0; i < objs.Count; i++)
        {
            var obj = objs[i];
            var userData = obj.userData as UserDataBase;
            userData.check = false;
            obj.rect.x += userData.vx * deltaTime;
            obj.rect.y += userData.vy * deltaTime;

            if (obj.rect.xMax > 1) userData.vx = -Math.Abs(userData.vx);
            if (obj.rect.x < 0) userData.vx = Math.Abs(userData.vx);
            if (obj.rect.yMax > 1) userData.vy = -Math.Abs(userData.vy);
            if (obj.rect.y < 0) userData.vy = Math.Abs(userData.vy);

            root.Insert(obj);
        }

        for (var i = 0; i < objs.Count; i++)
        {
            var obj = objs[i];
            var candidates = root.Query(obj.rect);
            if (candidates != null)
            {
                foreach (var otherObj in candidates)
                {
                    if (obj == otherObj)
                    {
                        continue;
                    }
                    cauculateCollision(obj, otherObj);
                }
            }
        }
    }

    // 一定相交, a 碰向 b 此时b看作不动
    private void cauculateCollision(RectInfo aRectInfo, RectInfo bRectInfo)
    {
        var aUserData = aRectInfo.userData as UserDataBase;
        var bUserData = bRectInfo.userData as UserDataBase;

        Rect a = aRectInfo.rect;
        Rect b = bRectInfo.rect;
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

        var overlaps = false;
        if (xAbsDis < (aHalfW + bHalfW) && yAbsDis < (aHalfH + bHalfH))
        {
            overlaps = true;
        }

        if (!overlaps)
        {
            return;
        }
        aUserData.check = true;
        bUserData.check = true;
        var pushX = (aHalfW + bHalfW) - xAbsDis;
        var pushY = (aHalfH + bHalfH) - yAbsDis;

        if (pushX < pushY) // 选择移动距离小的
        {
            if (aCenterX > bCenterX) // a 在 b 右边
            {
                a.x = b.x + b.width;
            }
            else
            {
                a.x = b.x - a.width;
            }
            //reverse X trajectory (bounce)
            aUserData.vx *= -1;
        }
        else
        {
            if (aCenterY > bCenterY)// a 在 b 上边
            {
                a.y = b.y + b.height;
            }
            else
            {
                a.y = b.y - a.height;
            }
            //reverse Y trajectory (bounce)
            aUserData.vy *= -1;
        }
        aRectInfo.rect = a;

        if (aRectInfo.rect.xMax > 1) aUserData.vx = -Math.Abs(aUserData.vx);
        if (aRectInfo.rect.x < 0) aUserData.vx = Math.Abs(aUserData.vx);
        if (aRectInfo.rect.yMax > 1) aUserData.vy = -Math.Abs(aUserData.vy);
        if (aRectInfo.rect.y < 0) aUserData.vy = Math.Abs(aUserData.vy);
    }

    protected override void checkMouse()
    {
        if (IsMouseMove && hero != null)
        {
            var pos = MouseMoveWorldPos;
            {
                hero.rect.x = pos.x - hero.rect.width / 2;
                hero.rect.y = pos.y - hero.rect.height / 2;
            }
        }
        // checkQuery();
    }
}
