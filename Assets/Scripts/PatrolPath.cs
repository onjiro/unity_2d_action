using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public Vector2 startPosition, endPosition;
    public Vector2 StartPositionInGlobal() => this.transform.TransformPoint(this.startPosition);
    public Vector2 EndPositionInGlobal() => this.transform.TransformPoint(this.endPosition);
}
