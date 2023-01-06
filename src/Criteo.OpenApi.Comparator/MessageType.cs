namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Types of differences that can be found
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// If an OpenAPI element is added in the new version
        /// </summary>
        Addition,
        
        /// <summary>
        /// If an OpenAPI element is updated in the new version
        /// </summary>
        Update,
        
        /// <summary>
        /// If an OpenAPI element is removed in the new version
        /// </summary>
        Removal,
    }
}
