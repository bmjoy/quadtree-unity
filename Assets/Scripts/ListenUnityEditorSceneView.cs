using System;
using System.Linq;
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class ListenUnityEditorSceneView : MonoBehaviour
{
    public static Vector3 ScreenPositionToWorldPotion(Vector3 mousePosition)
    {
        var currCamera = Camera.current;
        Ray ray = currCamera.ScreenPointToRay(new Vector3(mousePosition.x, -mousePosition.y + currCamera.pixelHeight));
        Vector3 worldPos = ray.origin;
        return worldPos;
    }

    public void OnEnable()
    {
        SceneView.duringSceneGui += duringSceneGui;
    }
    public void OnDisable()
    {
        SceneView.duringSceneGui -= duringSceneGui;
    }

    public bool IsMouseMove { get; private set; }
    [ShowInInspector, ReadOnly]
    public Vector3 MouseMoveWorldPos; // 此处的序列化字段是为了在数据变动的时候能够让Inspector实时刷新
    public bool IsMouseClick { get; private set; }
    public Vector3 MouseClickWorldPos { get; private set; }
    private void duringSceneGui(SceneView sceneView)
    {
        sceneView.Repaint();

        IsMouseMove = false;
        IsMouseClick = false;

        var unityEvent = Event.current;
        if (unityEvent.isMouse && unityEvent.type == EventType.MouseMove)
        {
            var worldPos = ScreenPositionToWorldPotion(unityEvent.mousePosition);
            IsMouseMove = true;
            MouseMoveWorldPos = worldPos;
            unityEvent.Use();
        }
        // https://docs.unity3d.com/ScriptReference/Event-button.html // unityEvent.button == 0 (Left Click)
        if (unityEvent.isMouse && unityEvent.type == EventType.MouseDown && unityEvent.button == 0)
        {
            var worldPos = ScreenPositionToWorldPotion(unityEvent.mousePosition);
            IsMouseClick = true;
            MouseClickWorldPos = worldPos;
            unityEvent.Use();
        }

        OnSceneGUI(sceneView);
    }

    public virtual void OnSceneGUI(SceneView sceneView)
    {

    }
}