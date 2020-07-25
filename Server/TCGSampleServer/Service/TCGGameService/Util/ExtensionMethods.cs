/*
 * Copyright 2020 LINE Corporation
 *
 * LINE Corporation licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGGameService
{
    public static class ExtensionMethods
    {
        public static string ToDateFormat(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static T PickRandomOrDefault<T>(this IEnumerable<T> source)
        {
            if (source.Count() == 0)
                return default(T);

            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static T PopAT<T>(this List<T> list, int index)
        {
            if (list.Count == 0)
                return default(T);

            var r = list[index];
            list.RemoveAt(index);
            return r;
        }

        public static List<T> PopRange<T>(this List<T> list, int index, int count)
        {
            var r = list.GetRange(index, count);
            list.RemoveRange(index, count);
            return r;
        }

        public static T PopFirst<T>(this List<T> list, Predicate<T> predicate)
        {
            var index = list.FindIndex(predicate);
            var r = list[index];
            list.RemoveAt(index);
            return r;
        }

        public static T PopFirstOrDefault<T>(this List<T> list, Predicate<T> predicate) where T : class
        {
            var index = list.FindIndex(predicate);
            if (index > -1)
            {
                var r = list[index];
                list.RemoveAt(index);
                return r;
            }

            return null;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static List<T> Swap<T>(this List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }

    }
}
