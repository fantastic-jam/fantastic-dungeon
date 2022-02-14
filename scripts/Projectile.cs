using Godot;

public class Projectile : Area2D
{
    public int Damage { get; set; }
    public float Speed { get; set; }
    public float SpriteRotation { get; set; }

    private AnimatedSprite _animatedSprite;
    private AnimatedSprite _shadow;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _shadow = GetNode<AnimatedSprite>("Shadow");
        _animatedSprite.Rotation = SpriteRotation;
        _shadow.Rotation = SpriteRotation;
    }

    public override void _PhysicsProcess(float delta)
    {
        Position += _animatedSprite.Transform.x * Speed * delta;
    }

    private void OnBodyEntered(CollisionObject2D body)
    {
        if (body is Unit unit)
            unit.TakeHit(this, Damage);
        QueueFree();
    }
}
