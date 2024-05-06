using Godot;
using System;

public partial class MenuManager : Control {
	
	
	/// <summary>
	/// Method called when the Credits button is pressed. Changes the scene to the Credits scene.
	/// </summary>
	public void OnPressedCredits(){
		GetTree().ChangeSceneToFile("res://Scenes/UI/Credits.tscn");
	}
	
	/// <summary>
	/// Method called when the New Game button is pressed. Changes the scene to the Starting Room scene.
	/// </summary>
	public void OnPressedNewGame(){
		GetTree().ChangeSceneToFile("res://Scenes/Base.tscn");
	}
	
	/// <summary>
	/// Method called when the Config button is pressed. Changes the scene to the Config scene.
	/// </summary>
	public void OnPressedConfig(){
		GetTree().ChangeSceneToFile("res://Scenes/UI/Config.tscn");
	}
	
	/// <summary>
	/// Method called when the Exit button is pressed. Quits the game.
	/// </summary>
	public void OnPressedExit(){
		GetTree().Quit();
	}
	
	/// <summary>
	/// Method called when the Resume button is pressed. Resume the game loading the configurations
	/// from the save file.
	/// </summary>
	public void OnPressedResume(){
		var ResumeButton = GetNode<Button>("MarginContainer/VBoxContainer/Resume");
		//TODO resume game
	}
	
}
