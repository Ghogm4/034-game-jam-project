using Godot;
using System;

public partial class Player_PlayerState : State
{
	protected Player Player => field ??= Owner as Player;
}