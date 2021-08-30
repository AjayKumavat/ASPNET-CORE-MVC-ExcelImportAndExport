using CORE_MVC_ExcelImportAndExport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CORE_MVC_ExcelImportAndExport.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            var users = GetUserList();
            return View(users);
        }

        public IActionResult ExportToExcel()
        {
            //Getting users data
            var users = GetUserList();

            //Start Exporting to Excel
            var stream = new MemoryStream();

            using (var xlPackage = new ExcelPackage(stream))
            {
                //Define a worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Users");

                //Styling
                var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
                customStyle.Style.Font.UnderLine = true;
                customStyle.Style.Font.Color.SetColor(Color.Red);

                //First row
                var startRow = 5;
                var row = startRow;

                worksheet.Cells["A1"].Value = "Sample User Export";
                using(var r = worksheet.Cells["A1:C1"])
                {
                    r.Merge = true;
                    r.Style.Font.Color.SetColor(Color.Green);
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
                }

                worksheet.Cells["A4"].Value = "Name";
                worksheet.Cells["B4"].Value = "Email";
                worksheet.Cells["C4"].Value = "Phone";
                worksheet.Cells["A4:C4"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A4:C4"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                row = 5;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Name;
                    worksheet.Cells[row, 2].Value = user.Email;
                    worksheet.Cells[row, 3].Value = user.Phone;

                    //Incrementing the row
                    row++;
                }

                xlPackage.Workbook.Properties.Title = "User List";
                xlPackage.Workbook.Properties.Author = "Ajay Kumawat";

                xlPackage.Save();
            }

            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user.xlsx");
        }


        [HttpGet]
        public IActionResult BatchUserUpload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BatchUserUpload(IFormFile batchUsers)
        {
            if(ModelState.IsValid)
            {
                if(batchUsers?.Length > 0)
                {
                    //Convert to a stream
                    var stream = batchUsers.OpenReadStream();

                    List<User> users = new List<User>();

                    try
                    {
                        using(var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets.First();
                            var rowCount = worksheet.Dimension.Rows;

                            for(var row = 2; row <= rowCount; row++)
                            {
                                try
                                {
                                    var name = worksheet.Cells[row, 1].Value?.ToString();
                                    var email = worksheet.Cells[row, 2].Value?.ToString();
                                    var phone = worksheet.Cells[row, 3].Value?.ToString();

                                    var user = new User()
                                    {
                                        Email = email,
                                        Name = name,
                                        Phone = phone
                                    };

                                    users.Add(user);
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }

                        return View("Index", users);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return View();
        }

        private List<User> GetUserList()
        {
            var users = new List<User>()
            {
                new User
                {
                    Name = "Ajay Kumawat",
                    Email = "ajaykumavat@gmail.com",
                    Phone = "9823982398"
                },
                new User
                {
                    Name = "Donald Duck",
                    Email = "donald.duck@gmail.com",
                    Phone = "9823982323"
                },
                new User
                {
                    Name = "Mickey Mouse",
                    Email = "mickey.mouse@gmail.com",
                    Phone = "9823982343"
                }
            };

            return users;
        }
    }
}
