using System;
using System.Collections.Generic;
using System.Data;
using Common.Tools;

namespace Common.Models.Repositories
{
	public class OfferRepository : BaseRepository, IOfferRepository
	{
		public IList<Offer> FindAllForSmartOrder(Client client)
		{
			return TransactionHelper.InTransaction(
					CurrentSession.BeginTransaction(IsolationLevel.RepeatableRead),
					delegate
						{
							using (InvokeGetOffers(client))
								return CurrentSession.CreateSQLQuery(@"
SELECT  Offer.Id as {Offer.Id}, 
		Offer.ProductId as {Offer.ProductId},
		Offer.CodeFirmCr as {Offer.CodeFirmCr},
		Offer.SynonymCode as {Offer.SynonymCode},
		Offer.SynonymFirmCrCode as {Offer.SynonymFirmCrCode},
		cast(if(Offer.Quantity = '' or Offer.Quantity is null or Offer.Quantity < 0, 999999, Offer.Quantity) as UNSIGNED) as {Offer.Quantity},
		Offer.Code as {Offer.Code}, 
		Offer.CodeCr as {Offer.CodeCr},
		Offer.Junk as {Offer.Junk},
		Offer.Await as {Offer.Await},
		c.Cost as {Offer.Cost},
		c.PriceCode as {Offer.PriceList},
		Offer.RequestRatio as {Offer.RequestRatio},
		Offer.OrderCost as {Offer.OrderCost},
		Offer.MinOrderCount as {Offer.MinOrderCount}
FROM usersettings.Core c
	JOIN Farm.Core0 as Offer ON c.Id = Offer.Id
WHERE Offer.Junk = 0  
ORDER BY Offer.ProductId, c.cost;")
										.AddEntity("Offer", typeof (Offer))
										.List<Offer>();
						}
					);
		}

		public IList<Offer> FindAllForSmartOrderWithCategory(Client client)
		{
				return TransactionHelper.InTransaction(
					CurrentSession.BeginTransaction(IsolationLevel.RepeatableRead),
					delegate
						{
							using (InvokeGetOffers(client))
								return CurrentSession.CreateSQLQuery(@"
SELECT  Offer.Id as {Offer.Id}, 
		Offer.ProductId as {Offer.ProductId},
		Offer.CodeFirmCr as {Offer.CodeFirmCr},
		Offer.SynonymCode as {Offer.SynonymCode},
		Offer.SynonymFirmCrCode as {Offer.SynonymFirmCrCode},
		cast(if(Offer.Quantity = '' or Offer.Quantity is null or Offer.Quantity < 0, 999999, Offer.Quantity) as UNSIGNED) as {Offer.Quantity},
		Offer.Code as {Offer.Code}, 
		Offer.CodeCr as {Offer.CodeCr},
		Offer.Junk as {Offer.Junk},
		Offer.Await as {Offer.Await},
		c.Cost as {Offer.Cost},
		c.PriceCode as {Offer.PriceList},
		Offer.RequestRatio as {Offer.RequestRatio},
		Offer.OrderCost as {Offer.OrderCost},
		Offer.MinOrderCount as {Offer.MinOrderCount}
FROM usersettings.Core c
	JOIN Farm.Core0 as Offer ON c.Id = Offer.Id
		JOIN usersettings.activeprices ap on ap.pricecode = c.pricecode 
WHERE Offer.Junk = 0 
ORDER BY Offer.ProductId, ap.firmcategory DESC, c.cost;")
										.AddEntity("Offer", typeof (Offer))
										.List<Offer>();
						});
		}

		public IList<Offer> FindByIds(Client client, IEnumerable<ulong> ids)
		{
				return TransactionHelper.InTransaction(
					CurrentSession.BeginTransaction(IsolationLevel.RepeatableRead),
					delegate
						{
							using (InvokeGetOffers(client))
								return CurrentSession.CreateSQLQuery(@"
SELECT  Offer.Id as {Offer.Id}, 
		Offer.ProductId as {Offer.ProductId},
		Offer.CodeFirmCr as {Offer.CodeFirmCr},
		Offer.SynonymCode as {Offer.SynonymCode},
		Offer.SynonymFirmCrCode as {Offer.SynonymFirmCrCode},
		cast(if(Offer.Quantity = '' or Offer.Quantity is null or Offer.Quantity < 0, 999999, Offer.Quantity) as UNSIGNED) as {Offer.Quantity},
		Offer.Code as {Offer.Code}, 
		Offer.CodeCr as {Offer.CodeCr},
		Offer.Junk as {Offer.Junk},
		Offer.Await as {Offer.Await},
		c.Cost as {Offer.Cost},
		c.PriceCode as {Offer.PriceList},
		Offer.RequestRatio as {Offer.RequestRatio},
		Offer.OrderCost as {Offer.OrderCost},
		Offer.MinOrderCount as {Offer.MinOrderCount}
FROM usersettings.Core c
	JOIN Farm.Core0 as Offer ON c.Id = Offer.Id
		JOIN usersettings.activeprices ap on ap.pricecode = c.pricecode
WHERE Offer.Id in (:ids);
")
										.AddEntity("Offer", typeof (Offer))
										.SetParameterList("ids", ids)
										.List<Offer>();
						});
		}

		private static IDisposable InvokeGetOffers(Client client)
		{
			using (var command = CurrentSession.Connection.CreateCommand())
			{
				command.CommandText = String.Format("usersettings.GetOffers");
				command.CommandType = CommandType.StoredProcedure;

				var parameter = command.CreateParameter();
				parameter.ParameterName = "ClientCodeParam";
				parameter.Value = client.FirmCode;
				command.Parameters.Add(parameter);

				parameter = command.CreateParameter();
				parameter.ParameterName = "FreshOnly";
				parameter.Value = false;
				command.Parameters.Add(parameter);
				
				command.ExecuteNonQuery();
				return new DisposibleAction(DropTemporaryTable);
			}
		}

		private static void DropTemporaryTable()
		{
			using(var command = CurrentSession.Connection.CreateCommand())
			{
				command.CommandText = @"
DROP TEMPORARY TABLE IF EXISTS usersettings.Core;
DROP TEMPORARY TABLE IF EXISTS usersettings.ActivePrices;
DROP TEMPORARY TABLE IF EXISTS usersettings.Prices;
DROP TEMPORARY TABLE IF EXISTS usersettings.MinCosts;";
				command.ExecuteNonQuery();
			}
		}
	}
}
