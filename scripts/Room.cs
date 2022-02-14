using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class Room : Node2D
{
    [Export(PropertyHint.Range, "0,15,1")] private float _startDelayInSeconds = 5;
    [Export(PropertyHint.Range, "0,15,1")] private float _nextRoundDelayInSeconds = 5;
    [Export] private NodePath _mapPath;
    [Export] private PackedScene _keyPrefab;

    private Node2D _map;
    private Door _door;
    private Node _rounds;
    private Node[] _roundNodes;
    private int _roundIdx = -1;
    private int _remainingUnits = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _map = GetNode<Node2D>(_mapPath);
        _door = GetNode<Door>("Door");
        _rounds = GetNode("Rounds");
        _roundNodes = _rounds.GetChildren().OfType<Node>().ToArray();
    }

    private void OnBodyEnteredTrigger(Player player)
    {
        GetNode<Area2D>("Trigger").QueueFree();
        StartRounds();
    }

    private async void StartRounds()
    {
        if (_roundNodes.Length == 0)
            throw new InvalidOperationException("No rounds found in room");
        await ToSignal(GetTree().CreateTimer(_startDelayInSeconds), "timeout");
        NextRound();
    }

    private async void NextRound()
    {
        _roundIdx++;
        if (_roundIdx >= _roundNodes.Length)
        {
            EndRoom();
            return;
        }
        await ToSignal(GetTree().CreateTimer(_nextRoundDelayInSeconds), "timeout");
        Node round = _roundNodes[_roundIdx];
        List<Spawner> spawners = round.GetChildren().OfType<Spawner>().ToList();
        foreach (Spawner spawner in spawners)
            spawner.Spawn().Connect(nameof(Unit.OnDeathSignal), this, "OnUnitDeath");
        _remainingUnits = spawners.Count;

    }

    private void OnUnitDeath()
    {
        if (Interlocked.Decrement(ref _remainingUnits) == 0)
            NextRound();
    }

    private async void EndRoom()
    {
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        var key = _keyPrefab.Instance<Key>();
        key.Door = _door;
        key.GlobalPosition = GlobalPosition;
        _map.AddChild(key);
        _roundNodes = null;
        _rounds.QueueFree();
    }

}
