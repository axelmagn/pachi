using Godot;

[Tool]
[GlobalClass]
public partial class HexGridOverlay : Node2D {

    [Export]
    public float HexRadius {
        get => _hexRadius;
        set {
            _hexRadius = value;
            QueueRedraw();
        }
    }
    private float _hexRadius = 16.0f;


    public override void _Draw() {
        // TODO

        // TEMP: draw a hex at the center of the object
        DrawHexSolid(Vector2.Zero, 16.0f, new("FFFF00FF"));
        DrawHexLines(Vector2.Zero, 18.0f, new("FF00FFFF"), 2.0f);
    }

    private void DrawHexSolid(Vector2 position, float radius, Color color) {
        Vector2[] points = new Vector2[6];
        Color[] colors = new Color[6];
        Vector2 cursor = new(0, radius);
        for(int i = 0; i < points.Length; i++) {
            points[i] = cursor + position;
            colors[i] = color;
            cursor = cursor.Rotated(Mathf.Pi / 3);
        }
        DrawPolygon(points, colors);
    }

    private void DrawHexLines(Vector2 position, float radius, Color color, float width) {
        Vector2[] points = new Vector2[7];
        Vector2 cursor = new(0, radius);
        for(int i = 0; i < points.Length; i++) {
            points[i] = cursor + position;
            cursor = cursor.Rotated(Mathf.Pi / 3);
        }
        DrawPolyline(points, color, width);
    }
}
