using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using swag.DataStore;
using swag.Model;

namespace swag.Controllers
{
    [Route("api/[controller]")]
    public class CompaniesController : Controller
    {

        public List<Company> Get()
        {
            lock (Database.Instance)
            {
                return Database.Instance.Companies;
            }
        }

    }
    
}
