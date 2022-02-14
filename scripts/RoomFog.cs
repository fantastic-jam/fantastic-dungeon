using Godot;

public class RoomFog : Sprite
{
    [Export(PropertyHint.Enum, "visible,hidden")]
    private string _animationState;

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.Play(_animationState);
    }

    public void Dispel()
    {
        _animationPlayer.Play("dispel");
    }

    public void Show()
    {
        _animationPlayer.Play("show");
    }
}
