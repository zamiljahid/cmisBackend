using cmis.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cmis.Manager
{
    public interface IProfileRepository
    {
        //Task<List<ProfileModel>> GetAllItemsAsync(); 
        ProfileModel GetUserProfileById(string user_id); 
    }
}
