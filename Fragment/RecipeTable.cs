using Godot;
using System;
using System.Threading.Tasks;

public partial class RecipeTable : Node
{
    [Export] public Recipe[] Recipes = Array.Empty<Recipe>();
    private CraftingEffectController CraftingEffectController => field ??= GetNode<CraftingEffectController>("%CraftingEffectController");
    private bool _craftingInProgress = false;

    public override void _Ready()
    {
        AddToGroup("RecipeTable");
    }

    public async void TryCraft(Fragment fragmentA, Fragment fragmentB)
    {
        if (_craftingInProgress) return;
        if (!IsInstanceValid(fragmentA) || !IsInstanceValid(fragmentB)) return;
        if (fragmentA.GetInstanceId() <= fragmentB.GetInstanceId()) return;

        var resultScene = GetCraftingResult(fragmentA.FragmentName, fragmentB.FragmentName);
        if (resultScene == null) return;

        var resultInstance = resultScene.Instantiate<Fragment>();
        if (resultInstance == null) return;

        _craftingInProgress = true;
        try
        {
            await CraftingEffectController.PlayCraftingEffect(fragmentA, fragmentB, resultInstance);
        }
        finally
        {
            _craftingInProgress = false;
        }
    }

    public PackedScene GetCraftingResult(string fragmentA, string fragmentB)
    {
        PackedScene result = null;
        foreach (var recipe in Recipes)
        {
            result = recipe.GetResult(fragmentA, fragmentB);
            if (result != null) break;
        }
        return result;
    }
}
