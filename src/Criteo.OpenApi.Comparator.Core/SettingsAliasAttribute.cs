// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Criteo.OpenApi.Comparator.Core
{
    /// <summary>
    /// Attribute used for extending parameters with aliases.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class SettingsAliasAttribute : Attribute
    {
        /// <param name="alias">Alias of the setting.</param>
        public SettingsAliasAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; }
    }
}
