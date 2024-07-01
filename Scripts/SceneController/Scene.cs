#nullable enable
using Godot;
using Players;

namespace SceneController;

public partial class Scene : Node {
    [Export]
    private Color _backgroundLightColor = Colors.White;

    /// <summary> This music will start playing when the scene is loaded. Leave null to keep the previous music. </summary>
    [Export]
    public AudioStream? BackgroundMusic = null;

    [ExportGroup("Limits")]
    [Export] private int LimitLeft = -10000000;
    [Export] private int LimitTop = -10000000;
    [Export] private int LimitRight = 10000000;
	[Export] private int LimitBottom = 10000000;

    public override void _Ready() {
        BaseScene.Camera.SetLimits(bottom: LimitBottom, left: LimitLeft, right: LimitRight, top: LimitTop);
        foreach (Player player in BaseScene.Players) {
            player.GetNode<PointLight2D>("Light").Color = _backgroundLightColor;
        }
    }
}
