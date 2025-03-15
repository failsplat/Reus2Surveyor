using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reus2Surveyor
{
    class DictHelper
    {
        public static object DigValueAtKeys(Dictionary<string, object> dict, List<string> keys)
        {
            switch (keys.Count())
            {
                case 0:
                    {
                        throw new KeyNotFoundException();
                    }
                case 1:
                    {
                        string thiskey = keys[0];
                        if (dict.ContainsKey(thiskey))
                        {
                            return dict[thiskey];
                        }
                        throw new KeyNotFoundException();
                    }
                default:
                    {
                        string thiskey = keys[0];
                        if (dict.ContainsKey(thiskey))
                        {
                            if (dict[thiskey].GetType() == typeof(Dictionary<string, object>))
                            {
                                return DigValueAtKeys((Dictionary<string, object>)dict[thiskey], keys[1..(keys.Count())]);
                            }

                        }
                        throw new KeyNotFoundException();
                    }
            }
        }

        public static int? TryGetInt(Dictionary<string, object> dict, List<string> keys)
        {
            try
            {
                object result = DictHelper.DigValueAtKeys(dict, keys);
                if (result is null)
                {
                    return (int?)null;
                }
                return (int)(long)DictHelper.DigValueAtKeys(dict, keys);
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
            return null;
        }

        public static int? TryGetInt(Dictionary<string, object> dict, string key)
        {
            return TryGetInt(dict, [key]);
        }

        public static float? TryGetFloat(Dictionary<string, object> dict, List<string> keys)
        {
            try
            {
                object result = DictHelper.DigValueAtKeys(dict, keys);
                if (result is null)
                {
                    return (float?)null;
                }
                return Convert.ToSingle(DictHelper.DigValueAtKeys(dict, keys));
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
            return null;
        }

        public static float? TryGetFloat(Dictionary<string, object> dict, string key)
        {
            return TryGetFloat(dict, [key]);
        }

        public static string TryGetString(Dictionary<string, object> dict, List<string> keys)
        {
            try
            {
                object result = DictHelper.DigValueAtKeys(dict, keys);
                if (result is null)
                {
                    return null;
                }
                return (string)DictHelper.DigValueAtKeys(dict, keys);
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
            return null;
        }

        public static string TryGetString(Dictionary<string, object> dict, string key) 
        {
            return TryGetString(dict, [key]);
        }

        public static bool? TryGetBool(Dictionary<string, object> dict, List<string> keys)
        {
            try
            {
                object result = DictHelper.DigValueAtKeys(dict, keys);
                if (result is null)
                {
                    return null;
                }
                return (bool)DictHelper.DigValueAtKeys(dict, keys);
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
            return null;
        }

        public static bool? TryGetBool(Dictionary<string, object> dict, string key)
        {
            return TryGetBool(dict, [key]);
        }

        public static List<Dictionary<string, object>> TryGetDictList(Dictionary<string, object> dict, List<string> keysToList, string finalKey = null)
        {
            List<Dictionary<string, object>> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }

            if (finalKey is not null)
            {
                foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
                {
                    Dictionary<string, object> subdict = (Dictionary<string, object>)r[finalKey];
                    output.Add(subdict);
                }
            }
            else
            {
                foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
                {
                    output.Add(r);
                }
            }
            return output;
        }

        public static List<string> TryGetStringList(Dictionary<string, object> dict, List<string> keysToList, string finalKey)
        {
            List<string> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                if (r.ContainsKey(finalKey))
                {
                    output.Add(r[finalKey] is null ? null : (string)r[finalKey]);
                }
                else
                {
                    output.Add(null);
                }
            }
            return output;
        }

        public static List<string> TryGetStringList(Dictionary<string, object> dict, List<string> keysToList, List<string> finalKeys)
        {
            List<string> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                try
                {
                    output.Add(TryGetString(r, finalKeys));
                }
                catch (KeyNotFoundException e)
                {
                    output.Add(null);
                }
            }
            return output;
        }

        public static List<int?> TryGetIntList(Dictionary<string, object> dict, List<string> keysToList, string finalKey)
        {
            List<int?> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                if (r.ContainsKey(finalKey))
                {
                    output.Add(r[finalKey] is null ? null : (int)(long)r[finalKey]);
                }
                else
                {
                    output.Add(null);
                }
            }
            return output;
        }

        public static List<int?> TryGetIntList(Dictionary<string, object> dict, List<string> keysToList, List<string> finalKeys)
        {
            List<int?> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                try
                {
                    output.Add(TryGetInt(r, finalKeys));
                }
                catch (KeyNotFoundException e)
                {
                    output.Add(null);
                }
            }
            return output;
        }

        public static List<float?> TryGetFloatList(Dictionary<string, object> dict, List<string> keysToList, string finalKey)
        {
            List<float?> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                if (r.ContainsKey(finalKey))
                {
                    output.Add(r[finalKey] is null ? null : (float)r[finalKey]);
                }
                else
                {
                    output.Add(null);
                }
            }
            return output;
        }

        public static List<float?> TryGetFloatList(Dictionary<string, object> dict, List<string> keysToList, List<string> finalKeys)
        {
            List<float?> output = [];
            List<object> result = (List<object>)DictHelper.DigValueAtKeys(dict, keysToList);

            if (result is null)
            {
                return [];
            }
            foreach (Dictionary<string, object> r in result.Cast<Dictionary<string, object>>())
            {
                try
                {
                    output.Add(TryGetFloat(r, finalKeys));
                }
                catch (KeyNotFoundException e)
                {
                    output.Add(null);
                }
            }
            return output;
        }
    }
}
