using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public abstract class Unit : KinematicBody2D
{
    private const string ExportSeparator = "----------------------------------";

    [Export] private NodePath _mapPath = new NodePath("/root/Game/Map");
    [Export] private AudioStream _attackSound;
    [Export] private AudioStream _hurtSound;

    [Export] private string P_R_O_J_E_C_T_I_L_E = ExportSeparator;
    [Export] private PackedScene _projectilePrefab;
    [Export] private Color _projectileColor;
    [Export] private Vector2 _projectileScale;
    [Export] private float _projectileSpeed;

    [Export] private string A_T_T_R_I_B_U_T_E_S = ExportSeparator;
    [Export] protected float Speed = 0f;
    [Export] private int _health;
    [Export] private int _attackDamage;
    [Export] private ulong _attackCooldown = 0;
    [Export] private bool _attackOnTargetCollision = false;
    [Export] private ulong _hitCooldown = 1000;
    [Export] private float _bumpStrength = 15f;

    [Export] private string F_L_O_C_K_I_N_G = ExportSeparator;
    [Export] private bool _flockingEnabled = false;
    [Export] private float _cohesionForce = 0.1f;
    [Export] private float _alignForce = 0.1f;
    [Export] private float _separationForce = 0.5f;
    [Export] private float _neighbourDistance = 30.0f;
    [Export] private float _maxForce = 30f;

    public int Health { get => _health; set => _health = value; }
    public int AttackDamage { set => _attackDamage = value; }

    public float AttackCooldownModifier { set => _attackCooldown = (ulong)(_attackCooldown * value); }

    public UnitState State { get; private set; } = UnitState.Normal;

    protected Node2D Map;
    private AnimationPlayer _effectsAnimationPlayer;
    private AnimatedSprite _animatedSprite;
    private AudioStreamPlayer2D _audio;

    protected Vector2 Velocity = Vector2.Zero;
    protected Vector2 WalkToPosition = Vector2.Zero;
    protected Unit Target = null;
    protected Vector2 LookAtPosition;

    private ulong _nextAttackTime = 0;
    private ulong _nextHitTime = 0;

    [Signal] public delegate void OnTookHitSignal();
    [Signal] public delegate void OnDeathSignal();

    public override void _Ready()
    {
        Map = GetNode<Node2D>(_mapPath);
        _animatedSprite = HasNode("AnimatedSprite") ? GetNode<AnimatedSprite>("AnimatedSprite") : null;
        _effectsAnimationPlayer = HasNode("EffectsAnimationPlayer") ? GetNode<AnimationPlayer>("EffectsAnimationPlayer") : null;
        _audio = HasNode("AudioStreamPlayer2D") ? GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D") : null;
    }

    public override void _Process(float delta)
    {
        if (State == UnitState.Dead || State == UnitState.Bump)
            return;
        if (LookAtPosition != Vector2.Zero && _animatedSprite != null)
        {
            Vector2 lookDirection = Position.DirectionTo(LookAtPosition);
            if (lookDirection.x < -float.Epsilon)
                _animatedSprite.FlipH = true;
            else if (lookDirection.x > float.Epsilon)
                _animatedSprite.FlipH = false;
        }
        if (Target != null)
        {
            if (Target.State != UnitState.Dead)
            {
                Velocity = GlobalPosition.DirectionTo(Target.GlobalPosition).Normalized() * Speed;
                if (_flockingEnabled)
                {
                    List<Unit> units = Map.GetChildren().OfType<Unit>().Where(unit => unit != Target && unit != this).ToList();
                    Velocity += Flocking(units);
                }
            }
            else
            {
                Target = null;
                Velocity = Vector2.Zero;
            }
        }
        if (_animatedSprite != null)
            _animatedSprite.Animation = Velocity.LengthSquared() > float.Epsilon || WalkToPosition != Vector2.Zero ? "running" : "idle";
    }

    public override void _PhysicsProcess(float delta)
    {
        if (State != UnitState.Normal && State != UnitState.CutScene && State != UnitState.Bump)
            return;
        if (WalkToPosition != Vector2.Zero)
        {
            Position = Position.MoveToward(WalkToPosition, delta * (Speed / 3.0f));
            if ((Position - WalkToPosition).Length() < float.Epsilon)
            {
                WalkToPosition = Vector2.Zero;
                State = UnitState.Normal;
            }
        }
        else if (Velocity != Vector2.Zero)
        {
            Velocity = MoveAndSlide(Velocity);
            if (_attackOnTargetCollision && Target != null)
            {
                for (var i = 0; i < GetSlideCount(); i++)
                {
                    KinematicCollision2D collision = GetSlideCollision(i);
                    if (collision?.Collider == Target)
                    {
                        Target?.TakeHit(this, _attackDamage);
                        break;
                    }
                }
            }
        }
    }

    #region Attack

    protected void AttackWithCooldown(Vector2 aim)
    {
        if (State != UnitState.Normal)
            return;
        var time = OS.GetTicksMsec();
        if (_nextAttackTime <= time)
        {
            if (_audio != null && _attackSound != null)
            {
                _audio.Stream = _attackSound;
                _audio.Play();
            }
            PerformAttack(aim);
            _nextAttackTime = time + _attackCooldown;
        }
    }

    protected void FireProjectile(Vector2 aim)
    {
        var projectile = _projectilePrefab.Instance<Projectile>();
        projectile.Modulate = _projectileColor;
        projectile.Scale = _projectileScale;
        projectile.SpriteRotation = GetAngleTo(aim);
        projectile.GlobalPosition = GlobalPosition + GlobalPosition.DirectionTo(aim).Normalized();
        projectile.Speed = _projectileSpeed;
        projectile.Damage = _attackDamage;
        Map.AddChild(projectile);
    }

    protected virtual void PerformAttack(Vector2 aim)
    {
    }

    public void TakeHit(Node2D source, int damage)
    {
        var time = OS.GetTicksMsec();
        if (_nextHitTime > time) return;
        _nextHitTime = time + _hitCooldown;
        _health -= damage;
        if (_audio != null && _hurtSound != null)
        {
            _audio.Stream = _hurtSound;
            _audio.Play();
        }
        EmitSignal(nameof(OnTookHitSignal));
        if (_health <= 0)
        {
            if (_effectsAnimationPlayer != null && _effectsAnimationPlayer.HasAnimation("death"))
                _effectsAnimationPlayer?.Play("death");
            State = UnitState.Dead;
            EmitSignal(nameof(OnDeathSignal));
            return;
        }
        Bump(source.GlobalPosition.DirectionTo(GlobalPosition).Normalized() * _bumpStrength);
        if (_effectsAnimationPlayer != null && _effectsAnimationPlayer.HasAnimation("hit"))
            _effectsAnimationPlayer.Play("hit");
    }

    #endregion

    #region Movement

    public void WalkTo(Vector2 position)
    {
        State = UnitState.CutScene;
        WalkToPosition = position;
    }

    private async void Bump(Vector2 force)
    {
        State = UnitState.Bump;
        Velocity = force;
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        State = UnitState.Normal;
    }

    public async Task FreezeFor(TimeSpan duration)
    {
        State = UnitState.CutScene;
        Velocity = Vector2.Zero;
        await ToSignal(GetTree().CreateTimer((float)duration.TotalSeconds), "timeout");
        State = UnitState.Normal;
    }

    #endregion

    #region Flocking

    private Vector2 Flocking(List<Unit> units)
    {
        // TODO: Single iteration to compute all forces
        return Separate(units) * _separationForce + Align(units) * _alignForce + Cohesion(units) * _cohesionForce;
    }

    private Vector2 Separate(List<Unit> units) {
        if (units.Count == 0)
            return Vector2.Zero;
        Vector2 force = Vector2.Zero;
        foreach (Unit unit in units)
        {
            var distance = GlobalPosition.DistanceTo(unit.GlobalPosition);
            if (distance > 0 && distance < _neighbourDistance)
                force += (GlobalPosition - unit.GlobalPosition).Normalized() / distance;
        }
        return ((force / units.Count).Normalized() * Speed - Velocity).Clamped(_maxForce);
    }

    private Vector2 Align(List<Unit> units)
    {
        if (units.Count == 0)
            return Vector2.Zero;
        Vector2 force = Vector2.Zero;
        foreach (Unit unit in units)
        {
            var distance = GlobalPosition.DistanceTo(unit.GlobalPosition);
            if (distance > 0 && distance < _neighbourDistance) {
                force += unit.Velocity;
            }
        }
        return ((force / units.Count).Normalized() * Speed - Velocity).Clamped(_maxForce);
    }

    private Vector2 Cohesion(List<Unit> units)
    {
        if (units.Count == 0)
            return Vector2.Zero;
        Vector2 force = Vector2.Zero;
        foreach (Unit unit in units)
        {
            float distance = GlobalPosition.DistanceTo(unit.GlobalPosition);
            if (distance > 0 && distance < _neighbourDistance) {
                force += unit.GlobalPosition;
            }
        }
        return ((force / units.Count).Normalized() * Speed - Velocity).Clamped(_maxForce);
    }

    #endregion
}

public enum UnitState
{
    Normal,
    Bump,
    CutScene,
    Dead
}
