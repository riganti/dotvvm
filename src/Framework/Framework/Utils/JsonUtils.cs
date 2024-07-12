using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Buffers;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Text.Json;

namespace DotVVM.Framework.Utils
{
    public static class JsonUtils
    {
        public static JsonObject Diff(JsonObject source, JsonObject target, bool nullOnRemoved = false, Func<(string TypeId, string Property), bool?>? includePropertyOverride = null)
        {
            var typeId = target["$type"]?.GetValue<string>();

            var diff = new JsonObject();
            foreach (var item in target)
            {
                if (typeId != null && includePropertyOverride != null && !item.Key.StartsWith("$"))
                {
                    var include = includePropertyOverride((typeId, item.Key));
                    if (include == true)
                    {
                        diff[item.Key] = item.Value?.DeepClone();
                        continue;
                    }
                    else if (include == false)
                    {
                        continue;
                    }
                }

                if (!source.TryGetPropertyValue(item.Key, out var sourceItem))
                {
                    diff[item.Key] = item.Value?.DeepClone();
                    continue;
                }

                var sourceKind = sourceItem?.GetValueKind();
                var targetKind = item.Value?.GetValueKind();
                if (sourceKind != targetKind)
                {
                    diff[item.Key] = item.Value?.DeepClone();
                }
                else if (sourceKind == JsonValueKind.Object)
                {
                    var itemDiff = Diff((JsonObject)sourceItem!, (JsonObject)item.Value!, nullOnRemoved);
                    if (itemDiff.Count > 0)
                    {
                        diff[item.Key] = itemDiff;
                    }
                }
                else if (sourceKind == JsonValueKind.Array)
                {
                    var arrDiff = Diff((JsonArray)sourceItem!, (JsonArray)item.Value!, out var subchanged, nullOnRemoved);
                    if (subchanged)
                    {
                        diff[item.Key] = arrDiff;
                    }
                }
                else if (!JsonNode.DeepEquals(sourceItem, item.Value))
                {
                    diff[item.Key] = item.Value?.DeepClone();
                }
            }

            if (nullOnRemoved)
            {
                foreach (var item in source)
                {
                    if (target[item.Key] == null)
                        diff[item.Key] = null;
                }
            }
            return diff;
        }


        public static JsonArray Diff(JsonArray source, JsonArray target, out bool changed, bool nullOnRemoved = false)
        {
            changed = source.Count != target.Count;
            var diffs = new JsonNode?[target.Count];
            var commonLen = Math.Min(diffs.Length, source.Count);
            for (int i = 0; i < commonLen; i++)
            {
                var targetKind = target[i]?.GetValueKind();
                var sourceKind = source[i]?.GetValueKind();
                if (targetKind == JsonValueKind.Object && sourceKind == JsonValueKind.Object)
                {
                    diffs[i] = Diff(source[i]!.AsObject(), target[i]!.AsObject(), nullOnRemoved);
                    if (((JsonObject)diffs[i]!).Count > 0) changed = true;
                }
                else if (targetKind == JsonValueKind.Array && sourceKind == JsonValueKind.Array)
                {
                    diffs[i] = Diff((JsonArray)source[i]!, (JsonArray)target[i]!, out var subchanged, nullOnRemoved);
                    if (subchanged) changed = true;
                }
                else
                {
                    diffs[i] = target[i]?.DeepClone();
                    if (!JsonNode.DeepEquals(source[i], target[i]))
                        changed = true;
                }
            }
            for (int i = commonLen; i < diffs.Length; i++)
            {
                diffs[i] = target[i]?.DeepClone();
                changed = true;
            }
            return new JsonArray(diffs);
        }

        private static JsonNode? PatchItem(JsonNode? target, JsonNode? diff, bool removeOnNull = false)
        {
            if (target is null || diff is null) return diff?.DeepClone();

            var targetKind = target.GetValueKind();
            var diffKind = diff.GetValueKind();
            if (targetKind == JsonValueKind.Object && diffKind == JsonValueKind.Object)
            {
                Patch((JsonObject)target!, (JsonObject)diff!, removeOnNull);
                return target;
            }
            else if (targetKind == JsonValueKind.Array && diffKind == JsonValueKind.Array)
            {
                var targetArray = target!.AsArray();
                var targetItems = targetArray.ToArray();
                targetArray.Clear();
                var diffArray = diff!.AsArray();
                for (int i = 0; i < diffArray.Count; i++)
                {
                    targetArray.Add(i >= targetItems.Length ? diffArray[i]?.DeepClone() : PatchItem(targetItems[i], diffArray[i], removeOnNull));
                }
                return targetArray;
            }
            else
            {
                return diff.DeepClone();
            }
        }

        public static void Patch(JsonObject target, JsonObject diff, bool removeOnNull = false)
        {
            foreach (var prop in diff)
            {
                var val = target[prop.Key];
                if (val == null) target[prop.Key] = prop.Value?.DeepClone();
                else if (prop.Value is null && removeOnNull) target.Remove(prop.Key);
                else target[prop.Key] = PatchItem(val, prop.Value, removeOnNull);
            }
        }
    }
}
