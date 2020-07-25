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

namespace TCGGameService.LBD.Msg
{
    public class LBDMsg_TokenTypeInfo_Fungible : LBDBaseMsg
    {
        public LBDMsg_TokenTypeInfo_Fungible()
        {
            msgType = this.GetType();
        }

        public Int32 limit;
        public string orderBy;
        public Int32 page;
    }
}
