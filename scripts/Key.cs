using System;
using Godot;

public class Key : Area2D
{
    [Export] private bool _finalKey = false;
    public Door Door { get; set; }

    private AudioStreamPlayer2D _audio;

    public override void _Ready()
    {
        _audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    private async void OnPickedUp(Player player)
    {
        _audio.Play();
        if (_finalKey)
        {
            GetNode<Game>("/root/Game").Victory();
        }
        else
        {
            player.FreezeFor(TimeSpan.FromSeconds(2.5f));
            Door.Unlock();
        }
        await ToSignal(_audio, "finished");
        QueueFree();
    }
}
