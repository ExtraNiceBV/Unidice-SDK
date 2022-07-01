using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unidice.SDK.Utilities
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Insert<T>(this IEnumerable<T> enumerable, params T[] args)
        {
            var list = enumerable.ToList();
            list.AddRange(args);
            return list;
        }

        public static string UpperFirst(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var charArray = str.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }

        public static List<T> GetAllComponentsFromRoot<T>() where T : Component
        {
            var list = new List<T>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                foreach (var rootGameObject in scene.GetRootGameObjects())
                    list.AddRange(rootGameObject.GetComponentsInChildren<T>(true));
            }

            return list;
        }

        public static void AddRangeArray<T>(this HashSet<T> set, IEnumerable<T> entries)
        {
            set.UnionWith(entries);
        }

        public static void RemoveAllArray<T>(this HashSet<T> set, Predicate<T> match)
        {
            set.RemoveWhere(match);
        }

        /// <summary>
        /// Note: Will only add if there is a null slot in the array.
        /// </summary>
        public static void AddArray<T>(this T[] objects, T obj) 
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if(objects[i] == null)
                {
                    objects[i] = obj;
                    return;
                }
            }
        }

        public static void RemoveArray<T>(this T[] objects, T obj)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if(obj.Equals(objects[i]))
                {
                    objects[i] = default(T);
                    return;
                }
            }
        }
        public static T Random<T>(this IEnumerable<T> objects)
        {
            var array = objects.ToArray();
            if (array.Length <= 0) return default(T);
            var index = UnityEngine.Random.Range(0, array.Length);
            return array[index];
        }

        /// <summary>
        /// Optimized version that doesn't create an array
        /// </summary>
        public static T Random<T>(this ICollection<T> objects)
        {
            if (objects.Count <= 0) return default(T);
            var index = UnityEngine.Random.Range(0, objects.Count);
            return objects.ElementAt(index);
        }

        public static T Random<T>(this IEnumerable<T> objects, Func<T, bool> qualifier)
        {
            var array = objects.Where(qualifier).ToArray();
            if (array.Length <= 0) return default(T);
            var index = UnityEngine.Random.Range(0, array.Length);
            return array[index];
        }

        public static IEnumerable<T> Reversed<T>(this IEnumerable<T> objects)
        {
            return objects.Reverse();
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> objects, T element)
        {
            foreach (var obj in objects)
            {
                if (!obj.Equals(element)) yield return obj;
            }
        }

        public static bool HasIndex<T>(this IEnumerable<T> objects, int index)
        {
            if (index < 0) return false;
            return objects.Count() > index;
        }

        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> objects)
        {
            // Fisher-Yates Shuffle
            var array = objects.ToArray();
            var m = array.Length;
            while (m > 0)
            {
                int i = UnityEngine.Random.Range(0, m--);
                (array[m], array[i]) = (array[i], array[m]);
            }
            return array;
        }

        public static string ToCommaList<T>(this IEnumerable<T> objects)
        {
            var sb = new StringBuilder();

            bool first = true;
            foreach (var obj in objects)
            {
                if (first) first = false;
                else sb.Append(", ");

                sb.Append(obj);
            }

            return sb.ToString();
        }

        /// <summary>Gets only elements that occur a specific amount of times.</summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="objects">The objects to act on.</param>
        /// <param name="comparison">What to check for. E.g. >= 1.</param>
        /// <param name="propertySelector">How to determine duplicates.</param>
        /// <returns>The matching elements.</returns>
        public static IEnumerable<TElement> Occurrences<TElement, TProperty>(this IEnumerable<TElement> objects, Func<int, bool> comparison, Func<TElement, TProperty> propertySelector)
        {
            IEnumerable<IGrouping<TProperty, TElement>> groupBy = objects.GroupBy(propertySelector);
            return groupBy.Where(g => comparison(g.Count())).SelectMany(x=>x);
        }

        public static void SetStartColor(this ParticleSystem system, Func<Color, Color> func)
        {
            var main = system.main;
            main.startColor = func(main.startColor.color);
        }

        public static void SetStartColorChildren(this ParticleSystem system, Func<Color, Color> func)
        {
            foreach (var s in system.GetComponentsInChildren<ParticleSystem>())
            {
                var main = s.main;
                main.startColor = func(main.startColor.color);
            }
        }

        public static T RandomWeighted<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
        {
            var items = enumerable.ToArray();
            if (items.Length == 0) return default;

            float total = items.Sum(selector);

            float chance = UnityEngine.Random.Range(0, total);


            foreach (var type in items)
            {
                var tChance = selector(type);
                if (tChance <= 0) continue;
                if (chance < tChance) return type;
                chance -= tChance;
            }
            // No items left
            return default;
        }

        public static int RandomWeightedIndex<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
        {
            var items = enumerable.ToArray();
            if (items.Length == 0) return -1;

            float total = items.Sum(selector);

            float chance = UnityEngine.Random.Range(0, total);


            for (int i = 0; i < items.Length; i++)
            {
                var type = items[i];
                var tChance = selector(type);
                if (tChance <= 0) continue;
                if (chance < tChance) return i;
                chance -= tChance;
            }
            // No items left
            return -1;
        }

        public static IEnumerable<IEnumerable<T>> GetPermutationsWithRepetitions<T>(this IEnumerable<T> list, int length)
        {
            if(list == null) throw new NullReferenceException("List is null.");

            // List back as is
            if (length == 1) return list.Select(t => new[] { t } as IEnumerable<T>);

            // Against multiple enumeration
            var array = list as T[] ?? list.ToArray();

            // Linq recursion magic
            return GetPermutationsWithRepetitions(array, length - 1).SelectMany(t => array, (t1, t2) => t1.Concat(new[] { t2 }));
        }

        public static string HierarchyPath(this Transform transform)
        {
            return transform ? HierarchyPath(transform.parent) + "/" + transform.name : "";
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static string TextEffect(this string text, float percent, string color = "red")
        {
            var index = Mathf.RoundToInt(text.Length * percent);
            if (index > text.Length - 1) index = text.Length - 1;

            var left = text.Substring(0, index);
            var right = text.Substring(index, text.Length - index);
            return string.Format("<color={2}>{0}</color>{1}", left, right, color);
        }

        public static string Colored(this string text, Color32 color)
        {
            // Has to be Color32, or X2 won't work.
            return $"<color=#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}>{text}</color>";
        }

        public static T FirstOrDefault<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[0] : default;
        }

        public static bool IsUsable(this Selectable selectable)
        {
            return selectable && selectable.gameObject.activeInHierarchy && selectable.interactable;
        }

        public static void SetHideFlagsRecursively(this Component obj, HideFlags flags)
        {
            foreach (var child in obj.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.hideFlags = flags;
            }
        }

        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {
            RunRecursively(parent, g => g.layer = layer, includeChildren);
        }

        public static void RunRecursively(this GameObject parent, Action<GameObject> action, bool includeChildren = true)
        {
            action?.Invoke(parent);
            if (includeChildren)
            {
                foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
                {
                    action?.Invoke(trans.gameObject);
                }
            }
        }

        public static void SetX(this Transform transform, float x, Space space = Space.World)
        {
            if (space == Space.World)
            {
                var pos = transform.position;
                pos.x = x;
                transform.position = pos;
            }
            else
            {
                var pos = transform.localPosition;
                pos.x = x;
                transform.localPosition = pos;
            }
        }

        public static void SetY(this Transform transform, float y, Space space = Space.World)
        {
            if (space == Space.World)
            {
                var pos = transform.position;
                pos.y = y;
                transform.position = pos;
            }
            else
            {
                var pos = transform.localPosition;
                pos.y = y;
                transform.localPosition = pos;
            }
        }

        public static void SetZ(this Transform transform, float z, Space space = Space.World)
        {
            if (space == Space.World)
            {
                var pos = transform.position;
                pos.z = z;
                transform.position = pos;
            }
            else
            {
                var pos = transform.localPosition;
                pos.z = z;
                transform.localPosition = pos;
            }
        }

        private static readonly List<Transform> internalTransformList = new List<Transform>(30);
    
        public static void ExecuteInChildren<T>(this GameObject root, BaseEventData eventData, ExecuteEvents.EventFunction<T> callbackFunction) where T : IEventSystemHandler
        {
            internalTransformList.Clear();
            GetEventChain(root.transform, internalTransformList);
            // ReSharper disable once ForCanBeConvertedToForeach
            // Can't use foreach, the list can change by events that are executed!
            for (var i = 0; i < internalTransformList.Count; i++)
            {
                ExecuteEvents.Execute(internalTransformList[i].gameObject, eventData, callbackFunction);
            }
        }

        private static void GetEventChain(Transform root, ICollection<Transform> eventChain)
        {
            if (root == null) return;
            eventChain.Add(root);

            foreach (Transform child in root) GetEventChain(child, eventChain);
        }

        public static string ToHex(this Color c)
        {
            Color32 col = c;
            return $"#{col.r:X2}{col.g:X2}{col.b:X2}{col.a:X2}";
        }

        public static void PlayAt(this ParticleSystem system, Collision2D collision)
        {
            var contact = collision.contacts[0];
            system.transform.position = contact.point;
            system.transform.LookAt(contact.point + contact.normal);

            system.Play(true);
        }

        public static IEnumerable<T> PadRight<T>(this IEnumerable<T> source, int length, T toAdd)
        {
            int i = 0;
            // use "Take" in case there are less items than length
            foreach (var item in source.Take(length))
            {
                yield return item;
                i++;
            }
            for (; i < length; i++) yield return toAdd;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence == null) yield break;
            var enumerator = sequence.GetEnumerator();
            while (enumerator.MoveNext())
            {
                action(enumerator.Current);
                yield return enumerator.Current;
            }
            enumerator.Dispose();
        }
    }
}