using cmis.Model;
using System;
namespace cmis.Manager
{
	public interface ILoginRepository
	{
        Task<List<LoginModel>> GetAllItemsAsync();
        LoginModel GetLoginById(string user_id);

    }
}

