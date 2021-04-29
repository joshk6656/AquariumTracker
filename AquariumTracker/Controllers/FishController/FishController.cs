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

namespace AquariumTracker.Controllers
{
    public class FishController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public FishController(ILogger<HomeController> logger)
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
        public IActionResult Index()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Fish WHERE AquariumId = @AquariumId";
                
                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    DataTable fishTable = new DataTable("Fish");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(fishTable);
                    _con.Close();

                    var fishList = (from rw in fishTable.AsEnumerable()
                                     select new Fish()
                                     {
                                         FishId = Convert.ToInt32(rw["FishId"]),
                                         Name = Convert.ToString(rw["Name"]),
                                         DateAdded = Convert.ToDateTime(rw["DateAdded"]),
                                         DateRemoved = rw.IsNull("DateRemoved") ? (DateTime?)null : Convert.ToDateTime(rw["DateRemoved"]),
                                         Quarantined = Convert.ToBoolean(rw["Quarantined"])
                                     }).ToList();

                    var vm = new FishViewModel { Owners = GetAquariumSelector(), FishList = fishList };
                    return View(vm);
                }
            }            
        }

        public ActionResult EditFish(int fishId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Fish WHERE FishId = @FishId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@FishId", fishId);

                    DataTable fishTable = new DataTable("Fish");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var fish = new Fish { AquariumId = reader.GetInt32("AquariumId"), FishId = reader.GetInt32("FishId"), Name = reader.GetString("Name"), DateAdded = reader.GetDateTime("DateAdded"), DateRemoved = reader.IsDBNull("DateRemoved") ? (DateTime?) null : (DateTime?)reader.GetDateTime("DateRemoved"), Quarantined = reader.GetBoolean("Quarantined") };
                            _con.Close();
                            return View(new EditFishViewModel { Owners = GetAquariumSelector(), Fish = fish });
                        }
                        return View(new EditFishViewModel { Owners = GetAquariumSelector(), Fish = new Fish() });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditFish(Fish fish)
        {
            UpsertFish(fish);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteFish(int fishId)
        {

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "DELETE FROM Fish WHERE FishId = @FishId";
               
                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@FishId", fishId);
                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();
                }
            }
            return Json(true);
        }

        public IActionResult UpsertFish(Fish fish)
        {
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if(fish.FishId == 0)
                    queryStatement = @"INSERT INTO Fish VALUES (@AquariumId, @FishName, @DateAdded, @DateRemoved, @Quarantined)";
                else
                    queryStatement = @"UPDATE Fish SET  Name = @FishName, DateAdded = @DateAdded, DateRemoved = @DateRemoved, Quarantined = @Quarantined WHERE FishId = @FishId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);
                    _cmd.Parameters.AddWithValue("@FishName", fish.Name);
                    _cmd.Parameters.AddWithValue("@DateAdded", fish.DateAdded);
                    _cmd.Parameters.AddWithValue("@DateRemoved", fish.DateRemoved ?? (object)DBNull.Value);
                    _cmd.Parameters.AddWithValue("@Quarantined", fish.Quarantined);
                    _cmd.Parameters.AddWithValue("@FishId", fish.FishId);

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
