using Godot;

public class Door : Node
{
	[Export] private NodePath _nextRoomFogPath;
	[Export] private NodePath _previousRoomFogPath;

	private AnimatedSprite _animatedSprite;
	private CollisionShape2D _collisionShape;
	private Area2D _doorStep;
	private RoomFog _nextRoomFog;
	private RoomFog _previousRoomFog;

	public override void _Ready()
	{
		_animatedSprite = HasNode("AnimatedSprite") ? GetNode<AnimatedSprite>("AnimatedSprite") : null;
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_doorStep = GetNode<Area2D>("DoorStep");
		_nextRoomFog = GetNode<RoomFog>(_nextRoomFogPath);
		_previousRoomFog = GetNode<RoomFog>(_previousRoomFogPath);
	}

	public void Unlock()
	{
		_animatedSprite?.Play("unlocking");
		_nextRoomFog.Dispel();
		_collisionShape.SetDeferred("disabled", true);
	}

	private void Lock()
	{
		_animatedSprite?.Play("locking");
		_previousRoomFog.Show();
		_collisionShape.SetDeferred("disabled", false);
	}

	private void OnBodyEnteredDoorStep(Player player)
	{
		player.WalkTo(_nextRoomFog.GlobalPosition);
		Lock();
		_doorStep.QueueFree();
	}
}
