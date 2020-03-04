#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HittableEnemy))]
public class HittableEnemyEditor : Editor
{
    public void OnSceneGUI()
    {
        var enemy = target as HittableEnemy;
        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            var point = enemy.transform.InverseTransformPoint(Handles.PositionHandle(enemy.transform.TransformPoint(enemy.destroyEffectOffset), enemy.transform.rotation));
            if (cc.changed)
            {
                enemy.destroyEffectOffset = point;
            }
        }
        Handles.Label(enemy.transform.position, enemy.destroyEffectOffset.ToString());
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void OnDrawGizmo(HittableEnemy enemy, GizmoType gizmoType)
    {
        var point = enemy.transform.TransformPoint(enemy.destroyEffectOffset);
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(point, enemy.transform.forward, 0.1f);
    }
}
#endif
