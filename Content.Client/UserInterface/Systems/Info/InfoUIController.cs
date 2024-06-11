using Content.Client.Gameplay;
using Content.Client.Info;
using Content.Shared.CCVar;
using Content.Shared.Guidebook;
using Content.Shared.Info;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Info;

public sealed class InfoUIController : UIController, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public RulesPopup? RulesPopup;
    private RulesAndInfoWindow? _infoWindow;

    public event Action? Accepted;

    public override void Initialize()
    {
        base.Initialize();


        _netManager.RegisterNetMessage<RulesAcceptedMessage>();
        _netManager.RegisterNetMessage<ShowRulesPopupMessage>(OnShowRulesPopupMessage);

        _consoleHost.RegisterCommand("fuckrules",
            "",
            "",
            (_, _, _) =>
        {
            OnAcceptPressed();
        });
    }

    private void OnShowRulesPopupMessage(ShowRulesPopupMessage message)
    {
        ShowRules(message.PopupTime);
    }

    public void OnStateExited(GameplayState state)
    {
        if (_infoWindow == null)
            return;

        _infoWindow.Dispose();
        _infoWindow = null;
    }

    private void ShowRules(float time)
    {
        if (RulesPopup != null)
            return;

        RulesPopup = new RulesPopup
        {
            Timer = time
        };

        RulesPopup.OnQuitPressed += OnQuitPressed;
        RulesPopup.OnAcceptPressed += OnAcceptPressed;
        UIManager.WindowRoot.AddChild(RulesPopup);
        LayoutContainer.SetAnchorPreset(RulesPopup, LayoutContainer.LayoutPreset.Wide);
    }

    private void OnQuitPressed()
    {
        _consoleHost.ExecuteCommand("quit");
    }

    private void OnAcceptPressed()
    {
        _netManager.ClientSendMessage(new RulesAcceptedMessage());

        RulesPopup?.Orphan();
        RulesPopup = null;
        Accepted?.Invoke();
    }

    public GuideEntryPrototype GetCoreRuleEntry()
    {
        var guide = _cfg.GetCVar(CCVars.RulesFile);
        var guideEntryPrototype = _prototype.Index<GuideEntryPrototype>(guide);
        return guideEntryPrototype;
    }

    public void OpenWindow()
    {
        if (_infoWindow == null || _infoWindow.Disposed)
            _infoWindow = UIManager.CreateWindow<RulesAndInfoWindow>();

        _infoWindow?.OpenCentered();
    }
}
