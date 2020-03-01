using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlinkSprite : MonoBehaviour
{
    void FixedUpdate()
    {
        var alpha = (Mathf.Abs(Mathf.Sin(Time.time * 12))) < 0.4 ? 0 : 1;
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
    }

    void OnDestroy()
    {
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
