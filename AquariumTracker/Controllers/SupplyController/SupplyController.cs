using AquariumTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AquariumTracker.Controllers.OwnerController
{
    public class SupplyController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public SupplyController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        private List<SelectListItem> GetAquariumSelector()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT AquariumId, AquariumName, FirstName, LastName FROM Aquarium
                                            INNER JOIN AquariumOwner ON Aquarium.AquariumOwnerId = AquariumOwner.AquariumOwnerId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    DataTable ownersTable = new DataTable("Owners");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(ownersTable);
                    _con.Close();

                    var ownerList = (from rw in ownersTable.AsEnumerable()
                                         select new SelectListItem()
                                         {
                                             Value = Convert.ToString(rw["AquariumId"]),
                                             Text = Convert.ToString(rw["FirstName"] + " " + rw["LastName"] + " - " + rw["AquariumName"])
                                         }).ToList();

                    var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
                    if (selectedAquarium == "")
                    {
                        selectedAquarium = "1";
                        HttpContext.Session.SetInt32("aquariumId", 1);

                    }
                    foreach (var owner in ownerList)
                    {
                        if (owner.Value == selectedAquarium)
                            owner.Selected = true;
                    }

                    return ownerList;
                }
            }            
        }

        public string GetSelectedAquariumOwnerId()
        {
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT AquariumOwnerId FROM Aquarium WHERE AquariumId = @AquariumId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    _con.Open();
                    var ownerId =_cmd.ExecuteScalar().ToString();
                    _con.Close();
                    return ownerId;
                }
            }
        }
        

        public IActionResult Index()
        {
            var ownerId = GetSelectedAquariumOwnerId();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Supply WHERE AquariumOwnerId = @OwnerId ORDER BY AmountRemaining DESC";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@OwnerId", ownerId);

                    DataTable suppliesTable = new DataTable("Supplies");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(suppliesTable);
                    _con.Close();

                    var ownerList = (from rw in suppliesTable.AsEnumerable()
                                    select new Supply()
                                    {
                                        SupplyId = Convert.ToInt32(rw["SupplyId"]),
                                        AquariumOwnerId = Convert.ToInt32(rw["AquariumOwnerId"]),
                                        Name = Convert.ToString(rw["Name"]),
                                        AmountRemaining = Convert.ToInt32(rw["AmountRemaining"])
                                    }).ToList();

                    var vm = new SuppliesViewModel { Owners = GetAquariumSelector(), Supplies = ownerList };
                    return View(vm);
                }
            }
        }

        public ActionResult EditSupply(int supplyId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Supply WHERE SupplyId = @supplyId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@supplyId", supplyId);

                    _con.Open();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var supply = new Supply { SupplyId = reader.GetInt32("SupplyId"), AquariumOwnerId = reader.GetInt32("AquariumOwnerId"), Name = reader.GetString("Name"), AmountRemaining = reader.GetInt32("AmountRemaining") };
                            _con.Close();
                            return View(new EditSupplyViewModel { Owners = GetAquariumSelector(), Supply = supply });
                        }
                        return View(new EditSupplyViewModel { Owners = GetAquariumSelector(), Supply = new Supply() });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditSupply(Supply supply)
        {
            UpsertSupply(supply);
            return RedirectToAction("Index");
        }

        public IActionResult UpsertSupply(Supply supply)
        {
            var aquariumOwnerId = GetSelectedAquariumOwnerId();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if (supply.SupplyId == 0)
                    queryStatement = @"INSERT INTO Supply VALUES (@AquariumOwnerId, @Name, @AmountRemaining)";
                else
                    queryStatement = @"UPDATE Supply SET AquariumOwnerId = @AquariumOwnerId, Name = @Name, AmountRemaining = @AmountRemaining WHERE SupplyId = @SupplyId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@SupplyId", supply.SupplyId);
                    _cmd.Parameters.AddWithValue("@AquariumOwnerId", aquariumOwnerId);
                    _cmd.Parameters.AddWithValue("@Name", supply.Name);
                    _cmd.Parameters.AddWithValue("@AmountRemaining", supply.AmountRemaining);

                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();

                    return RedirectToAction("Index");
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
