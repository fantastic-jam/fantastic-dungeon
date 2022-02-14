using Godot;

public class Gobelin : Unit
{
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
        FireProjectile(aim - GlobalPosition.Rotated(0.05f) + GlobalPosition);
        FireProjectile(aim);
        FireProjectile(aim - GlobalPosition.Rotated(-0.05f) + GlobalPosition);
    }

}
