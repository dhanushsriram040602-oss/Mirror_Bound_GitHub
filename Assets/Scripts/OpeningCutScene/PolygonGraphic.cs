using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class PolygonGraphic : MaskableGraphic
{
    [Tooltip("Assign the texture you want to show here.")]
    public Texture textureOverride;

    [Tooltip("Four points in normalized rect space. Order: BL, TL, TR, BR")]
    public Vector2[] points = new Vector2[4]
    {
        new Vector2(0f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
        new Vector2(1f, 0f)
    };

    public override Texture mainTexture => textureOverride != null ? textureOverride : s_WhiteTexture;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length != 4)
            return;

        Rect r = GetPixelAdjustedRect();

        UIVertex[] verts = new UIVertex[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 p = points[i];
            verts[i] = UIVertex.simpleVert;
            verts[i].color = color;
            verts[i].position = new Vector3(
                Mathf.Lerp(r.xMin, r.xMax, p.x),
                Mathf.Lerp(r.yMin, r.yMax, p.y),
                0f
            );
            verts[i].uv0 = p;
        }

        vh.AddUIVertexQuad(verts);
    }
}