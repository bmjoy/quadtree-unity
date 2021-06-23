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
public class QuadTreeTestDynamic : QuadTreeTestBase
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
    private void createObjects(int count = 200)
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
            obj.rect.x += userData.vx * deltaTime;
            obj.rect.y += userData.vy * deltaTime;

            if (obj.rect.x > 1) obj.rect.x = 0;
            if (obj.rect.x < 0) obj.rect.x = 1;
            if (obj.rect.y > 1) obj.rect.y = 0;
            if (obj.rect.y < 0) obj.rect.y = 1;

            root.Insert(obj);
        }
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
        checkQuery();
    }
}
