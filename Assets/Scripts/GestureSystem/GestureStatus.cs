namespace GestureControl
{
    public enum GestureStatus
    {
        /// <summary>
        /// Conditions of gesture have just become true (Inactive -> Active)
        /// </summary>
        Starting,

        /// <summary>
        /// Conditions of gesture are still true (Holding active)
        /// </summary>
        Active,

        /// <summary>
        /// Conditions of gesture have just become false (Active -> Inactive)
        /// </summary>
        Stopping,

        /// <summary>
        /// Conditions of gesture are still unfulfilled (Holding inactive)
        /// </summary>
        Inactive
    }
}
