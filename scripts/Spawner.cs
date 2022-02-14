using Godot;

public class Spawner : Position2D
{
    [Export] private PackedScene _unitPrefab;
    [Export] private PackedScene _spawnEffectPrefab;

    private Game _game;
    private Node2D _map;
    private Unit _unit;
    private Node2D _effect;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Game");
        _map = GetNode<Node2D>("/root/Game/Map");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public Unit Spawn()
    {
        _unit = _unitPrefab.Instance<Unit>();
        _unit.GlobalPosition = GlobalPosition;
        _unit.AttackCooldownModifier = _game.Difficulty.EnemyAttackCooldownModifier;
        _effect = _spawnEffectPrefab.Instance<Node2D>();
        _effect.GlobalPosition = GlobalPosition;
        _animationPlayer.Play("spawn");
        return _unit;
    }

    private void SpawnEffect()
    {
        _map.AddChild(_effect);
    }

    private void SpawnUnit()
    {
        _effect.QueueFree();
        _map.AddChild(_unit);
        QueueFree();
    }
}
