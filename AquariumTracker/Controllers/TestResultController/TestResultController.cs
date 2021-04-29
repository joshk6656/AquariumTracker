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
    public class TestResultController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        const string connectionString = "CONNECTIONSTRINGHERE";

        public TestResultController(ILogger<HomeController> logger)
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

        private List<SelectListItem> GetTestList()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT TestId, Name FROM Test ORDER BY Name ASC";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {                    
                    DataTable suppliesTable = new DataTable("Supplied");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(suppliesTable);
                    _con.Close();

                    var testList = (from rw in suppliesTable.AsEnumerable()
                                     select new SelectListItem()
                                     {
                                         Value = Convert.ToString(rw["TestId"]),
                                         Text = Convert.ToString(rw["Name"])
                                     }).ToList();

                    return testList;
                }
            }
        }       

        public IActionResult Index()
        {
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT a.*, b.Name FROM TestResult a INNER JOIN Test b ON a.TestId = b.TestId WHERE AquariumId = @AquariumId ORDER BY TestDate DESC";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);

                    DataTable testTable = new DataTable("Tests");

                    SqlDataAdapter _dap = new SqlDataAdapter(_cmd);

                    _con.Open();
                    _dap.Fill(testTable);
                    _con.Close();

                    var testsList = (from rw in testTable.AsEnumerable()
                                    select new Models.TestResult()
                                    {
                                        TestResultId = Convert.ToInt32(rw["TestResultId"]),
                                        TestId = Convert.ToInt32(rw["TestId"]),
                                        TestDate = Convert.ToDateTime(rw["TestDate"]),
                                        TestName = Convert.ToString(rw["Name"]),
                                        ResultValue = Convert.ToDecimal(rw["Result"])
                                    }).ToList();

                    var vm = new TestResultViewModel { Owners = GetAquariumSelector(), TestResults = testsList };
                    return View(vm);
                }
            }
        }

        public ActionResult EditTestResult(int testResultId)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = @"SELECT * FROM TestResult a WHERE TestResultId = @TestResultId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@TestResultId", testResultId);

                    _con.Open();
                    using (var reader = _cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var test = new TestResult() { TestDropdown = GetTestList(), TestResultId = reader.GetInt32("TestResultId"), TestDate = reader.GetDateTime("TestDate"), TestId = reader.GetInt32("TestId"), ResultValue = reader.GetDecimal("Result") };
                            _con.Close();
                            return View(new EditTestResultViewModel { Owners = GetAquariumSelector(), Result = test });
                        }
                        return View(new EditTestResultViewModel { Owners = GetAquariumSelector(), Result = new TestResult { TestDropdown = GetTestList() } });
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult EditTestResult(TestResult result)
        {
            UpsertTestResult(result);
            return RedirectToAction("Index");
        }

        public IActionResult UpsertTestResult(TestResult result)
        {
            var selectedAquarium = HttpContext.Session.GetInt32("aquariumId").ToString();
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "";
                if (result.TestResultId == 0)
                    queryStatement = @"INSERT INTO TestResult VALUES (@AquariumId, @TestId, @TestDate, @Result);";
                else
                    queryStatement = @"UPDATE TestResult SET TestDate = @TestDate, TestId = @TestId, Result = @Result WHERE TestResultId = @TestResultId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@AquariumId", selectedAquarium);
                    _cmd.Parameters.AddWithValue("@TestId", result.TestId);
                    _cmd.Parameters.AddWithValue("@TestDate", result.TestDate);
                    _cmd.Parameters.AddWithValue("@Result", result.ResultValue);
                    _cmd.Parameters.AddWithValue("@TestResultId", result.TestResultId);

                    _con.Open();
                    _cmd.ExecuteNonQuery();
                    _con.Close();

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteTestResult(int testResultId)
        {

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                string queryStatement = "DELETE FROM TestResult WHERE TestResultId = @TestResultId";

                using (SqlCommand _cmd = new SqlCommand(queryStatement, _con))
                {
                    _cmd.Parameters.AddWithValue("@TestResultId", testResultId);
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
