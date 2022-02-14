using Godot;

public class Slime : Unit
{
    private bool _attackStraight = true;
    public override void _Ready()
    {
        base._Ready();
        Target = GetNode<Unit>("/root/Game/Map/Player");
    }

    public override void _Process(float delta)
    {
        if (State == UnitState.Normal && Target != null)
        {
            LookAtPosition = Target.GlobalPosition;
            AttackWithCooldown(Target.GlobalPosition);
        }
        base._Process(delta);
    }

    protected override void PerformAttack(Vector2 aim)
    {
        if (_attackStraight)
        {
            FireProjectile(GlobalPosition + Vector2.Up);
            FireProjectile(GlobalPosition + Vector2.Down);
            FireProjectile(GlobalPosition + Vector2.Left);
            FireProjectile(GlobalPosition + Vector2.Right);
        }
        else
        {
            FireProjectile(GlobalPosition + new Vector2(-1, 1));
            FireProjectile(GlobalPosition + new Vector2(-1, -1));
            FireProjectile(GlobalPosition + new Vector2(1, 1));
            FireProjectile(GlobalPosition + new Vector2(1, -1));
        }
        _attackStraight = !_attackStraight;
    }

}
