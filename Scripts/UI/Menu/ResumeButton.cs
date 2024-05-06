using Godot;
using System;

public partial class ResumeButton : Button {
	
	private string SaveFile = null;
	public override void _Ready() {
		SaveFile = GetSave();
		if (SaveFile == null)
			Disabled = false;
	}


	/// <summary>
	/// Checks if there is a valid save file.
	/// </summary>
	/// <returns>Returns true if there is a valid save file, otherwise returns false.</returns>
	public string GetSave() {
		return null;
	}

}
