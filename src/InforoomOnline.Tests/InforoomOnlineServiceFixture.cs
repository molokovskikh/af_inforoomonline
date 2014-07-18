using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using Castle.ActiveRecord;
using Common.Models;
using Common.Models.Repositories;
using Common.Service;
using Common.Tools;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;
using Test.Support.Suppliers;
using Order = Common.Models.Order;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class InforoomOnlineServiceFixture : BaseFixture
	{
		[Test]
		public void GetNameFromCatalog()
		{
			session.Transaction.Commit();
			service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0);
			service.GetNamesFromCatalog(new[] { "*Тест*" }, new string[0], false, 100, 0);
			service.GetNamesFromCatalog(new string[0], new[] { "*Тест*" }, false, 100, 0);
			service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0);
			service.GetNamesFromCatalog(new[] { "*Тест*" }, new string[0], true, 100, 0);
			service.GetNamesFromCatalog(new string[0], new[] { "*Тест*" }, true, 100, 0);
		}

		[Test]
		public void Get_offers_with_filter_by_supplier_id()
		{
			session.Transaction.Commit();
			var priceId = user.GetActivePricesNaked(session).First(p => p.PositionCount > 1).Price.Supplier.Id;
			var data = service.GetOffers(new[] { "SupplierId" }, new[] { priceId.ToString() }, false, null, null, 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0));
		}

		[Test]
		public void GetOffers()
		{
			session.Transaction.Commit();
			var data = service.GetOffers(null, null, false,
				new string[0], new string[0], 100, 0);

			var table = data.Tables[0];
			Assert.That(table.Rows.Count, Is.GreaterThan(0));
			var columns = table.Columns;
			Assert.That(columns.Contains("RequestRatio"));
			Assert.That(columns.Contains("MinOrderSum"));
			Assert.That(columns.Contains("MinOrderCount"));
			Assert.That(columns.Contains("RegistryCost"));
			Assert.That(columns.Contains("SupplierId"));
		}

		[Test]
		public void GetPriceList()
		{
			session.Transaction.Commit();
			var priceList = service.GetPriceList(new string[0]);
			Assert.That(priceList.Tables[0].Columns.Contains("SupplierId"));
			service.GetPriceList(new[] { "%а%" });
		}

		[Test]
		public void PostOrder()
		{
			session.Transaction.Commit();
			Assert.That(client.Addresses.Count, Is.EqualTo(2), "для того что бы тест удался адресов должно быть два");
			var offerRepository = IoC.Resolve<IOfferRepository>();

			var data = service.GetOffers(new[] { "name" }, new[] { "*" }, false, null, null, 100, 0);

			session.Transaction.Begin();
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0), "не нашли предложений");
			var row = data.Tables[0].Rows[0];
			var coreId = Convert.ToInt64(row["OfferId"]);
			var core = TestCore.Find((ulong)coreId);
			core.MinOrderCount = 50;
			session.Save(core);

			session.Transaction.Commit();
			var result = service.PostOrder(new[] { coreId },
				new[] { 50 },
				new[] { "это тестовый заказ" },
				address.Id);

			Assert.That(result.Tables[0].Rows[0]["OfferId"], Is.EqualTo(coreId));
			Assert.That(result.Tables[0].Rows[0]["Posted"], Is.EqualTo(true));

			var offer = offerRepository.GetById(new User { Id = user.Id }, (ulong)coreId);
			var order = TestOrder.Queryable.Single(o => o.Client == client);
			Assert.That(offer.MinOrderCount, Is.EqualTo(50));
			Assert.That(order.Address.Id, Is.EqualTo(address.Id));
			Assert.That(order.Items[0].CoreId, Is.EqualTo(offer.Id.CoreId));
			Assert.That(order.Items[0].OrderCost, Is.EqualTo(offer.OrderCost));
			Assert.That(order.Items[0].MinOrderCount, Is.EqualTo(offer.MinOrderCount));
			Assert.That(order.Items[0].RequestRatio, Is.EqualTo(offer.RequestRatio));
		}

		[Test]
		public void GetMinReqSettings()
		{
			session.Transaction.Commit();
			var settings = service.GetMinReqSettings();
			Assert.That(settings.Tables[0].Columns.Contains("MinReq"), Is.True);
		}

		[Test]
		public void Map_catalog_id()
		{
			var clientCatalogId = Generator.Random().First();
			var product = session.Query<TestProduct>().First();

			var mapSupplier = TestSupplier.CreateNaked(session, TestRegion.Inforoom);
			mapSupplier.Prices[0].PriceType = PriceType.Assortment;
			var core = mapSupplier.AddCore(product);
			core.Code = clientCatalogId.ToString();
			session.Save(mapSupplier);

			client.Settings.CatalogMapPrice = mapSupplier.Prices[0];

			var supplier = TestSupplier.CreateNaked(session);
			supplier.AddFullCore(session, product);
			session.Save(supplier);

			user.Client.MaintainIntersection(session);
			session.Transaction.Commit();

			var data = service.GetOffers(new[] { "fullcode" }, new[] { product.CatalogProduct.Id.ToString() }, false,
				new string[0], new string[0], 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0),
				"код каталожного товара {0} пользователь {1}", product.CatalogProduct.Id, user.Id);
			Assert.That(data.Tables[0].Rows[0]["ClientCatalogId"], Is.EqualTo(clientCatalogId.ToString()),
				"код каталожного товара {0} пользователь {1}", product.CatalogProduct.Id, user.Id);
		}

		[Test]
		public void All_methods_must_be_marked_with_fault_contract_attribute()
		{
			foreach (var method in typeof(IInforoomOnlineService).GetMethods()) {
				var finded = false;
				foreach (var attribute in method.GetCustomAttributes(typeof(FaultContractAttribute), true)) {
					finded = ((FaultContractAttribute)attribute).DetailType == typeof(DoNotHavePermissionFault);
					if (finded)
						break;
				}

				Assert.That(finded, String.Format("Метод {0} не содержит атрибута FaultContract с типом NotHavePermissionException", method.Name));
			}
		}

		[Test]
		public void Get_address()
		{
			session.Transaction.Commit();
			var data = service.GetAddresses().Tables[0];
			Assert.That(data.Rows.Count, Is.GreaterThan(0));
		}

		[Test]
		public void Get_waybill()
		{
			var supplier = TestSupplier.CreateNaked(session);
			var log = new TestDocumentLog(supplier, client);
			session.Save(log);
			log.CreateFile(ConfigurationManager.AppSettings["DocPath"], "test");
			session.Transaction.Commit();
			var data = service.GetWaybills(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)).Tables[0];
			Assert.That(data.Rows.Count, Is.GreaterThan(0));
			var result = service.GetWaybill(Convert.ToUInt32(data.Rows[0]["Id"]));
			Assert.That(result.Length, Is.GreaterThan(0));
		}
	}
}