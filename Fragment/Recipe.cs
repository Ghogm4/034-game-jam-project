using Godot;
using System;
[GlobalClass]
public partial class Recipe : Resource
{
    [Export] private string Input1 = "";
    [Export] private string Input2 = "";
    [Export] private PackedScene Result = null;
    public PackedScene GetResult(string input1, string input2)
    {
        if ((input1 == Input1 && input2 == Input2) || (input1 == Input2 && input2 == Input1))
        {
            return Result;
        }
        return null;
    }
}
