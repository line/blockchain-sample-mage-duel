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

namespace TCGGameService.LBD
{
    public enum TransactionResultType
    {
        None,
        TxHash,
        RequestCommit
    }

    public class TokenTypeInfo
    {
        public string tokenType;
        public string name;
        public string meta;

        public Int32 GetMetaToInt()
        {
            return Convert.ToInt32(meta);
        }
    }

    public class TokenIdInfo
    {
        public string tokenId;
    }

    public class TokenInfo
    {
        public string tokenType;
        public string tokenIdx;
        public string name;
        public string meta;
        public Int32 amount;

        public string GetTokenId()
        {
            return $"{tokenType}{tokenIdx}";
        }

        public Int32 GetResourceId()
        {
            return Convert.ToInt32(meta);
        }

        public Int32 GetTokenIdxNum()
        {
            return Convert.ToInt32(tokenIdx, 16);
        }
    }

    public class ServiceTokenTypeInfo
    {
        public string contractId;
        public string name;
        public string symbol;
        public string imgUri;
        public string meta;
        public string serviceId;
    }

    public class ServiceTokenInfo
    {
        public string contractId;
        public string name;
        public string symbol;
        public Int32 amount;
    }
}
