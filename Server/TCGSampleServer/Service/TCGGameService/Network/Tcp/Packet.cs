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

using Newtonsoft.Json;

namespace TCGGameService
{
	public sealed class Packet : TcpNet.Packet.NetPacket
	{
		public int size = 0;
		public short reserve1 = 0;
		public short reserve2 = 0;

		public Packet(TcpMsg.MessageResponse msg)
		{
			Write(reserve1);
			Write(reserve2);

            Write(JsonConvert.SerializeObject(msg));
        }

        public Packet(byte[] buffer)
            : base(buffer)
        {
            size = Read<int>();
            reserve1 = Read<short>();
            reserve2 = Read<short>();
        }
    }
}