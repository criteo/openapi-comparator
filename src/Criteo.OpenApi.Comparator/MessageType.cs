// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

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

        /// <summary>
        ///     An element doesn't follow the Open API specification
        /// </summary>
        Specification
    }
}
