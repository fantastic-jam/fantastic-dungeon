using System;
using Godot;

public class Game : Node2D
{
    public Difficulty Difficulty { get; private set; }

    [Export] private PackedScene _playerPrefab;
    [Export] private NodePath _playerStartPositionPath;
    [Export] private NodePath _hudPath;
    [Export] private NodePath _deathPopupPath;
    [Export] private NodePath _victoryPopupPath;

    private Position2D _playerStartPosition;
    private Player _player;
    private HUD _hud;
    private Popup _deathPopup;
    private Popup _victoryPopup;

    public override void _Ready()
    {
        _playerStartPosition = GetNode<Position2D>(_playerStartPositionPath);
        _hud = GetNode<HUD>(_hudPath);
        _deathPopup = GetNode<Popup>(_deathPopupPath);
        _victoryPopup = GetNode<Popup>(_victoryPopupPath);
    }

    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Escape))
            GetTree().Quit();
    }

    public void Start(Difficulty difficulty)
    {
        Difficulty = difficulty;
        _player = _playerPrefab.Instance<Player>();
        _player.Name = "Player";
        _player.GlobalPosition = _playerStartPosition.GlobalPosition;
        _player.Health = difficulty.PlayerHealth;
        _player.AttackDamage = difficulty.PlayerAttackDamage;
        _player.Connect(nameof(Unit.OnDeathSignal), this, "GameOver");
        _hud.Player = _player;
        _playerStartPosition.ReplaceBy(_player);
        _playerStartPosition.QueueFree();
    }

    private async void GameOver()
    {
        await ToSignal(GetTree().CreateTimer(3), "timeout");
        _deathPopup.PopupCentered();
    }

    public async void Victory()
    {
        await _player.FreezeFor(TimeSpan.FromSeconds(3));
        _victoryPopup.PopupCentered();
        _player.QueueFree();
    }
}

public class Difficulty
{
    public int PlayerHealth { get; }
    public int PlayerAttackDamage { get; }
    public float EnemyAttackCooldownModifier { get; }

    private Difficulty(int playerHealth, int playerAttackDamage, float enemyAttackCooldownModifier)
    {
        PlayerHealth = playerHealth;
        PlayerAttackDamage = playerAttackDamage;
        EnemyAttackCooldownModifier = enemyAttackCooldownModifier;
    }

    public static Difficulty Easy = new Difficulty(7, 15, 1.2f);
    public static Difficulty Normal = new Difficulty(3, 7, 0.75f);
    public static Difficulty Hardcore = new Difficulty(1, 4, 0.45f);
}
