using Godot;
using System;
using System.Threading.Tasks;

public partial class CraftingEffectController : Node
{
    [Export] public PackedScene BlurOverlayScene = null;
    [Export] public float IntroDuration = 0.35f;
    [Export] public float ShakeDuration = 0.35f;
    [Export] public float OutroDuration = 0.45f;
    [Export] public float PeakBlurAmount = 3.5f;
    [Export] public float CenterSpacing = 90.0f;
    [Export] public float EnlargedScaleMultiplier = 1.7f;
    [Export] public float ResultScaleMultiplier = 2.0f;
    [Export] public float ShakeAmplitude = 18.0f;

    private CanvasLayer BlurOverlay => field ??= GetTree().GetFirstNodeInGroup("BlurOverlay") as CanvasLayer;
    private ColorRect HorizontalBlur => field ??= BlurOverlay?.GetNode<ColorRect>("HorizontalBlur");
    private ColorRect VerticalBlur => field ??= BlurOverlay?.GetNode<ColorRect>("VerticalBlur");
    private ShaderMaterial HorizontalMaterial => field ??= HorizontalBlur?.Material as ShaderMaterial;
    private ShaderMaterial VerticalMaterial => field ??= VerticalBlur?.Material as ShaderMaterial;


    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public async Task PlayCraftingEffect(Fragment fragmentA, Fragment fragmentB, Fragment resultInstance)
    {
        SceneTree tree = GetTree();
        Node parentNode = tree.CurrentScene ?? tree.Root;
        Vector2 worldMidpoint = (fragmentA.GlobalPosition + fragmentB.GlobalPosition) * 0.5f;
        Vector2 screenPosA = GetScreenPosition(fragmentA);
        Vector2 screenPosB = GetScreenPosition(fragmentB);
        Vector2 targetScreenPos = screenPosA.Lerp(screenPosB, 0.5f);
        Vector2 screenCenter = GetViewport().GetVisibleRect().GetCenter();
        Vector2 centerPosA = screenCenter + Vector2.Left * CenterSpacing;
        Vector2 centerPosB = screenCenter + Vector2.Right * CenterSpacing;

        CanvasLayer visualLayer = null;
        Sprite2D spriteCloneA = null;
        Sprite2D spriteCloneB = null;
        Sprite2D resultClone = null;

        visualLayer = CreateVisualLayer(parentNode);

        spriteCloneA = CreateCloneSprite(fragmentA.VisualSprite, screenPosA);
        spriteCloneB = CreateCloneSprite(fragmentB.VisualSprite, screenPosB);
        resultClone = CreateCloneSprite(resultInstance.VisualSprite, screenCenter);
        resultClone.Visible = false;

        visualLayer.AddChild(spriteCloneA);
        visualLayer.AddChild(spriteCloneB);
        visualLayer.AddChild(resultClone);

        tree.Paused = true;

        Vector2 originalSpriteAScale = spriteCloneA.Scale;
        Vector2 originalSpriteBScale = spriteCloneB.Scale;
        await Animate(IntroDuration, progress =>
        {
            float eased = Ease(progress);
            SetBlur(Mathf.Lerp(0.0f, PeakBlurAmount, eased), eased);
            spriteCloneA.Position = screenPosA.Lerp(centerPosA, eased);
            spriteCloneB.Position = screenPosB.Lerp(centerPosB, eased);
            spriteCloneA.Scale = originalSpriteAScale.Lerp(originalSpriteAScale * EnlargedScaleMultiplier, eased);
            spriteCloneB.Scale = originalSpriteBScale.Lerp(originalSpriteBScale * EnlargedScaleMultiplier, eased);
            // GD.Print("A Scale: " + spriteCloneA.Scale + " B Scale: " + spriteCloneB.Scale);
        });

        await Animate(ShakeDuration, progress =>
        {
            float falloff = 1.0f - progress;
            spriteCloneA.Position = centerPosA + RandomOffset(ShakeAmplitude * falloff);
            spriteCloneB.Position = centerPosB + RandomOffset(ShakeAmplitude * falloff);
        });

        spriteCloneA.Visible = false;
        spriteCloneB.Visible = false;
        resultClone.Visible = true;
        resultClone.Position = screenCenter;
        Vector2 originalResultScale = resultClone.Scale;
        resultClone.Scale = originalResultScale * ResultScaleMultiplier;

        AudioManager.Instance.PlaySFX("Connect");

        await Animate(OutroDuration, progress =>
        {
            float eased = Ease(progress);
            SetBlur(Mathf.Lerp(PeakBlurAmount, 0.0f, eased), 1.0f - eased);
            resultClone.Position = screenCenter.Lerp(targetScreenPos, eased);
            resultClone.Scale = originalResultScale * Mathf.Lerp(ResultScaleMultiplier, 1.0f, eased);
            // GD.Print("Result Scale: " + resultClone.Scale);
        });

        if (IsInstanceValid(resultInstance) && !resultInstance.IsInsideTree())
        {
            parentNode.AddChild(resultInstance);
            resultInstance.GlobalPosition = worldMidpoint;
            resultInstance.ResetToFloating();
        }

        tree.Paused = false;
        visualLayer?.QueueFree();
        fragmentA.QueueFree();
        fragmentB.QueueFree();
    }
    private CanvasLayer CreateVisualLayer(Node parentNode)
    {
        CanvasLayer visualLayer = new()
        {
            Layer = 100,
            ProcessMode = ProcessModeEnum.Always
        };
        parentNode.AddChild(visualLayer);
        return visualLayer;
    }

    private Sprite2D CreateCloneSprite(Sprite2D sourceSprite, Vector2 screenPosition)
    {
        // GD.Print(sourceSprite.Scale);
        Sprite2D clone = new Sprite2D()
        {
            Texture = sourceSprite?.Texture,
            Centered = true,
            Position = screenPosition,
            Scale = sourceSprite.Scale,
            Modulate = Colors.White
        };
        return clone;
    }

    private void SetBlur(float blurAmount, float alpha)
    {
        HorizontalMaterial.SetShaderParameter("blur_amount", blurAmount);
        VerticalMaterial.SetShaderParameter("blur_amount", blurAmount);
        HorizontalBlur.Modulate = new Color(1f, 1f, 1f, alpha);
        VerticalBlur.Modulate = new Color(1f, 1f, 1f, alpha);
    }

    private Vector2 GetScreenPosition(Node2D node)
    {
        return node.GetGlobalTransformWithCanvas().Origin;
    }

    private Vector2 GetScreenPosition(Vector2 worldPosition, Node2D referenceNode)
    {
        return referenceNode.GetCanvasTransform() * worldPosition;
    }

    private Vector2 RandomOffset(float amplitude)
    {
        return new Vector2(
            (float)GD.RandRange(-amplitude, amplitude),
            (float)GD.RandRange(-amplitude, amplitude)
        );
    }

    private async Task Animate(float duration, Action<float> update)
    {
        double startTime = Time.GetTicksUsec() / 1000000.0;
        while (true)
        {
            double currentTime = Time.GetTicksUsec() / 1000000.0;
            float progress = duration <= 0.0f ? 1.0f : Mathf.Clamp((float)((currentTime - startTime) / duration), 0.0f, 1.0f);
            update(progress);
            if (progress >= 1.0f)
            {
                break;
            }

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private float Ease(float t)
    {
        return t * t * (3.0f - 2.0f * t);
    }
}