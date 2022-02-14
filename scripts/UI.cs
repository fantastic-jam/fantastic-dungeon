using Godot;

public class UI : CanvasLayer
{
    private Game _game;
    private Popup _menu;

    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Game");
        _menu = GetNode<Popup>("Menu");
        _menu.PopupCentered();
    }

    private void OnEasyButtonPressed()
    {
        _game.Start(Difficulty.Easy);
        _menu.Hide();
    }

    private void OnNormalButtonPressed()
    {
        _game.Start(Difficulty.Normal);
        _menu.Hide();
    }

    private void OnHardcoreButtonPressed()
    {
        _game.Start(Difficulty.Hardcore);
        _menu.Hide();
    }

    private void OnRestartButtonPressed()
    {
        GetTree().ReloadCurrentScene();
    }
}
