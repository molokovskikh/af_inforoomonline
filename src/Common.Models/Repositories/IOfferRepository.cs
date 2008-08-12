using System.Collections.Generic;

namespace Common.Models.Repositories
{
	public interface IOfferRepository
	{
		IList<Offer> FindAllForSmartOrder(Client client);
		IList<Offer> FindAllForSmartOrderWithCategory(Client client);
		IList<Offer> FindByIds(Client client, IEnumerable<ulong> ids);
		Offer GetById(Client client, ulong id);
	}
}
