namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para />
    /// </summary>
    public static class StorePropertyExtensions
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="storeproperty">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static string ValueToString(this StoreProperty storeproperty)
        {
            if (storeproperty.Value == null)
            {
                return "Null";
            }

            if (storeproperty.Id.Type == StorePropertyType.String)
            {
                return string.Format("\"{0}\"", storeproperty.Value);
            }

            return storeproperty.Value.ToString();
        }
    }
}
