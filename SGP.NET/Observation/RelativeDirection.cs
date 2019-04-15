namespace SGPdotNET.Observation
{
    /// <summary>
    /// Defines a direction of movement relative to a ground station
    /// </summary>
    public enum RelativeDirection
    {
        /// <summary>
        /// Moving toward the ground station
        /// </summary>
        Approaching,
        /// <summary>
        /// Not moving relative to the ground station
        /// </summary>
        Fixed,
        /// <summary>
        /// Moving away from the ground station
        /// </summary>
        Receding
    }
}