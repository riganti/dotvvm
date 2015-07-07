using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Utils
{
    public static class JsonUtils
    {
        public static JObject Diff(JObject source, JObject target, out bool changed, bool nullOnRemoved = false)
        {
            changed = false;
            var diff = new JObject();
            foreach (var item in target)
            {
                var sourceItem = source[item.Key];
                if (sourceItem == null)
                {
                    if (item.Value != null)
                    {
                        diff[item.Key] = item.Value;
                    }
                }
                else if (sourceItem.Type != item.Value.Type)
                {
                    if (sourceItem.Type == JTokenType.Object || sourceItem.Type == JTokenType.Array
                        || item.Value.Type == JTokenType.Object || item.Value.Type == JTokenType.Array
                        || item.Value.ToString() != sourceItem.ToString())
                    {

                        diff[item.Key] = item.Value;
                        changed = true;
                    }
                }
                else if (sourceItem.Type == JTokenType.Object) // == item.Value.Type
                {
                    bool subchanged;
                    var itemDiff = Diff((JObject)sourceItem, (JObject)item.Value, out subchanged, nullOnRemoved);
                    if (subchanged)
                    {
                        diff[item.Key] = itemDiff;
                        changed = true;
                    }
                }
                else if (sourceItem.Type == JTokenType.Array)
                {
                    var sourceArr = (JArray)sourceItem;
                    var subchanged = false;
                    var arrDiff = Diff(sourceArr, (JArray)item.Value, out subchanged, nullOnRemoved);
                    if (subchanged)
                    {
                        diff[item.Key] = arrDiff;
                        changed = true;
                    }
                }
                else if (!JToken.DeepEquals(sourceItem, item.Value))
                {
                    diff[item.Key] = item.Value;
                    changed = true;
                }
            }
            if (nullOnRemoved)
            {
                foreach (var item in source)
                {
                    if (target[item.Key] == null) diff[item.Key] = JValue.CreateNull();
                }
            }
            return diff;
        }

        public static JArray Diff(JArray source, JArray target, out bool changed, bool nullOnRemoved = false)
        {
            changed = source.Count != target.Count;
            var diffs = new JToken[target.Count];
            var commonLen = Math.Min(diffs.Length, source.Count);
            for (int i = 0; i < commonLen; i++)
            {
                if (target[i].Type == JTokenType.Object && source[i].Type == JTokenType.Object)
                {
                    var subchanged = false;
                    diffs[i] = Diff((JObject)source[i], (JObject)target[i], out subchanged, nullOnRemoved);
                    if (subchanged) changed = true;
                }
                else if (target[i].Type == JTokenType.Array && source[i].Type == JTokenType.Array)
                {
                    var subchanged = false;
                    diffs[i] = Diff((JArray)source[i], (JArray)target[i], out subchanged, nullOnRemoved);
                    if (subchanged) changed = true;
                }
                else
                {
                    diffs[i] = target[i];
                    if (!JToken.DeepEquals(source[i], target[i]))
                        changed = true;
                }
            }
            for (int i = commonLen; i < diffs.Length; i++)
            {
                diffs[i] = target[i];
                changed = true;
            }
            return new JArray(diffs);
        }

        internal static JObject Diff(JObject source, JObject jobj)
        {
            bool c;
            return Diff(source, jobj, out c);
        }
    }
}
