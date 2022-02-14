using Godot;

public class HUD : Control
{
    public Player Player
    {
        set
        {
            _player = value;
            UpdatePlayerHealthBar();
            _player.Connect(nameof(Unit.OnTookHitSignal), this, "UpdatePlayerHealthBar");
        }
    }

    private Sprite _playerHealthBar;
    private Player _player;

    public override void _Ready()
    {
        _playerHealthBar = GetNode<Sprite>("PlayerHealthBar");
    }


    private void UpdatePlayerHealthBar()
    {
        _playerHealthBar.RegionRect = new Rect2(_playerHealthBar.Position, new Vector2(16 * _player.Health, 16));
    }

}
