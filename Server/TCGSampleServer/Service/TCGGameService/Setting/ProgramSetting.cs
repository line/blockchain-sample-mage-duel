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
using System.Xml.Linq;

namespace Setting
{
	public class ProgramSetting
	{
        static readonly Lazy<ProgramSetting> instance = new Lazy<ProgramSetting>(() => new ProgramSetting());
        public static ProgramSetting Instance
        {
            get { return instance.Value; }
        }

		public ServerHostInfo serverHostInfo = null;
        public ServerWebHostInfo serverWebHostInfo = null;
        public ResourceInfo resourceInfo = null;
        public MysqlInfo mysqlInfo = null;
		public LBDInfo lbdInfo = null;
		public LDCInfo ldcInfo = null;

		public bool Load(string xmlfilename)
		{
			try
			{
				string fileName = xmlfilename;

				XDocument xdoc = XDocument.Load(fileName);

				SettingServerHostInfo(xdoc);
                SettingServerWebHostInfo(xdoc);
                SettingResourceInfo(xdoc);
                SettingMysql(xdoc);
                SettingLBDInfo(xdoc);
				SettingLDCInfo(xdoc);

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("XML Load Error  {0} : {1} : {2}", ex.Message, ex.Source, ex.StackTrace);
				return false;
			}
		}

		void SettingServerHostInfo(XDocument xdoc)
		{
			XElement elem = xdoc.Root.Element("serverhostinfo");

            serverHostInfo = new ServerHostInfo();
            serverHostInfo.sid = elem.Attribute("id").Value;
            serverHostInfo.host = elem.Attribute("host").Value;
            serverHostInfo.port = Convert.ToInt32(elem.Attribute("port").Value);
            serverHostInfo.keepAliveCheckTime = Convert.ToInt32(elem.Attribute("keepalivechecktime").Value);
            serverHostInfo.sendingCycle = Convert.ToInt32(elem.Attribute("sendingcycle").Value);
        }

        void SettingServerWebHostInfo(XDocument xdoc)
        {
            XElement elem = xdoc.Root.Element("serverwebhostinfo");

            serverWebHostInfo = new ServerWebHostInfo();
            serverWebHostInfo.host = elem.Attribute("host").Value;
            serverWebHostInfo.port = Convert.ToInt32(elem.Attribute("port").Value);
        }

        void SettingResourceInfo(XDocument xdoc)
        {
            XElement elem = xdoc.Root.Element("resourceinfo");

            resourceInfo = new ResourceInfo();
            resourceInfo.path = elem.Attribute("path").Value;
        }

        void SettingMysql(XDocument xdoc)
		{
			XElement elem = xdoc.Root.Element("mysqlinfo");

			mysqlInfo = new MysqlInfo();
			mysqlInfo.server = elem.Attribute("server").Value;
			mysqlInfo.uid = elem.Attribute("uid").Value;
			mysqlInfo.pwd = elem.Attribute("pwd").Value;
			mysqlInfo.database = elem.Attribute("database").Value;
			mysqlInfo.sslmode = elem.Attribute("sslmode").Value;
			mysqlInfo.allow_user_variables = Convert.ToBoolean(elem.Attribute("allow_user_variables").Value);
			mysqlInfo.database_create = Convert.ToBoolean(elem.Attribute("database_create").Value);
		}

		void SettingLBDInfo(XDocument xdoc)
		{
			XElement elem = xdoc.Root.Element("lbdinfo");

            lbdInfo = new LBDInfo();
            lbdInfo.uri = elem.Attribute("uri").Value;
            lbdInfo.dappId = elem.Attribute("dappid").Value;
            lbdInfo.apiKey = elem.Attribute("apikey").Value;
            lbdInfo.apiSecret = elem.Attribute("apisecret").Value;
            lbdInfo.itemTokenContractId = elem.Attribute("itemtoken_contractid").Value;
            lbdInfo.serviceToeknContractId = elem.Attribute("servicetoken_contractid").Value;
            lbdInfo.operatorAddr = elem.Attribute("operaddr").Value;
            lbdInfo.secretKey = elem.Attribute("secretkey").Value;
        }

		void SettingLDCInfo(XDocument xdoc)
		{
			XElement elem = xdoc.Root.Element("ldcinfo");

			ldcInfo = new LDCInfo();
            ldcInfo.uri = elem.Attribute("uri").Value;
		}
    }
}
