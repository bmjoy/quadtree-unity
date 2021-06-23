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
public class QuadTreeTestStatic : QuadTreeTestBase
{
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
}
