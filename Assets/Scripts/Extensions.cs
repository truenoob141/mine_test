using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mine
{
    public static class Extensions
    {
        public static GameObject Clone(this GameObject origin, Transform parent)
        {
            var item = GameObject.Instantiate(origin, parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(true);
            return item;
        }

        public static GameObject Clone(this GameObject origin, bool setAcitve = true)
        {
            var item = GameObject.Instantiate(origin, origin.transform.parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(setAcitve);
            return item;
        }

        public static T Clone<T>(this T origin, Transform parent) where T : Component
        {
            T item = GameObject.Instantiate<T>(origin, parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(true);
            return item;
        }

        public static T Clone<T>(this T origin, bool setAcitve = true) where T : Component
        {
            T item = GameObject.Instantiate<T>(origin, origin.transform.parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(setAcitve);
            return item;
        }

        public static T RandomValue<T>(this List<T> list)
        {
            if (list.Count == 0) return default(T);

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T RandomValue<T>(this IEnumerable<T> list)
        {
            return list.OrderBy(i => UnityEngine.Random.value).FirstOrDefault();
        }

        public static T RandomValue<T>(this IEnumerable<T> list, System.Func<T, bool> predicate)
        {
            return list.Where(predicate).OrderBy(i => UnityEngine.Random.value).FirstOrDefault();
        }
    }
}
