namespace Common.Models.Repositories
{
	public interface IRepository<T>
	{
		void Save(T item);
		T Get(object id);
	}
}