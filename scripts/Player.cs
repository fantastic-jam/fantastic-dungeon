using Godot;

public class Player : Unit
{
	public override void _Process(float delta)
	{
        Vector2 aim = GetGlobalMousePosition();
        if (State == UnitState.Normal)
        {
            Velocity = new Vector2
            (
                Input.GetActionStrength("player_right") - Input.GetActionStrength("player_left"),
                Input.GetActionStrength("player_down") - Input.GetActionStrength("player_up")
            ).Normalized() * Speed;
        }
        LookAtPosition = aim;
        base._Process(delta);
		if (Input.IsActionPressed("player_action_1"))
			AttackWithCooldown(aim);
	}

	protected override void PerformAttack(Vector2 aim)
    {
        FireProjectile(aim);
	}

}
