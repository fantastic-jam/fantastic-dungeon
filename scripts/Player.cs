using Godot;

public class Player : Unit
{
    public override void _Process(float delta)
    {
        HandleAim();
        HandleMovementInputs();
        base._Process(delta);
		if (Input.IsActionPressed("player_action_1"))
			AttackWithCooldown(LookAtPosition);
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && motionEvent.Relative != Vector2.Zero && Input.GetMouseMode() == Input.MouseMode.Hidden)
            Input.SetMouseMode(Input.MouseMode.Confined);
    }

    // We cannot wrap mouse position to controller aim since it is not supported in web browsers
    // Input.WarpMousePosition(GetGlobalTransformWithCanvas().origin + controllerAimPosition * 200f);
    private void HandleAim()
    {
        Vector2 controllerAimPosition = InputGetVector("player_aim_left", "player_aim_right", "player_aim_up", "player_aim_down").Normalized();
        if (controllerAimPosition.Length() > float.Epsilon)
        {
            Input.SetMouseMode(Input.MouseMode.Hidden);
            LookAtPosition = GlobalPosition + controllerAimPosition * 200f;
        }
        else if (Input.GetMouseMode() != Input.MouseMode.Hidden)
        {
            LookAtPosition = GetGlobalMousePosition();
        }
    }

    private void HandleMovementInputs()
    {
        if (State != UnitState.Normal)
            return;
        Velocity = InputGetVector("player_left", "player_right", "player_up", "player_down").Normalized() * Speed;
    }

    // We cannot use Input.GetVector since there is an issue in godot when exporting in HTML5
    // https://github.com/godotengine/godot/issues/58168
    private Vector2 InputGetVector(string negativeX, string positiveX, string negativeY, string positiveY, float deadzone = 0.5f)
    {
        var strength = new Vector2(
            Input.GetActionStrength(positiveX) - Input.GetActionStrength(negativeX),
            Input.GetActionStrength(positiveY) - Input.GetActionStrength(negativeY)
        );
        return strength.Length() > deadzone ? strength : Vector2.Zero;
    }

    protected override void PerformAttack(Vector2 aim)
    {
        FireProjectile(aim);
	}

}
