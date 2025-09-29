// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Logging;

namespace Criteo.OpenApi.Comparator
{
    /// <summary>
    /// Types of differences that can be found
    /// </summary>
    public enum MessageSeverity 
    {
        /// <summary>
        /// The change hasn't any impact on the API (i.e. the version of the OAS is updated)
        /// </summary>
        Info,

        /// <summary>
        /// The change has an impact on the API, but should not break client's integration (i.e. adding an optional parameter)
        /// </summary>
        Warning,

        /// <summary>
        ///     The change should be considered as a breaking change (i.e. updating the format of a response body)
        ///     This would be an error if in strict mode, or when comparing minor version changes.
        ///     When not in strict mode (major version changes) it will be considered a warning.
        /// </summary>
        Breaking,

        /// <summary>
        ///     The change should be considered as an error regardless of the version differences
        /// </summary>
        Error
    }
}
