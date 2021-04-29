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
    public class AquariumsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public AquariumsController(ILogger<HomeController> logger)
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

        private List<SelectListItem> GetOwnerList()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT AquariumOwnerId, FirstName, LastName FROM AquariumOwner";

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
                                         Value = Convert.ToString(rw["AquariumOwnerId"]),
                                         Text = Convert.ToString(rw["FirstName"] + " " + rw["LastName"])
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
                string queryStatement = @"SELECT * FROM Aquarium a INNER JOIN AquariumOwner b on a.AquariumOwnerId = b.AquariumOwnerId ORDER BY LastName";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    DataTable ownersTable = new DataTable("Aquariums");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(ownersTable);
                    _con.Close();

                    var aquariumList = (from rw in ownersTable.AsEnumerable()
                                    select new Aquarium()
                                    {
                                        AquariumId = Convert.ToInt32(rw["AquariumId"]),
                                        AquariumName = Convert.ToString(rw["AquariumName"]),
                                        OwnerFirstName = Convert.ToString(rw["FirstName"]),
                                        OwnerLastName = Convert.ToString(rw["LastName"]),
                                        Volume = Convert.ToInt32(rw["Volume"]),
                                        StartDate = Convert.ToDateTime(rw["StartDate"]),
                                        EndDate = rw.IsNull("EndDate") ? (DateTime?)null : Convert.ToDateTime(rw["EndDate"]),
                                    }).ToList();

                    var vm = new AquariumsViewModel { Owners = GetAquariumSelector(), AquariumList = aquariumList };
                    return View(vm);
                }
            }
        }

        public ActionResult EditAquarium(int aquariumId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Aquarium WHERE AquariumId = @AquariumId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", aquariumId);

                    _con.Open();
                    var ownerList = GetOwnerList();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ownerId = reader.GetInt32("AquariumOwnerId");
                            var aquarium = new Aquarium { OwnerDropdown = ownerList, AquariumOwnerId = reader.GetInt32("AquariumOwnerId"), AquariumId = reader.GetInt32("AquariumId"), AquariumName = reader.GetString("AquariumName"), Volume = reader.GetInt32("Volume"), StartDate = reader.GetDateTime("StartDate"), EndDate = reader.IsDBNull("EndDate") ? (DateTime?)null : (DateTime?)reader.GetDateTime("EndDate") };
                            _con.Close();
                            return View(new EditAquariumViewModel { Owners = GetAquariumSelector(), Aquarium = aquarium });
                        }
                        return View(new EditAquariumViewModel { Owners = GetAquariumSelector(), Aquarium = new Aquarium { OwnerDropdown = ownerList } });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditAquarium(Aquarium aquarium)
        {
            UpsertAquarium(aquarium);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteAquarium(int aquariumId)
        {

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "DELETE FROM Aquarium WHERE AquariumId = @AquariumId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", aquariumId);
                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();
                }
            }
            return Json(true);
        }

        public IActionResult UpsertAquarium(Aquarium aquarium)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if (aquarium.AquariumId == 0)
                    queryStatement = @"INSERT INTO Aquarium VALUES (@AquariumOwnerId, @AquariumName, @Volume, @StartDate, @EndDate)";
                else
                    queryStatement = @"UPDATE Aquarium SET AquariumOwnerId = @AquariumOwnerId, AquariumName = @AquariumName, Volume = @Volume, StartDate = @StartDate, EndDate = @EndDate WHERE AquariumId = @AquariumId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumOwnerId", aquarium.AquariumOwnerId);
                    _cmd.Parameters.AddWithValue("@AquariumName", aquarium.AquariumName);
                    _cmd.Parameters.AddWithValue("@Volume", aquarium.Volume);
                    _cmd.Parameters.AddWithValue("@StartDate", aquarium.StartDate);
                    _cmd.Parameters.AddWithValue("@EndDate", aquarium.EndDate ?? (object)DBNull.Value);
                    _cmd.Parameters.AddWithValue("@AquariumId", aquarium.AquariumId);

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
