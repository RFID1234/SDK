using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SoochakBharat.SDK.Models;

namespace SoochakBharat.SDK.Concurrency
{
    /// <summary>
    /// Thread-safe tag de-duplication cache keyed by EPC (and optionally antenna).
    /// </summary>
    public class TagCache
    {
        private readonly ConcurrentDictionary<string, TagData> _tags = new();

        /// <summary>
        /// Upserts a tag. If the EPC exists, read counts are aggregated.
        /// Returns the current snapshot of the tag.
        /// </summary>
        public TagData Upsert(TagData incoming, bool includeAntennaInKey = false)
        {
            var key = BuildKey(incoming, includeAntennaInKey);

            return _tags.AddOrUpdate(
                key,
                _ => Clone(incoming),
                (_, existing) =>
                {
                    existing.ReadCount += incoming.ReadCount;
                    existing.Rssi = incoming.Rssi;
                    existing.TimestampUtc = incoming.TimestampUtc;
                    existing.Antenna = incoming.Antenna;
                    existing.FrequencyKHz = incoming.FrequencyKHz;
                    return existing;
                });
        }

        public IReadOnlyCollection<TagData> Snapshot()
        {
            return _tags.Values.ToList();
        }


        public void Clear() => _tags.Clear();

        private static string BuildKey(TagData tag, bool includeAntenna)
        {
            return includeAntenna ? $"{tag.Epc}|{tag.Antenna}" : tag.Epc;
        }

        private static TagData Clone(TagData src)
        {
            return new TagData
            {
                Epc = src.Epc,
                Antenna = src.Antenna,
                Rssi = src.Rssi,
                ReadCount = src.ReadCount,
                FrequencyKHz = src.FrequencyKHz,
                Phase = src.Phase,
                BankData = src.BankData,
                TimestampUtc = src.TimestampUtc
            };
        }
    }
}


