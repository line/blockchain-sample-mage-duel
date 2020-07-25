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

namespace Setting
{
	public class ServerHostInfo
	{
        public string sid { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public int keepAliveCheckTime { get; set; }
        public int sendingCycle { get; set; }
    }

    public class ServerWebHostInfo
    {
        public string host { get; set; }
        public int port { get; set; }
    }

	public class MysqlInfo
	{
		public string server { get; set; }
		public string uid { get; set; }
		public string pwd { get; set; }
		public string database { get; set; }
		public string sslmode { get; set; }
		public bool allow_user_variables { get; set; }
		public bool database_create { get; set; }

		public string GetConnectionString()
		{
			return $"Server={server};Uid={uid};Pwd={pwd};SslMode={sslmode};allow user variables={allow_user_variables}";
		}

		public string GetConnectionStringWithDatabase(bool databaseMachineName = false)
		{
			string connectionStr = string.Empty;
			if (databaseMachineName)
				connectionStr = $"Server={server};Uid={uid};Pwd={pwd};database={database};SslMode={sslmode};allow user variables={allow_user_variables}";
			else
				connectionStr = string.Format("Server={0};Uid={1};Pwd={2};database={3};SslMode={4};allow user variables={5}", server, uid, pwd, "{0}", sslmode, allow_user_variables);

			return connectionStr;
		}

		public bool IsDataBaseCreate()
		{
			return database_create;
		}
	}

    public class ResourceInfo
    {
        public string path { get; set; }
    }

    public class LBDInfo
	{
		public string uri { get; set; }
		public string dappId { get; set; }
		public string apiKey { get; set; }
		public string apiSecret { get; set; }
		public string itemTokenContractId { get; set; }
        public string serviceToeknContractId { get; set; }
        public string operatorAddr { get; set; }
        public string secretKey { get; set; }
    }

    public class LDCInfo
    {
        public string uri { get; set; }
    }
}