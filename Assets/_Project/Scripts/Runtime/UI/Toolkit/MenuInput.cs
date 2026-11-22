using Clover.Player;
using UnityEngine.InputSystem;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The pad and the keys on a UI Toolkit menu (ui_rest_fire_plan.md 5.0): reads the Controls' UI map itself, so the
    /// held-push repeat is the game's own (0.32 s, then 0.2 s) and every binding follows rebinding. The screen swallows
    /// UI Toolkit's own navigation events, so nothing moves twice even once Controls is the project-wide actions asset.
    /// The pointer is the screen's (pointer and click events on its places).
    /// </summary>
    public sealed class MenuInput
    {
        /// <summary>How many lines a second the right stick scrolls the pane at full lean.</summary>
        public const float ScrollLinesPerSecond = 14f;

        private readonly InputActionMap map;
        private readonly InputAction navigate, submit, cancel, keep, shape, halfLeft, halfRight, scrollPane, textSize, rightClick;
        private readonly NavRepeat repeat = new NavRepeat();
        // The presses that tell which device is in hand, kept in one array so a frame allocates nothing.
        private readonly InputAction[] watched;

        public MenuInput(InputActionAsset controls)
        {
            map = controls != null ? controls.FindActionMap(ControlBindings.MenusMap) : null;
            if (map == null) return;
            navigate = map.FindAction("Navigate");
            submit = map.FindAction("Submit");
            cancel = map.FindAction("Cancel");
            keep = map.FindAction("Keep");
            shape = map.FindAction("Shape");
            halfLeft = map.FindAction("HalfLeft");
            halfRight = map.FindAction("HalfRight");
            scrollPane = map.FindAction("ScrollPane");
            textSize = map.FindAction("TextSize");
            rightClick = map.FindAction("RightClick");
            watched = new[] { navigate, submit, cancel, keep, shape, halfLeft, halfRight, textSize };
        }

        /// <summary>False when the controls have no UI map (saved before it existed): the screen cannot be driven by pad then.</summary>
        public bool Ready => map != null;

        public void Enable()
        {
            repeat.Reset();
            map?.Enable();
        }

        public void Disable() => map?.Disable();

        /// <summary>Feeds this frame's presses to the Loadout. True when B (or a right click) asked to go back to the fire.</summary>
        public bool Drive(LoadoutController loadout, float deltaSeconds)
        {
            if (map == null || loadout == null) return false;
            Notice(watched);
            if (Pressed(cancel) || Pressed(rightClick)) return loadout.Cancel();
            if (Pressed(submit))
            {
                loadout.Submit();
                return false;
            }
            if (Pressed(shape)) loadout.Shape();
            if (Pressed(keep)) loadout.KeepDown();
            else if (keep != null && keep.IsPressed()) loadout.KeepHeld(deltaSeconds);
            if (keep != null && keep.WasReleasedThisFrame()) loadout.KeepUp();
            // Shift with Tab is LB, as Shift+Tab goes back a field everywhere else.
            bool shift = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            if (Pressed(halfLeft) || (Pressed(halfRight) && shift)) loadout.JumpHalf(true);
            else if (Pressed(halfRight)) loadout.JumpHalf(false);
            if (Pressed(textSize)) loadout.StepTextSize();
            if (scrollPane != null)
            {
                float lean = scrollPane.ReadValue<float>();
                if (lean > 0.2f || lean < -0.2f) loadout.ScrollPane(-lean * ScrollLinesPerSecond * deltaSeconds);
            }
            if (navigate != null)
            {
                var stick = navigate.ReadValue<UnityEngine.Vector2>();
                if (repeat.Step(stick.x, stick.y, deltaSeconds, out var direction)) loadout.Move(direction);
            }
            return false;
        }

        private static bool Pressed(InputAction action) => action != null && action.WasPressedThisFrame();

        // The glyphs follow the device last touched (4.5): a press on the menu counts as touching it.
        private static void Notice(InputAction[] actions)
        {
            foreach (var action in actions)
            {
                if (action == null || !action.WasPerformedThisFrame()) continue;
                var control = action.activeControl;
                if (control != null) ControlDevices.Notice(control.device);
                return;
            }
        }
    }
}
