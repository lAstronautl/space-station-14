using Robust.Shared.GameStates;

namespace Content.Shared.Flash.Components
{
    /// <summary>
    ///     Exists for use as a status effect. Adds a shader to the client that obstructs vision.
    /// </summary>
    [RegisterComponent, NetworkedComponent, Access(typeof(SharedFlashSystem))]
    public sealed partial class FlashableModifierComponent : Component
    {
        /// <summary>
        /// Coefficient for flash duration
        /// </summary>
        [DataField]
        public float DurationMultiplier { get; set; } = 1;

        /// <summary>
        /// Coefficient for movement reduction on flash
        /// </summary>
        [DataField]
        public float SlowdownMultiplier { get; set; } = 1;

        /// <summary>
        /// Coefficient for stun duration on melee flash
        /// </summary>
        [DataField]
        public float StunDurationMultiplier { get; set; } = 1;
    }
}
