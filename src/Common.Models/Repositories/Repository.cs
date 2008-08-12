namespace Common.Models.Repositories
{
	public class Repository<T> : BaseRepository, IRepository<T>
	{
		public void Save(T item)
		{
			CurrentSession.Save(item);
		}

		public T Get(object id)
		{
			return CurrentSession.Get<T>(id);
		}
	}
}
