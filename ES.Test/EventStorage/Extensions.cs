using ES.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ES.Test.EventStorage
{
    public static class Extensions
    {
        public static T JsonClone<T>(this T source)
    where T : class, IEsMessage
        {
            if (source == null)
                return default!;
            var type = source.GetType();
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject(serialized, type) as T;
        }
    }
}
