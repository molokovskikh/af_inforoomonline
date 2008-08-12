using System;
using Common.Tools;

namespace Common.MySql
{
	public class CleanUp
	{
		public static IDisposable AfterGetOffers(MySqlHelper helper)
		{
			return new DisposibleAction(helper.Command(@"
DROP TEMPORARY TABLE IF EXISTS usersettings.Core;
DROP TEMPORARY TABLE IF EXISTS usersettings.ActivePrices;
DROP TEMPORARY TABLE IF EXISTS usersettings.Prices;
DROP TEMPORARY TABLE IF EXISTS usersettings.MinCosts;")
			                                .Execute);
		}

		public static IDisposable AfterGetPrices(MySqlHelper helper)
		{
			return new DisposibleAction(helper.Command("DROP TEMPORARY TABLE IF EXISTS usersettings.Prices;")
			                                .Execute);
		}

		public static IDisposable AfterGetActivePrices(MySqlHelper helper)
		{
			return new DisposibleAction(helper.Command(
@"
DROP TEMPORARY TABLE IF EXISTS usersettings.ActivePrices;
DROP TEMPORARY TABLE IF EXISTS usersettings.Prices;
")
			                                .Execute);
		}
	}
}
