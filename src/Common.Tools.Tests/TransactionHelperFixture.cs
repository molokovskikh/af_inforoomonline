using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using NHibernate;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Common.Tools.Test
{
	[TestFixture]
	public class TransactionHelperFixture
	{
		private MockRepository repository;
		private ITransaction transaction;

		[SetUp]
		public void Setup()
		{
			repository = new MockRepository();
			transaction = repository.CreateMock<ITransaction>();
		}

		[Test]
		[ExpectedException(typeof (Exception), UserMessage = "TestException")]
		public void WithTransactionRollbackOnExceptionTest()
		{
			transaction.Rollback();
			repository.ReplayAll();

			try
			{
				TransactionHelper.InTransaction(transaction,
				                                delegate { throw new Exception("TestException"); });
			}
			finally
			{
				repository.VerifyAll();
			}
		}

		[Test]
		public void WithTransactionCommit()
		{
			transaction.Commit();
			repository.ReplayAll();

			TransactionHelper.InTransaction(transaction, 
											delegate
												{
			                                		
												});

			repository.VerifyAll();
		}

		[Test]
		public void WihtTransactionResult()
		{
			Assert.That(TransactionHelper.InTransaction<int>(transaction, delegate { return 50; }), Is.EqualTo(50));
		}
	}
}