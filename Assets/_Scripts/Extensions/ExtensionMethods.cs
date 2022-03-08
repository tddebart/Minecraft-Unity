using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UnboundLib
{
    public static class ExtensionMethods
    {
        #region string
        public static string Sanitize(this string str, string[] invalidSubstrs = null)
        {

            invalidSubstrs = invalidSubstrs ?? new string[] { "\n", "\t", "\\", "\"", "\'", "[", "]" };

            foreach (string invalidsubstr in invalidSubstrs)
            {
                str.Replace(invalidsubstr, string.Empty);
            }

            return str;
            
        }
        #endregion

        #region GameObject

        public static T GetOrAddComponent<T>(this GameObject go, bool searchChildren = false) where T : Component
        {
            var component = searchChildren == true ? go.GetComponentInChildren<T>() : go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        #endregion

        #region MonoBehaviour

        public static void ExecuteAfterFrames(this MonoBehaviour mb, int delay, Action action)
        {
            mb.StartCoroutine(ExecuteAfterFramesCoroutine(delay, action));
        }
        public static void ExecuteAfterSeconds(this MonoBehaviour mb, float delay, Action action)
        {
            mb.StartCoroutine(ExecuteAfterSecondsCoroutine(delay, action));
        }

        private static IEnumerator ExecuteAfterFramesCoroutine(int delay, Action action)
        {
            for (int i = 0; i < delay; i++)
                yield return null;

            action();
        }
        private static IEnumerator ExecuteAfterSecondsCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        #endregion

        #region Image

        public static void SetAlpha(this Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        #endregion

        #region Bool

        public static int AsMultiplier(this bool value)
        {
            return value == true ? 1 : -1;
        }

        #endregion

        #region Array/List

        public static T GetRandom<T>(this IList array)
        {
            return (T)array[Random.Range(0, array.Count)];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion

        #region Dictionary

        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            V result;
            dictionary.TryGetValue(key, out result);
            return result;
        }

        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue)
        {
            V result;
            if (dictionary.TryGetValue(key, out result) == false)
            {
                result = defaultValue;
            }
            return result;
        }

        #endregion

        #region Transform

        /// <summary>
        /// Recursively search for children with a given name.
        /// WARNING: Is this really the best way to do what you want?
        /// </summary>
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        // Increment the x, y, or z position of a transform
        public static void AddXPosition(this Transform transform, float x)
        {
            Vector3 position = transform.position;
            position.x += x;
            transform.position = position;
        }
        public static void AddYPosition(this Transform transform, float y)
        {
            Vector3 position = transform.position;
            position.y += y;
            transform.position = position;
        }
        public static void AddZPosition(this Transform transform, float z)
        {
            Vector3 position = transform.position;
            position.z += z;
            transform.position = position;
        }

        // Transform
        // Set the x, y, or z position of a transform
        public static void SetXPosition(this Transform transform, float x)
        {
            Vector3 position = transform.position;
            position.x = x;
            transform.position = position;
        }
        public static void SetYPosition(this Transform transform, float y)
        {
            Vector3 position = transform.position;
            position.y = y;
            transform.position = position;
        }
        public static void SetZPosition(this Transform transform, float z)
        {
            Vector3 position = transform.position;
            position.z = z;
            transform.position = position;
        }

        #endregion

        #region LayerMask

        public static bool IsLayerInMask(this LayerMask layerMask, int layer)
        {
            return layerMask.value == (layerMask.value | 1 << layer);
        }

        #endregion
    }
}