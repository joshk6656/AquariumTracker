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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        private List<SelectListItem> GetOwners()
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
            var vm = new HomeViewModel { Owners = GetOwners(), Averages = GetTestAverages(), Recommendations = GetRecommendations() };
            return View(vm);
        }

        public List<TestAverages> GetTestAverages()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT Name, ResultAverage = ISNULL(ResultAverage, 'No tests in last 31 days') FROM Test a
                                            OUTER APPLY
                                            (
                                                SELECT ResultAverage = CONVERT(VARCHAR, AVG(Result)) FROM TestResult average WHERE a.TestId = average.TestId and average.AquariumId = @AquariumId AND DATEDIFF(DAY, average.TestDate, GETDATE()) <= 31
                                            ) average
                                        ";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    DataTable fishTable = new DataTable("Fish");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(fishTable);
                    _con.Close();

                    var result = (from rw in fishTable.AsEnumerable()
                                    select new TestAverages()
                                    {
                                        TestName = Convert.ToString(rw["Name"]),
                                        Result = Convert.ToString(rw["ResultAverage"]) == "No tests in last 31 days" ? "No tests in last 31 days" : Math.Round(Convert.ToDecimal(rw["ResultAverage"]), 2).ToString()
                                    }).ToList();

                    return result;
                }
            }
        }

        public List<Recommendation> GetRecommendations()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"EXEC dbo.GetDashboard @AquariumId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    DataTable recommendations = new DataTable("Recommendations");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(recommendations);
                    _con.Close();

                    var result = (from rw in recommendations.AsEnumerable()
                                  select new Recommendation()
                                  {
                                      Text = Convert.ToString(rw["Recommendation"]),
                                      Severity = Convert.ToInt32(rw["Severity"])
                                  }).ToList();

                    return result;
                }
            }
        }
        public IActionResult ChangeOwner(int ownerId)
        {
            HttpContext.Session.SetInt32("aquariumId", ownerId);
            return Json(true);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
