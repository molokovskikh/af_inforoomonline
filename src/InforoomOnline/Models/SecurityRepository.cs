using Common.Models;
using Common.Models.Repositories;

namespace InforoomOnline.Models
{
	public interface ISecurityRepository
	{
		bool HavePermission(string permission, string userName);
	}

	public class SecurityRepository : BaseRepository, ISecurityRepository
	{
		public bool HavePermission(string userName, string permission)
		{
			var command = UnitOfWork.Current.CurrentSession.Connection.CreateCommand();
			command.CommandText = @"
select up.id
from usersettings.UserPermissions up
  join usersettings.AssignedPermissions ap on up.id = ap.permissionid
    join usersettings.osuseraccessright ouar on ouar.rowid = ap.userid
where up.Shortcut = ?Permission and ouar.osusername = ?UserName";

			var userNameParameter = command.CreateParameter();
			userNameParameter.ParameterName = "?UserName";
			userNameParameter.Value = userName;
			command.Parameters.Add(userNameParameter);

			var permissionParameter = command.CreateParameter();
			permissionParameter.ParameterName = "?Permission";
			permissionParameter.Value = permission;
			command.Parameters.Add(permissionParameter);

			return command.ExecuteScalar() != null;
		}
	}
}
