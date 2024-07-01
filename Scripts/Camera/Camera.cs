using Godot;
using Menus.Settings;

namespace Camera;

public partial class Camera : Camera2D {
	private Timer _shakeTimer;

	/// <summary>
	/// How fast the camera will move to its desired position.
	/// </summary>
	[Export]
	private float _shakeSpeed;

	/// <summary>
	/// How far the camera can move from its original position.
	/// </summary>
	private float _shakeIntensity;

	public override void _Ready() {
		_shakeTimer = GetNode<Timer>("ShakeTimer");
	}

	public override void _PhysicsProcess(double delta) {
		if (_shakeTimer.TimeLeft > 0) Shake();
	}

	public void SetLimits(int bottom, int left, int right, int top) {
		LimitBottom = bottom;
		LimitLeft = left;
		LimitRight = right;
		LimitTop = top;
	}

	public void StartShake(float intensity, float duration) {
		_shakeIntensity = intensity;
		_shakeTimer.WaitTime = duration;
		_shakeTimer.Start();
	}

	private void Shake() {
		if (!Settings.ScreenShakeEnabled) return;

		Vector2 desiredPosition = new(
			x: (float) GD.RandRange(-_shakeIntensity, _shakeIntensity),
			y: (float) GD.RandRange(-_shakeIntensity, _shakeIntensity)
		);
		Offset = Offset.Lerp(desiredPosition, _shakeSpeed);
	}

	public void StopShaking() {
		Offset = Vector2.Zero;
		_shakeTimer.Stop();
	}
}
