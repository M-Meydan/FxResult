using FxResults.ResultExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxResults.Core
{
    /// <summary>
    /// Represents metadata information, including pagination details and additional custom metadata.
    /// </summary>
    public readonly record struct MetaInfo
    {
        /// <summary>
        /// Gets the pagination information for the current data set.
        /// </summary>
        public PaginationInfo? Pagination { get; init; }

        /// <summary>
        /// Arbitrary custom metadata not explicitly modeled.
        /// </summary>
        public ImmutableDictionary<string, object?> Additional { get; init; }

        public MetaInfo()
        {
            Additional = ImmutableDictionary<string, object?>.Empty;
        }

        public MetaInfo(PaginationInfo? pagination = null, ImmutableDictionary<string, object?>? additional = null)
        {
            Pagination = pagination;
            Additional = additional ?? ImmutableDictionary<string, object?>.Empty;
        }
    }
}
