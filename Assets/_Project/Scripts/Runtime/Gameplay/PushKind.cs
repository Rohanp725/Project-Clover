namespace Clover.Gameplay
{
    /// <summary>What kind of push moves a body, so only a real shove is a moment its conditions answer.</summary>
    public enum PushKind
    {
        /// <summary>A push given all at once (a blow, a throw): a shove when it is strong enough.</summary>
        Blow,
        /// <summary>The first touch of a push spread over a while (a Gust's wind): always a shove, however small this step.</summary>
        Shove,
        /// <summary>A steady carry or pull a step at a time (Eye's carry, Lodestone's drag): never a shove.</summary>
        Drag,
    }
}
