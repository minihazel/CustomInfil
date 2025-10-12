using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace hazelify.EntryPointSelector.Data
{
    public static class ExfilLookup
    {
        public static string GetInternalName(string map, string translatedName)
        {
            var internalList = GetListFromClass(typeof(ExfilData), map);
            var translatedList = GetListFromClass(typeof(ExfilDescData), map);

            if (internalList != null && translatedList != null)
            {
                for (int i = 0; i < translatedList.Count; i++)
                {
                    if (translatedList[i].Equals(translatedName, StringComparison.OrdinalIgnoreCase))
                        return internalList[i];
                }
            }

            return null;
        }

        private static List<string> GetListFromClass(Type classType, string listName)
        {
            var field = classType.GetField(listName, BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null) as List<string>;
        }
    }
}
