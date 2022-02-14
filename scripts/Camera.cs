using Godot;

public class Camera : Camera2D
{
    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Confined);
        Input.SetCustomMouseCursor(ResourceLoader.Load("res://assets/crosshair_1.png"));
        var tileMap = GetNode<TileMap>("/root/Game/TileMapForeground");
        Vector2 cellSize = tileMap.CellSize;
        Rect2 limits = tileMap.GetUsedRect();
        LimitTop = (int)(limits.Position.y * cellSize.y);
        LimitBottom = (int)(limits.End.y * cellSize.y);
        LimitLeft = (int)(limits.Position.x * cellSize.x);
        LimitRight = (int)(limits.End.x * cellSize.x);
    }
}
