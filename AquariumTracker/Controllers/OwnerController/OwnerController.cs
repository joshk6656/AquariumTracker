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
    public class OwnersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public OwnersController(ILogger<HomeController> logger)
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
                string queryStatement = @"SELECT * FROM AquariumOwner ORDER BY LastName";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    DataTable ownersTable = new DataTable("Owners");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(ownersTable);
                    _con.Close();

                    var ownerList = (from rw in ownersTable.AsEnumerable()
                                    select new AquariumOwner()
                                    {
                                        AquariumOwnerId = Convert.ToInt32(rw["AquariumOwnerId"]),
                                        FirstName = Convert.ToString(rw["FirstName"]),
                                        LastName = Convert.ToString(rw["LastName"]),
                                        PhoneNumber = Convert.ToString(rw["PhoneNumber"])
                                    }).ToList();

                    var vm = new AquariumOwnersViewModel { Owners = GetAquariumSelector(), OwnerList = ownerList };
                    return View(vm);
                }
            }
        }

        public ActionResult EditOwner(int ownerId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM AquariumOwner WHERE AquariumOwnerId = @ownerId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@ownerId", ownerId);

                    _con.Open();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var owner = new AquariumOwner { AquariumOwnerId = reader.GetInt32("AquariumOwnerId"), FirstName = reader.GetString("FirstName"), LastName = reader.GetString("LastName"), PhoneNumber = reader.GetString("PhoneNumber") };
                            _con.Close();
                            return View(new EditOwnerViewModel { Owners = GetAquariumSelector(), Owner = owner });
                        }
                        return View(new EditOwnerViewModel { Owners = GetAquariumSelector(), Owner = new AquariumOwner() });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditOwner(AquariumOwner owner)
        {
            UpsertOwner(owner);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteOwner(int ownerId)
        {

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "DELETE FROM AquariumOwner WHERE AquariumOwnerId = @ownerId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@ownerId", ownerId);
                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();
                }
            }
            return Json(true);
        }

        public IActionResult UpsertOwner(AquariumOwner owner)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if (owner.AquariumOwnerId == 0)
                    queryStatement = @"INSERT INTO AquariumOwner VALUES (@FirstName, @LastName, @PhoneNumber)";
                else
                    queryStatement = @"UPDATE AquariumOwner SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber WHERE AquariumOwnerId = @OwnerId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@FirstName", owner.FirstName);
                    _cmd.Parameters.AddWithValue("@LastName", owner.LastName);
                    _cmd.Parameters.AddWithValue("@PhoneNumber", owner.PhoneNumber);
                    _cmd.Parameters.AddWithValue("@OwnerId", owner.AquariumOwnerId);

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
