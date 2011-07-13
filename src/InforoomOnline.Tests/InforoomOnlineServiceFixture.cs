using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common.Models;
using Common.Models.Repositories;
using Common.Service;
using Common.Service.Interceptors;
using Common.Service.Tests.ForTesting;
using MySql.Data.MySqlClient;
using NHibernate.Criterion;
using NHibernate.Mapping.Attributes;
using NUnit.Framework;
using Order=Common.Models.Order;

namespace InforoomOnline.Tests
{
	[TestFixture]
	public class InforoomOnlineServiceFixture
	{
		private IInforoomOnlineService service;
		private string[] Empty;

		public T[] Array<T>(params T[] items)
		{
			return items;
		}

		[SetUp]
		public void Setup()
		{
			ServiceContext.GetUserName = () => "kvasov";

			var container = new WindsorContainer();
			container.AddComponent("RepositoryInterceptor", typeof(RepositoryInterceptor));
			container.AddComponent("OfferRepository", typeof(IOfferRepository), typeof(OfferRepository));
			container.AddComponent("Repository", typeof(IRepository<>), typeof(Repository<>));
			var holder = new SessionFactoryHolder();
			holder
				.Configuration
				.AddInputStream(HbmSerializer.Default.Serialize(Assembly.Load("Common.Models")));
			container.Kernel.AddComponentInstance<ISessionFactoryHolder>(holder);
			IoC.Initialize(container);
			IoC.Container.Register(
				Component.For<IInforoomOnlineService>().ImplementedBy<InforoomOnlineService>().Interceptors(
					InterceptorReference.ForType<ContextLoaderInterceptor>()
				).Anywhere,
				Component.For<ContextLoaderInterceptor>(),
				Component.For<IClientLoader>().ImplementedBy<ClientLoader>()
			);

			service = IoC.Resolve<IInforoomOnlineService>();
		}

		[Test]
		public void GetNameFromCatalog()
		{
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, false, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new string[0], true, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new[] {"*Тест*"}, new string[0], true, 100, 0));
			LogDataSet(service.GetNamesFromCatalog(new string[0], new[] {"*Тест*"}, true, 100, 0));
		}

		[Test]
		public void Get_offers_with_filter_by_supplier_id()
		{
			var data = service.GetOffers(new[] {"SupplierId"}, new[] {"4598"}, false, null, null, 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0));
		}

		[Test]
		public void GetOffers()
		{
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
			

			LogDataSet(data);
		}

		[Test]
		public void GetPriceList()
		{
			var priceList = service.GetPriceList(new string[0]);
			Assert.That(priceList.Tables[0].Columns.Contains("SupplierId"));
			LogDataSet(priceList);

			LogDataSet(service.GetPriceList(new[] {"%а%"}));
		}

		[Test]
		public void PostOrder()
		{
			var offerRepository = IoC.Resolve<IOfferRepository>();
			var orderRepository = IoC.Resolve<IRepository<Order>>();

			var begin = DateTime.Now;
			var data = service.GetOffers(Array("name"), Array("*папа*"), false, Empty, Empty, 100, 0);
			Assert.That(data.Tables[0].Rows.Count, Is.GreaterThan(0), "не нашли предложений");
			var row = data.Tables[0].Rows[0];
			var coreId = Convert.ToInt64(row["OfferId"]);

			using (var connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString))
			{
				connection.Open();
				var command = new MySqlCommand(@"
update farm.core0
set OrderCost = 10.5,
	MinOrderCount = 50,
	RequestRatio = 10
where id = ?coreid;

delete from orders.ordershead
where clientcode = 2575 and writetime > curdate();", connection);
				command.Parameters.AddWithValue("?CoreId", coreId);
				command.ExecuteNonQuery();
			}

			var result = service.PostOrder(Array(coreId),
											Array(50),
											Array("это тестовый заказ"));

			Assert.That(result.Tables[0].Rows[0]["OfferId"], Is.EqualTo(coreId));
			Assert.That(result.Tables[0].Rows[0]["Posted"], Is.EqualTo(true));

			var offer = offerRepository.GetById(new Client {FirmCode = 2575}, (ulong)coreId);
			var order = orderRepository.FindOne(Expression.Eq("ClientCode", 2575u)
												&& Expression.Ge("WriteTime", begin));
			Assert.That(offer.MinOrderCount, Is.EqualTo(50));
			Assert.That(order.OrderItems[0].CoreId, Is.EqualTo(offer.Id));
			Assert.That(order.OrderItems[0].OrderCost, Is.EqualTo(offer.OrderCost));
			Assert.That(order.OrderItems[0].MinOrderCount, Is.EqualTo(offer.MinOrderCount));
			Assert.That(order.OrderItems[0].RequestRatio, Is.EqualTo(offer.RequestRatio));
		}

		[Test]
		public void All_methods_must_be_marked_with_fault_contract_attribute()
		{
			foreach(var method in typeof(IInforoomOnlineService).GetMethods())
			{
				var finded = false;
				foreach (var attribute in method.GetCustomAttributes(typeof(FaultContractAttribute), true))
				{
					finded = ((FaultContractAttribute) attribute).DetailType == typeof (DoNotHavePermissionFault);
					if (finded)
						break;
				}

				Assert.That(finded, String.Format("Метод {0} не содержит атрибута FaultContract с типом NotHavePermissionException", method.Name));
			}
		}

		public static void LogDataSet(DataSet dataSet)
		{
			foreach (DataTable dataTable in dataSet.Tables)
			{
				Console.WriteLine("<table>");
				foreach (DataRow dataRow in dataTable.Rows)
				{
					Console.WriteLine("\t<row>");
					Console.Write("\t");
					foreach (DataColumn column in dataTable.Columns)
					{
						Console.Write(dataRow[column] + " ");
					}
					Console.WriteLine();
					Console.WriteLine("\t</row>");
				}
				Console.WriteLine("</table>");
			}
		}
	}
}
