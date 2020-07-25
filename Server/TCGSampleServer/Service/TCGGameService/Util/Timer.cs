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

namespace TCGGameService
{
    public static class Constants
    {
        public const int TIME_1_SEC = 1000 * 1;
        public const int TIME_5_SEC = 1000 * 5;
        public const int TIME_10_SEC = 1000 * 10;
        public const int TIME_60_SEC = 1000 * 60;
    }

    public class Timer<T>
    {
        public Action<T, object> OnTImer;
        private C5.IntervalHeap<TImer_data> regist_timer = new C5.IntervalHeap<TImer_data>();
        //////////////////////////////////////////////////////////////////////////////////////////////////
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //////////////////////////////////////////////////////////////////////////////////////////////////
        private class TImer_data : IComparable<TImer_data>
        {
            public T timer_id { get; private set; }
            public int dueTime { get; private set; }
            public bool keep { get; private set; }
            public object data { get; private set; }
            public DateTime next { get; private set; }
            public TImer_data(T timer_id, int dueTime, bool keep, object data)
            {
                this.timer_id = timer_id;
                this.dueTime = dueTime;
                this.keep = keep;
                this.data = data;
                this.next = DateTime.Now.AddMilliseconds(dueTime);
            }
            public int CompareTo(TImer_data other)
            {
                return next.CompareTo(other.next);
            }
        }

        public Timer()
        {
        }
        public void Update()
        {
            if (OnTImer == null)
                return;

            var now = DateTime.Now;
            var next = new List<TImer_data>();
            while (!regist_timer.IsEmpty)
            {
                if (regist_timer.FindMin().next < now)
                    next.Add(regist_timer.DeleteMin());
                else
                    break;
            }

            foreach (var it in next)
            {
                try
                {
                    OnTImer(it.timer_id, it.data);
                }
                catch (Exception ex)
                {
                    logger.Error($"ex={ex.ToString()}");
                }
                if (it.keep)
                    AddTimer(it.timer_id, it.dueTime, it.keep, it.data);
            }
        }
        public bool AddTimer(T timer_id, int dueTime, bool keep, object data = null)
        {
            if (OnTImer == null)
            {
                logger.Error("OnTImer is null");
                return false;
            }

            var result = regist_timer.Add(new TImer_data(timer_id, dueTime, keep, data));
            if (!result)
                logger.Error($"regist_timer.Add failed, timer_id={timer_id}, dueTime={dueTime}, keep={keep}");

            return result;
        }
    }
}
