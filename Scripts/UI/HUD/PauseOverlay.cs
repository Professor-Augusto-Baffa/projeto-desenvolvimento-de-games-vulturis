using Godot;
using HUD;
using SceneController;

namespace Menus.Overlays;

public partial class PauseOverlay : FocusController {
    private NinePatchRect _border;
    private AnimationPlayer _colorRectAnimationPlayer;
    private FocusController _buttonsFocusController;

	[Export(PropertyHint.File, "*.tscn")]
	private string _settingsScenePath;

    private bool _paused = false;

    public override void _Ready() {
        base._Ready();
        Visible = false;
        _border = GetNode<NinePatchRect>("Border");
        _colorRectAnimationPlayer = GetNode<AnimationPlayer>("ColorRect/AnimationPlayer");
    }

    public override void _Input(InputEvent @event) {
        base._Input(@event);
        if (!_paused && _activatedFocus) {
            // The following loop blocks pausing if another UI element is active.
            foreach (Node node in BaseScene.UICanvas.GetChildren()) {
                // When new default UI elements are added to the BaseScene they should be also added here.
                if (node is not PauseOverlay && node is not HealthBar && node is not AmmoBar) {
                    GD.Print("The game could not be paused because the " + node.Name + " UI element in the BaseScene canvas is preventing it.");
                    return;
                }
            }

            Pause();
        }
    }

    private async void Pause() {
        GetTree().Paused = true;
        _paused = true;
        Visible = true;
        _border.Visible = false;

        _colorRectAnimationPlayer.Play("fade");
        await ToSignal(_colorRectAnimationPlayer, "animation_finished");

        _border.Visible = true;
        GrabFocus();
    }

    public void OnContinueButtonPressed() {
        if (!_paused) return;

        GetTree().Paused = false;
        Visible = false;
        _activatedFocus = false;
        _paused = false;
    }

    public void OnSettingsButtonPressed() {
        if (!_paused) return;

		PackedScene prefab = GD.Load<PackedScene>(_settingsScenePath);
        Settings.Settings scene = prefab.Instantiate<Settings.Settings>();
        scene.GetNode("SettingsBorder/CreditsButtonBorder").QueueFree();

		BaseScene.UICanvas.AddChild(scene);
        _activatedFocus = false;
    }
}
