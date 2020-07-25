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
using System.Linq;

namespace TCGGameService
{
    public partial class TCGGameSrv
    {
        public static Ioc.Core.IContainer IocContainer;
        public string dataBaseName = string.Empty;

        string GetDataBaseName()
        {
            dataBaseName = Setting.ProgramSetting.Instance.mysqlInfo.database;
            if (string.IsNullOrEmpty(dataBaseName))
            {
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                dataBaseName = $"{processName}_{System.Environment.MachineName}";
            }

            logger.Info($"database_name={dataBaseName}");

            return dataBaseName;
        }

        void SetDbString(string connectionString, string connectionStringWithDatabase)
        {
            DataBase.Setting.ConnectionString = connectionString;
            DataBase.Setting.ConnectionStringWithDatabse = connectionStringWithDatabase;
        }

        void SetIocContainer()
        {
            IocContainer = new Ioc.Container();
            IocContainer.RegisterInstanceType<Repository.IUser, Repository.User>();
            IocContainer.RegisterInstanceType<Repository.ITrading, Repository.Trading>();
        }

        void InitializeDataBase()
        {
            Setting.MysqlInfo mysqlInfo = Setting.ProgramSetting.Instance.mysqlInfo;
            GetDataBaseName();
            SetIocContainer();
            SetDbString(mysqlInfo.GetConnectionString(), 
                        mysqlInfo.GetConnectionStringWithDatabase());

            if (mysqlInfo.IsDataBaseCreate())
                CreateDatabase();
            else
                ConnectDatabase();

        }

        void CreateDatabase()
        {
            DataBase.Setting.DatabaseName = dataBaseName;
            DataBase.Database.Connection();

            Type type = typeof(TCGGameSrv);
            Type[] tblTypes = type.Assembly.GetTypes().Where(t => t.IsClass
                                    && t.Namespace == "TCGGameService.Table"
                                    && t.GetCustomAttributes(true).Where(x => x is Dapper.Contrib.Extensions.TableAttribute).Count() > 0
                                ).ToArray();

            DataBase.Database.CreateDatabaseEnvironment(tblTypes);
        }

        void ConnectDatabase()
        {
            DataBase.Setting.DatabaseName = dataBaseName;
            DataBase.Database.Connection(DataBase.Setting.DatabaseName);
        }
    }
}
