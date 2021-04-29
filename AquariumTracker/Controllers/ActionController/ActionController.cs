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
    public class ActionController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public ActionController(ILogger<HomeController> logger)
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

        private List<SelectListItem> GetSupplyList()
        {
            var selectedOwner = GetSelectedAquariumOwnerId();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT SupplyId, Name FROM Supply WHERE AquariumOwnerId = @OwnerId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@OwnerId", selectedOwner);
                    
                    DataTable suppliesTable = new DataTable("Supplied");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(suppliesTable);
                    _con.Close();

                    var supplyList = (from rw in suppliesTable.AsEnumerable()
                                     select new SelectListItem()
                                     {
                                         Value = Convert.ToString(rw["SupplyId"]),
                                         Text = Convert.ToString(rw["Name"])
                                     }).ToList();

                    return supplyList;
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
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT a.*, b.Name FROM Action a INNER JOIN Supply b ON a.SupplyId = b.SupplyId WHERE AquariumId = @AquariumId ORDER BY ActionDate DESC";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    DataTable actionsTable = new DataTable("Actions");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(actionsTable);
                    _con.Close();

                    var actionsList = (from rw in actionsTable.AsEnumerable()
                                    select new Models.Action()
                                    {
                                        ActionId = Convert.ToInt32(rw["ActionId"]),
                                        SupplyId = Convert.ToInt32(rw["SupplyId"]),
                                        SupplyName = Convert.ToString(rw["Name"]),
                                        AquariumId = Convert.ToInt32(rw["AquariumId"]),
                                        ActionDate = Convert.ToDateTime(rw["ActionDate"]),
                                        AmountUsed = Convert.ToInt32(rw["AmountUsed"])
                                    }).ToList();

                    var vm = new ActionsViewModel { Owners = GetAquariumSelector(), Actions = actionsList };
                    return View(vm);
                }
            }
        }

        public ActionResult EditAction(int actionId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM Action a WHERE ActionId = @ActionId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@ActionId", actionId);

                    _con.Open();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var action = new Models.Action() { SupplyDropdown = GetSupplyList(),ActionId = reader.GetInt32("ActionId"), SupplyId = reader.GetInt32("SupplyId"), ActionDate = reader.GetDateTime("ActionDate"), AmountUsed = reader.GetInt32("AmountUsed") };
                            _con.Close();
                            return View(new EditActionViewModel { Owners = GetAquariumSelector(), Action = action });
                        }
                        return View(new EditActionViewModel { Owners = GetAquariumSelector(), Action = new Models.Action { SupplyDropdown = GetSupplyList() } });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditAction(Models.Action action)
        {
            UpsertAction(action);
            return RedirectToAction("Index");
        }

        public IActionResult UpsertAction(Models.Action action)
        {
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if (action.ActionId == 0)
                    queryStatement = @"INSERT INTO Action VALUES (@SupplyId, @AquariumId, @DateUsed, @AmountUsed);
                                        UPDATE Supply SET AmountRemaining = AmountRemaining - @AmountUsed WHERE SupplyId = @SupplyId";
                else
                    queryStatement = @"UPDATE Action SET SupplyId = @SupplyId, ActionDate = @DateUsed, AmountUsed = @AmountUsed WHERE ActionId = @ActionId; 
                                        UPDATE Supply SET AmountRemaining = AmountRemaining - @AmountUsed WHERE SupplyId = @SupplyId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@ActionId", action.ActionId);
                    _cmd.Parameters.AddWithValue("@SupplyId", action.SupplyId);
                    _cmd.Parameters.AddWithValue("@DateUsed", action.ActionDate);
                    _cmd.Parameters.AddWithValue("@AmountUsed", action.AmountUsed);
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteAction(int actionId)
        {

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "DELETE FROM Action WHERE ActionId = @ActionId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@ActionId", actionId);
                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();
                }
            }
            return Json(true);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
