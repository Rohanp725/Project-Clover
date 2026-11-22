using UnityEngine;
using UnityEngine.UIElements;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// An icon of <see cref="IconShapes"/> drawn with Painter2D in the element's own box, in its USS <c>color</c>; cut
    /// lines are drawn in <c>--icon-cut</c> (the ink). Sharp at every scale, as no texture is involved. Added in code onto
    /// the empty <c>.icon</c> elements of a screen's tree.
    /// </summary>
    public class PaintedIcon : VisualElement
    {
        private static readonly CustomStyleProperty<Color> CutProperty = new CustomStyleProperty<Color>("--icon-cut");

        private string icon = "";
        private Color cut = new Color(0.04f, 0.035f, 0.08f, 0.55f);

        public PaintedIcon()
        {
            AddToClassList("icon");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Paint;
            RegisterCallback<CustomStyleResolvedEvent>(OnStyle);
        }

        public PaintedIcon(string icon) : this() => Icon = icon;

        /// <summary>The icon's key in <see cref="IconShapes"/>; "" draws nothing.</summary>
        public string Icon
        {
            get => icon;
            set
            {
                if (icon == (value ?? "")) return;
                icon = value ?? "";
                MarkDirtyRepaint();
            }
        }

        private void OnStyle(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(CutProperty, out var colour)) cut = colour;
            MarkDirtyRepaint();
        }

        private void Paint(MeshGenerationContext context)
        {
            if (string.IsNullOrEmpty(icon)) return;
            var box = contentRect;
            if (box.width <= 0f || box.height <= 0f) return;
            var shape = IconShapes.Get(icon);
            float scale = Mathf.Min(box.width, box.height) / shape.Box;
            var offset = new Vector2(box.x + (box.width - shape.Box * scale) * 0.5f, box.y + (box.height - shape.Box * scale) * 0.5f);
            var painter = context.painter2D;
            var tint = resolvedStyle.color;
            foreach (var layer in shape.Layers)
            {
                var colour = layer.Cut ? cut : tint;
                painter.BeginPath();
                Trace(painter, IconShapes.Steps(layer.Path), scale, offset);
                if (layer.Stroke > 0f)
                {
                    painter.strokeColor = colour;
                    painter.lineWidth = layer.Stroke * scale;
                    painter.lineCap = layer.Round ? LineCap.Round : LineCap.Butt;
                    painter.lineJoin = LineJoin.Round;
                    painter.Stroke();
                }
                else
                {
                    painter.fillColor = colour;
                    painter.Fill(FillRule.NonZero);
                }
            }
        }

        private static void Trace(Painter2D painter, IconShapes.Step[] steps, float scale, Vector2 offset)
        {
            Vector2 At(Vector2 point) => point * scale + offset;
            foreach (var step in steps)
            {
                switch (step.Kind)
                {
                    case IconShapes.StepKind.Move: painter.MoveTo(At(step.A)); break;
                    case IconShapes.StepKind.Line: painter.LineTo(At(step.A)); break;
                    case IconShapes.StepKind.Cubic: painter.BezierCurveTo(At(step.A), At(step.B), At(step.C)); break;
                    case IconShapes.StepKind.Quad: painter.QuadraticCurveTo(At(step.A), At(step.B)); break;
                    case IconShapes.StepKind.Arc:
                        painter.Arc(At(step.A), step.Radius * scale, Angle.Degrees(step.From), Angle.Degrees(step.To),
                            step.Clockwise ? ArcDirection.Clockwise : ArcDirection.CounterClockwise);
                        break;
                    case IconShapes.StepKind.Close: painter.ClosePath(); break;
                }
            }
        }
    }
}
