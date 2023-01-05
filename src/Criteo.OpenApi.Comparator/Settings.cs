// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.OpenApi;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using Criteo.OpenApi.Comparator.Core.Properties;

namespace Criteo.OpenApi.Comparator.Settings
{
    /// <summary>
    /// Class that represents the command line options
    /// The CommandLineInfo attribute is reflected to display help.
    /// Prefer to show required properties before optional.
    /// Although not guaranteed by the Framework, the iteration order matches the
    /// order of definition.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// If true, then checking should be strict, in other words, breaking changes are errors instead of warnings.
        /// </summary>
        public bool Strict { get; set; } = false;
    }
}
