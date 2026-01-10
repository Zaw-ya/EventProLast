using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventPro.DAL.Models;
using EventPro.Web.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;


namespace EventPro.Web.Controllers
{
    public partial class AdminController : Controller
    {
        public IActionResult GenerateInvoice(int id)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);

            SetBreadcrum("Generate Invoice", "/admin");
            var events = db.VwEvents.Where(p => p.Id == id).FirstOrDefault();
            var client = db.VwUsers.Where(p => p.UserId == events.CreatedFor).FirstOrDefault();

            ViewBag.EventInfo = events;
            ViewBag.ClientInfo = client;
            ViewBag.FilePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
            if (!db.Invoices.Where(p => p.EventId == id).Any())
            {
                Invoices invoice = new Invoices();
                invoice.EventId = id;
                invoice.EventLocation = events.EventVenue;
                invoice.EventCode = events.GmapCode;
                invoice.InvoiceDate = DateTime.Today;
                invoice.DueDate = DateTime.Today.AddDays(7);
                invoice.BillTo = client.FullName;
                invoice.BillingAddress = client.Address;
                invoice.BillingContactNo = client.PrimaryContactNo;
                invoice.TaxPer = 0;
                db.Invoices.Add(invoice);
                db.SaveChanges();
            }
            var invoices = db.Invoices.Where(p => p.EventId == id).FirstOrDefault();
            var details = db.InvoiceDetails.Where(p => p.InvoiceId == invoices.Id).ToList();
            ViewBag.InvoiceDetails = details;
            invoices.TotalDue = details.Sum(p => p.Total);
            invoices.NetDue = details.Sum(p => p.Total) + (details.Sum(p => p.Total) * invoices.TaxPer / 100);
            ViewBag.Tax = details.Sum(p => p.Total) * invoices.TaxPer / 100;
            var fileName = invoices.EventId + ".pdf";
            var filePath = _configuration.GetSection("Uploads").GetSection("Invoice").Value;
            bool fileExist = System.IO.File.Exists(filePath + "/" + fileName);
            ViewBag.InvoiceExist = fileExist;
            ViewBag.InvoicePath = filePath;
            db.SaveChanges();
            return View(invoices);
        }

        [HttpPost]
        public IActionResult GenerateInvoice(Invoices inv)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);
            int id = Convert.ToInt32(inv.EventId);
            Invoices _invoice = db.Invoices.Where(p => p.EventId == id).FirstOrDefault();
            _invoice.BillTo = inv.BillTo;
            _invoice.BillingContactNo = inv.BillingContactNo;
            _invoice.BillingAddress = inv.BillingAddress;
            _invoice.EventPlace = inv.EventPlace;
            _invoice.EventName = inv.EventName;
            _invoice.TaxPer = inv.TaxPer;
            db.SaveChanges();
            return RedirectToAction("GenerateInvoice", AppController.Admin, new { id = id });
        }
        public IActionResult GeneratePDF(int id)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);

            var events = db.VwEvents.Where(p => p.Id == id).FirstOrDefault();
            var client = db.VwUsers.Where(p => p.UserId == events.CreatedFor).FirstOrDefault();
            var invoices = db.Invoices.Where(p => p.EventId == events.Id).FirstOrDefault();
            var invoiceDetails = db.InvoiceDetails.Where(p => p.InvoiceId == invoices.Id).ToList();
            string rawHTML = System.IO.File.ReadAllText(_configuration.GetSection("Uploads").GetSection("Invoice").Value + "InvoiceTemplate.html");
            string rawLine = System.IO.File.ReadAllText(_configuration.GetSection("Uploads").GetSection("Invoice").Value + "RawLine.html");

            rawHTML = rawHTML.Replace("{{InvoiceNo}}", events.GmapCode);
            rawHTML = rawHTML.Replace("{{Location}}", events.EventVenue);
            rawHTML = rawHTML.Replace("{{CommercialNo}}", invoices.EventName);
            rawHTML = rawHTML.Replace("{{EventDate}}", Convert.ToDateTime(events.EventFrom).ToString("dd-MMM-yyyy"));
            rawHTML = rawHTML.Replace("{{PhoneNo}}", invoices.BillingContactNo);
            rawHTML = rawHTML.Replace("{{EventTitle}}", events.EventTitle);
            rawHTML = rawHTML.Replace("{{EventPlace}}", events.EventVenue);
            rawHTML = rawHTML.Replace("{{EventAddress}}", invoices.EventPlace);
            rawHTML = rawHTML.Replace("{{EventDate}}", Convert.ToDateTime(events.EventFrom).ToString("dd-MMM-yyyy"));
            rawHTML = rawHTML.Replace("{{TaxPer}}", Convert.ToDecimal(invoices.TaxPer).ToString("0.00"));
            rawHTML = rawHTML.Replace("{{Total}}", Convert.ToDecimal(invoices.TotalDue).ToString("0.00"));
            rawHTML = rawHTML.Replace("{{NetTotal}}", Convert.ToDecimal(invoices.NetDue).ToString("0.00"));


            var lm = db.LocallizationMaster.Where(p => p.RegionCode == "UAE").ToList();
            foreach (var l in lm)
            {
                rawHTML = rawHTML.Replace("{{" + l.LabelName + "}}", l.Translation);
            }

            StringBuilder sb = new StringBuilder();
            foreach (var data in invoiceDetails)
            {
                var rl = rawLine;
                rl = rl.Replace("{{Total}}", Convert.ToDecimal(data.Total).ToString("0.00"));
                rl = rl.Replace("{{Rate}}", Convert.ToDecimal(data.Rate).ToString("0.00"));
                rl = rl.Replace("{{NoG}}", data.NoFguest);
                rl = rl.Replace("{{Particulars}}", data.Product);
                sb.Append(rl);
                sb.Append(Environment.NewLine);
            }
            rawHTML = rawHTML.Replace("{{RawData}}", sb.ToString());



            System.IO.File.WriteAllText(_configuration.GetSection("Uploads").GetSection("Invoice").Value + id + ".html", rawHTML);
            StringReader sr = new StringReader(rawHTML);
            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            FileStream file = new FileStream(_configuration.GetSection("Uploads").GetSection("Invoice").Value + id + ".pdf", System.IO.FileMode.OpenOrCreate);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, file);
            pdfDoc.Open();
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
            pdfDoc.Close();
            return RedirectToAction("GenerateInvoice", AppController.Admin, new { id = id });
        }


        public IActionResult PreviewInvoice(int id)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);
            string rawLine = System.IO.File.ReadAllText(_configuration.GetSection("Uploads").GetSection("Invoice").Value + id + ".html");

            ViewBag.Invoice = rawLine;
            return View();
        }

        [HttpPost]
        public IActionResult AddItems(IFormCollection form)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);
            int lineId = Convert.ToInt32(form["lineId"]);
            int eventId = Convert.ToInt32(form["EventId"]);
            int InvoiceId = Convert.ToInt32(form["InvoiceId"]);
            string Product = Convert.ToString(form["Product"]);
            string nog = Convert.ToString(form["nog"]);
            decimal rate = Convert.ToDecimal(form["rate"]);

            if (lineId != 0)
            {
                var det = db.InvoiceDetails.Where(p => p.Idid == lineId).FirstOrDefault();
                det.InvoiceId = InvoiceId;
                det.Product = Product;
                det.NoFguest = nog;
                det.Rate = rate;
                det.Qty = 1;
                det.Total = rate * 1;
            }
            else
            {
                InvoiceDetails details = new InvoiceDetails
                {
                    InvoiceId = InvoiceId,
                    Product = Product,
                    NoFguest = nog,
                    Rate = rate,
                    Qty = 1,
                    Total = rate * 1
                };
                db.InvoiceDetails.Add(details);
            }


            db.SaveChanges();

            return RedirectToAction("GenerateInvoice", AppController.Admin, new { id = eventId });
        }

        [HttpGet]
        public IActionResult DeleteItem(int id)
        {
            if (AppSession.GetSession(this.HttpContext, SesionConstant.UserId) == "")
                return RedirectToAction(AppAction.Index, AppController.Login);

            if (AppSession.GetCurrentUserRole(this.HttpContext) != "Administrator")
                return RedirectToAction(AppAction.Index, AppController.Login);

            var invD = db.InvoiceDetails.Where(p => p.Idid == id).FirstOrDefault();
            var inv = db.Invoices.Where(p => p.Id == invD.InvoiceId).FirstOrDefault();
            db.InvoiceDetails.Remove(invD);
            db.SaveChanges();
            return RedirectToAction("GenerateInvoice", AppController.Admin, new { id = inv.EventId });
        }
    }
}
