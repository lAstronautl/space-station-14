using Content.Server.Administration.Managers;
using Content.Server.Afk.Events;
using Content.Server.GameTicking;
using Content.Server.EUI;
using Content.Shared.CCVar;
using Content.Shared.Afk;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Player;
using Robust.Shared.Timing;


namespace Content.Server.Afk;

/// <summary>
/// Actively checks for AFK players regularly and issues an event whenever they go afk.
/// </summary>
public sealed class AFKSystem : EntitySystem
{
    [Dependency] private readonly IAfkManager _afkManager = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly EuiManager _euiManager = null!;
    [Dependency] private readonly IAdminManager _adminManager = default!;

    private float _checkDelay;
    private float _kickDelay;
    private float _kickAdminDelay;
    private TimeSpan _checkTime;

    private readonly Dictionary<ICommonSession, TimeSpan> _afkPlayers = new();

    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += OnPlayerChange;
        _configManager.OnValueChanged(CCVars.AfkTime, SetAfkDelay, true);
        _configManager.OnValueChanged(CCVars.AfkKickTime, SetAfkKickDelay, true);
        _configManager.OnValueChanged(CCVars.AfkAdminKickTime, SetAfkAdminKickDelay, true);

        SubscribeNetworkEvent<FullInputCmdMessage>(HandleInputCmd);
    }

    private void HandleInputCmd(FullInputCmdMessage msg, EntitySessionEventArgs args)
    {
        _afkManager.PlayerDidAction(args.SenderSession);
    }

    private void SetAfkDelay(float obj)
    {
        _checkDelay = obj;
    }

    private void SetAfkKickDelay(float obj)
    {
        if(obj < 60.0f)
            obj = 60.0f;

        _kickDelay = obj;
    }

    private void SetAfkAdminKickDelay(float obj)
    {
        if(obj < 60.0f)
            obj = 60.0f;

        _kickAdminDelay = obj;
    }

    private void OnPlayerChange(object? sender, SessionStatusEventArgs e)
    {
        switch (e.NewStatus)
        {
            case SessionStatus.Disconnected:
                _afkPlayers.Remove(e.Session);
                break;
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _afkPlayers.Clear();
        _playerManager.PlayerStatusChanged -= OnPlayerChange;
        _configManager.UnsubValueChanged(CCVars.AfkTime, SetAfkDelay);
        _configManager.UnsubValueChanged(CCVars.AfkKickTime, SetAfkKickDelay);
        _configManager.UnsubValueChanged(CCVars.AfkAdminKickTime, SetAfkAdminKickDelay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_ticker.RunLevel != GameRunLevel.InRound)
        {
            _afkPlayers.Clear();
            _checkTime = TimeSpan.Zero;
            return;
        }

        // TODO: Should also listen to the input events for more accurate timings.
        if (_timing.CurTime < _checkTime)
            return;

        _checkTime = _timing.CurTime + TimeSpan.FromSeconds(_checkDelay);

        foreach (var pSession in Filter.GetAllPlayers())
        {
            if (pSession.Status != SessionStatus.InGame) continue;
            var isAfk = _afkManager.IsAfk(pSession);
            var isAdmin = _adminManager.IsAdmin(pSession);

            if (isAfk && _afkPlayers.TryAdd(pSession, _timing.CurTime))
            {
                var ev = new AFKEvent(pSession);
                RaiseLocalEvent(ref ev);
            }

            if (!isAfk && _afkPlayers.Remove(pSession))
            {
                var ev = new UnAFKEvent(pSession);
                RaiseLocalEvent(ref ev);
            }

            if (isAfk && _afkPlayers.TryGetValue(pSession, out var startAfkTime))
            {
                if (((_timing.CurTime - startAfkTime >= TimeSpan.FromSeconds(_kickDelay) && !isAdmin) ||
                    (_timing.CurTime - startAfkTime >= TimeSpan.FromSeconds(_kickAdminDelay) && isAdmin)))
                {
                    pSession.ConnectedClient.Disconnect(Loc.GetString("afk-system-kick-reason"));
                    continue;
                }

                if(((_timing.CurTime - startAfkTime >= TimeSpan.FromSeconds(_kickDelay - 60) && !isAdmin) ||
                    (_timing.CurTime - startAfkTime >= TimeSpan.FromSeconds(_kickAdminDelay - 60) && isAdmin)))
                {
                    _euiManager.OpenEui(new AfkCheckEui(pSession, _afkManager), pSession);
                }
            }
        }
    }
}
