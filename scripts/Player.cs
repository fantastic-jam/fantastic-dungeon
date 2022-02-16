using Godot;

public class Player : Unit
{
    public override void _Process(float delta)
    {
        Vector2 controllerAim = Input.GetVector("player_aim_left", "player_aim_right", "player_aim_up", "player_aim_down").Normalized();
        if (controllerAim.Length() > float.Epsilon)
            Input.WarpMousePosition(GetGlobalTransformWithCanvas().origin + controllerAim * 200.0f);
        if (State == UnitState.Normal)
            Velocity = Input.GetVector("player_left", "player_right", "player_up", "player_down").Normalized() * Speed;

        LookAtPosition = GetGlobalMousePosition();
        base._Process(delta);
		if (Input.IsActionPressed("player_action_1"))
			AttackWithCooldown(LookAtPosition);
	}

	protected override void PerformAttack(Vector2 aim)
    {
        FireProjectile(aim);
	}

}
