using cmis.Model;
using System.Collections.Generic;

namespace cmis.Manager
{
    public interface IMenuRepository
    {
        List<MenuModel> GetMenusByRoleId(string role_id);
    }
}
