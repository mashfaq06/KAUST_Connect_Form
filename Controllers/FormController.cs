using KAUSTConnectForm.HelperClasses;
using KAUSTConnectForm.Models;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Web;
using System.Web.Mvc;

namespace KAUSTConnectForm.Controllers
{
	public class FormController : Controller
	{
		// GET: Form
		Classes Classes = new Classes();
		KCEntities KC = new KCEntities();
		static string HttpLink = ConfigurationManager.ConnectionStrings["Link"].ToString();

		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public ActionResult New()
		{
			return View();
		}

		[HttpPost]
		public ActionResult New(Table_NewRequest table_NewRequest, HttpPostedFileBase image)
		{
			try
			{
				if (image != null)
				{
					table_NewRequest.IDimage = new byte[image.ContentLength];
					image.InputStream.Read(table_NewRequest.IDimage, 0, image.ContentLength);
				}
				if (table_NewRequest.LoginDetailsSentTo == null)
				{
					table_NewRequest.LoginDetailsSentTo = table_NewRequest.ReportingMngEmail;
				}
				var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
				var user = currentUser.Remove(0, 6);
				SearchResult value = Classes.SearchActiveDirectoryByUserName(user);
				var Requester = value.Properties.Contains("displayname") ? Convert.ToString(value.Properties["displayname"][0]) : string.Empty;
				var RequesterEmail = value.Properties.Contains("mail") ? Convert.ToString(value.Properties["mail"][0]) : "no-email";
				table_NewRequest.Requester = Requester;
				table_NewRequest.RequesterEmail = RequesterEmail;
				KC.Table_NewRequest.Add(table_NewRequest);
				KC.SaveChanges();
				int id = table_NewRequest.ID;
				string type = table_NewRequest.NewOrRenew;
				string ManagerUsername = table_NewRequest.ManagerUsername;
				string ManagerName = table_NewRequest.ManagerName;
				string ManagerEmail = table_NewRequest.ManagerEmail;
				TempData["Email"] = table_NewRequest.ManagerEmail;
				string FullName = table_NewRequest.FirstName + " " + table_NewRequest.LastName;
				string EncryptedID = Classes.Encrypt(id.ToString());
				string EncryptedManager = Classes.Encrypt(ManagerUsername);
				string ViewLink = String.Format("<a href=\"{0}Home/ManagerApproval?id={1}&mng={2}\">Click here</a>", HttpLink, EncryptedID, EncryptedManager);
				string StatusLink = String.Format("<a href=\"{0}Home/MySubmissions\">Click here</a>", HttpLink);
				string message = "Dear " + ManagerName +
					",<br /><br /> KAUST Connect Account request has been submitted for <font color='darkblue'> <strong>"
					+ FullName + " </strong></font> by <strong>"
					+ Requester + "</strong>,  please review and approve the request by visit URL below: <br /> <br /> "
					+ ViewLink + "<br /> <br /> Note: Make sure all the details are correct and job title is as per job description. <br /> <br /> Regards, <br /> Messaging Team";
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> The KAUST Connect Account Request for <strong>" + FullName + "</strong> has been submitted successfully. <br /> <br />To view the status kindly   " + StatusLink + "<br /> <br /> Regards, <br /> Messaging Team";
				if (RequesterEmail != "no-email")
				{
					Classes.SendMail(RequesterEmail, RequesterMessage, FullName, type);
				}
				Classes.SendMail(ManagerEmail, message, FullName, type);
				return RedirectToAction("Submitted", "Form");
			}
			catch
			{
				return RedirectToAction("ErrorOccured", "Home", null);
			}
		}

		[HttpGet]
		public ActionResult Renew()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Renew(Table_NewRequest table_NewRequest, HttpPostedFileBase image)
		{
			try
			{
				if (image != null)
				{
					table_NewRequest.IDimage = new byte[image.ContentLength];
					image.InputStream.Read(table_NewRequest.IDimage, 0, image.ContentLength);
				}
				if (table_NewRequest.LoginDetailsSentTo == null)
				{
					table_NewRequest.LoginDetailsSentTo = table_NewRequest.ReportingMngEmail;
				}
				var currentUser = System.Web.HttpContext.Current.User.Identity.Name;
				var user = currentUser.Remove(0, 6);
				SearchResult value = Classes.SearchActiveDirectoryByUserName(user);
				var Requester = value.Properties.Contains("displayname") ? Convert.ToString(value.Properties["displayname"][0]) : string.Empty;
				var RequesterEmail = value.Properties.Contains("mail") ? Convert.ToString(value.Properties["mail"][0]) : "no-email";
				table_NewRequest.Requester = Requester;
				table_NewRequest.RequesterEmail = RequesterEmail.ToLower();
				KC.Table_NewRequest.Add(table_NewRequest);
				KC.SaveChanges();
				int id = table_NewRequest.ID;
				string type = table_NewRequest.NewOrRenew;
				string ManagerUsername = table_NewRequest.ManagerUsername;
				string ManagerName = table_NewRequest.ManagerName;
				string ManagerEmail = table_NewRequest.ManagerEmail;
				TempData["Email"] = table_NewRequest.ManagerEmail;
				string FullName = table_NewRequest.FirstName + " " + table_NewRequest.LastName;
				string EncryptedID = Classes.Encrypt(id.ToString());
				string EncryptedManager = Classes.Encrypt(ManagerUsername);
				string ViewLink = String.Format("<a href=\"{0}Home/ManagerApproval?id={1}&mng={2}\">Click here</a>", HttpLink, EncryptedID, EncryptedManager);
				string StatusLink = String.Format("<a href=\"{0}Home/MySubmissions\">Click here</a>", HttpLink);
				string message = "Dear " + ManagerName +
					",<br /><br /> KAUST Connect Account request has been submitted for <font color='darkblue'> <strong>"
					+ FullName + " </strong></font> by <strong>"
					+ Requester + "</strong>,  please review and approve the request by visit URL below: <br /> <br /> "
					+ ViewLink + "<br /> <br /> Note: Make sure all the details are correct and job title is as per job description. <br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail(ManagerEmail, message, FullName, type);
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> The KAUST Connect Account Request for <strong>" + FullName + "</strong> has been submitted successfully. <br /> <br />To view the status kindly   " + StatusLink + "<br /> <br /> Regards, <br /> Messaging Team";
				if (RequesterEmail != "no-email")
				{
					Classes.SendMail(RequesterEmail, RequesterMessage, FullName, type);
				}

				return RedirectToAction("Submitted", "Form");
			}
			catch
			{
				return RedirectToAction("ErrorOccured", "Home", null);
			}

		}

		public ActionResult Submitted()
		{
			return View();
		}


	}

	
}