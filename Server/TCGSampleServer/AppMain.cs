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
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;

using TCGGameService;

namespace TCGSampleServer
{
	class AppMain
	{
		AutoResetEvent _are = new AutoResetEvent(false);
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public AppMain()
		{
			StringBuilder sb = new StringBuilder(512);
			sb.AppendLine("");
            sb.AppendLine(@" _____  _____  _____  _   _ ");
			sb.AppendLine(@"|_   _||  ___|/  __ \| | | |");
            sb.AppendLine(@"  | |  | |__  | /  \/| |_| |");
            sb.AppendLine(@"  | |  |  __| | |    |  _  |");
            sb.AppendLine(@"  | |  | |___ | \__/\| | | |");
            sb.AppendLine(@"  \_/  \____/  \____/\_| |_/");
            sb.AppendLine("");
            sb.AppendLine(@"  _      _____  _   _  _____     ______  _      _____  _____  _   __ _____  _   _   ___   _____  _   _  ");
            sb.AppendLine(@" | |    |_   _|| \ | ||  ___|    | ___ \| |    |  _  |/  __ \| | / //  __ \| | | | / _ \ |_   _|| \ | | ");
            sb.AppendLine(@" | |      | |  |  \| || |__      | |_/ /| |    | | | || /  \/| |/ / | /  \/| |_| |/ /_\ \  | |  |  \| | ");
            sb.AppendLine(@" | |      | |  | . ` ||  __|     | ___ \| |    | | | || |    |    \ | |    |  _  ||  _  |  | |  | . ` | ");
            sb.AppendLine(@" | |____ _| |_ | |\  || |___     | |_/ /| |____\ \_/ /| \__/\| |\  \| \__/\| | | || | | | _| |_ | |\  | ");
            sb.AppendLine(@" \_____/ \___/ \_| \_/\____/     \____/ \_____/ \___/  \____/\_| \_/ \____/\_| |_/\_| |_/ \___/ \_| \_/ ");
            sb.AppendLine("");
            sb.AppendLine(@" _____  _____  _____     _____   ___  ___  _________  _      _____     _____  _____ ______  _   _  _____ ______ ");
			sb.AppendLine(@"|_   _|/  __ \|  __ \   /  ___| / _ \ |  \/  || ___ \| |    |  ___|   /  ___||  ___|| ___ \| | | ||  ___|| ___ \");
            sb.AppendLine(@"  | |  | /  \/| |  \/   \ `--. / /_\ \| .  . || |_/ /| |    | |__     \ `--. | |__  | |_/ /| | | || |__  | |_/ /");
            sb.AppendLine(@"  | |  | |    | | __     `--. \|  _  || |\/| ||  __/ | |    |  __|     `--. \|  __| |    / | | | ||  __| |    / ");
            sb.AppendLine(@"  | |  | \__/\| |_\ \   /\__/ /| | | || |  | || |    | |____| |___    /\__/ /| |___ | |\ \ \ \_/ /| |___ | |\ \ ");
            sb.AppendLine(@"  \_/   \____/ \____/   \____/ \_| |_/\_|  |_/\_|    \_____/\____/    \____/ \____/ \_| \_| \___/ \____/ \_| \_|");
            sb.AppendLine("");

            Console.WriteLine(sb.ToString());
		}

		public void Run(string[] args)
		{
			try
			{
				if (TCGGameSrv.Instance.Initialize())
				{
					logger.Debug("Run Start");

					string uri = $"http://{TCGGameSrv.Instance.serverWebHostInfo.host}:{TCGGameSrv.Instance.serverWebHostInfo.port}";
					Uri baseAddress = new Uri(uri);
					using (var host = new WebServiceHost(typeof(TCGService), baseAddress))
					{
						host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");

						logger.Info("Server Start: Hosting " + baseAddress);

						host.Open();
						_are.WaitOne();
					}

					logger.Debug("Run End");
				}
				else
				{
					logger.Warn("LinkAPiRelayServer Initialize Fail");
					return;
				}
			}
			catch (Exception ex)
			{
				logger.Warn("Exception: {0} {1}", ex.Message, ex.StackTrace);
			}
			finally
			{
				logger.Debug("Run() ended");
                Console.ReadLine();
            }
		}

		public void Stop()
		{
			_are.Set();
		}
	}
}
