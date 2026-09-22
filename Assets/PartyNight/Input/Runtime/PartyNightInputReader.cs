using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PartyNight.Input
{
    public sealed class PartyNightInputReader : IDisposable
    {
        private readonly InputActionAsset actions;
        private readonly InputActionMap gameplay;
        private readonly InputAction move;
        private readonly InputAction look;
        private readonly InputAction jump;
        private readonly InputAction interact;
        private readonly InputAction grab;
        private readonly InputAction dash;
        private readonly InputAction useItem;
        private readonly InputAction emote;
        private readonly bool ownsActionAsset;
        private readonly bool manageActionMapState;
        private bool disposed;

        public PartyNightInputReader(InputActionAsset source)
            : this(source, cloneSource: true, manageActionMapState: true)
        {
        }

        private PartyNightInputReader(
            InputActionAsset source,
            bool cloneSource,
            bool manageActionMapState)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ownsActionAsset = cloneSource;
            this.manageActionMapState = manageActionMapState;
            actions = cloneSource
                ? UnityEngine.Object.Instantiate(source)
                : source;

            gameplay = actions.FindActionMap(PartyNightInputNames.GameplayMap, true);
            move = gameplay.FindAction(PartyNightInputNames.Move, true);
            look = gameplay.FindAction(PartyNightInputNames.Look, true);
            jump = gameplay.FindAction(PartyNightInputNames.Jump, true);
            interact = gameplay.FindAction(PartyNightInputNames.Interact, true);
            grab = gameplay.FindAction(PartyNightInputNames.Grab, true);
            dash = gameplay.FindAction(PartyNightInputNames.Dash, true);
            useItem = gameplay.FindAction(PartyNightInputNames.UseItem, true);
            emote = gameplay.FindAction(PartyNightInputNames.Emote, true);
        }

        public bool Enabled => !disposed && gameplay.enabled;

        public static PartyNightInputReader CreateFromProjectWideActions()
        {
            var source = InputSystem.actions;
            if (source == null)
            {
                throw new InvalidOperationException(
                    "Party Night project-wide Input Actions are not assigned.");
            }

            // Project-wide actions are already the single preloaded runtime asset.
            // Borrow it instead of cloning a second always-on action graph.
            return new PartyNightInputReader(
                source,
                cloneSource: false,
                manageActionMapState: false);
        }

        public void Enable()
        {
            ThrowIfDisposed();
            if (!gameplay.enabled)
            {
                gameplay.Enable();
            }
        }

        public void Disable()
        {
            if (!disposed && manageActionMapState)
            {
                gameplay.Disable();
            }
        }

        public PartyNightInputFrame ReadFrame()
        {
            ThrowIfDisposed();

            var lookValue = look.ReadValue<Vector2>();
            var lookMode = look.activeControl?.device is Pointer
                ? PartyNightLookInputMode.Delta
                : PartyNightLookInputMode.Rate;

            return new PartyNightInputFrame(
                move.ReadValue<Vector2>(),
                lookValue,
                lookMode,
                jump.WasPressedThisFrame(),
                interact.WasPressedThisFrame(),
                grab.WasPressedThisFrame(),
                dash.WasPressedThisFrame(),
                useItem.WasPressedThisFrame(),
                emote.WasPressedThisFrame());
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            if (manageActionMapState)
            {
                gameplay.Disable();
            }

            disposed = true;

            if (!ownsActionAsset)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(actions);
                return;
            }
#endif
            UnityEngine.Object.Destroy(actions);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(PartyNightInputReader));
            }
        }
    }
}
