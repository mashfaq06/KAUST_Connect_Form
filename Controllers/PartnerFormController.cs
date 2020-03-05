using KAUSTConnectForm.HelperClasses;
using KAUSTConnectForm.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KAUSTConnectForm.Controllers
{
    public class PartnerFormController : Controller
    {
		Classes Classes = new Classes();
		KCEntities KC = new KCEntities();
		static string HttpLink = ConfigurationManager.ConnectionStrings["Link"].ToString();
		// GET: PartnerForm
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
				string ReportingManagerUsername = table_NewRequest.ReportingMngUsername;
				string ReportingManagerName = table_NewRequest.ReportingMngName;
				string ReportingManagerEmail = table_NewRequest.ReportingMngEmail;
				TempData["Email"] = table_NewRequest.ReportingMngEmail;
				string FullName = table_NewRequest.FirstName + " " + table_NewRequest.LastName;
				string EncryptedID = Classes.Encrypt(id.ToString());
				string EncryptedReportingManager = Classes.Encrypt(ReportingManagerUsername);
				string ViewLink = String.Format("<a href=\"{0}PartnerForm/ReportingManagerApproval?id={1}&mng={2}\">Click here</a>", HttpLink, EncryptedID, EncryptedReportingManager);
				string StatusLink = String.Format("<a href=\"{0}Home/MySubmissions\">Click here</a>", HttpLink);
				string message = "Dear " + ReportingManagerName +
					",<br /><br /> KAUST Connect Account request has been submitted for <font color='darkblue'> <strong>"
					+ FullName + " </strong></font> by <strong>"
					+ Requester + "</strong>,  please review and approve the request by visit URL below: <br /> <br /> "
					+ ViewLink + "<br /> <br /> Note: Make sure all the details are correct and job title is as per job description. <br /> <br /> Regards, <br /> Messaging Team";
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> The KAUST Connect Account Request for <strong>" + FullName + "</strong> has been submitted successfully. <br /> <br />To view the status kindly   " + StatusLink + "<br /> <br /> Regards, <br /> Messaging Team";
				if (RequesterEmail != "no-email")
				{
					Classes.SendMail(RequesterEmail, RequesterMessage, FullName, type);
				}
				Classes.SendMail(ReportingManagerEmail, message, FullName, type);
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
				string ReportingManagerUsername = table_NewRequest.ReportingMngUsername;
				string ReportingManagerName = table_NewRequest.ReportingMngName;
				string ReportingManagerEmail = table_NewRequest.ReportingMngEmail;
				TempData["Email"] = table_NewRequest.ReportingMngEmail;
				string FullName = table_NewRequest.FirstName + " " + table_NewRequest.LastName;
				string EncryptedID = Classes.Encrypt(id.ToString());
				string EncryptedReportingManager = Classes.Encrypt(ReportingManagerUsername);
				string ViewLink = String.Format("<a href=\"{0}PartnerForm/ReportingManagerApproval?id={1}&mng={2}\">Click here</a>", HttpLink, EncryptedID, EncryptedReportingManager);
				string StatusLink = String.Format("<a href=\"{0}Home/MySubmissions\">Click here</a>", HttpLink);
				string message = "Dear " + ReportingManagerName +
					",<br /><br /> KAUST Connect Account request has been submitted for <font color='darkblue'> <strong>"
					+ FullName + " </strong></font> by <strong>"
					+ Requester + "</strong>,  please review and approve the request by visit URL below: <br /> <br /> "
					+ ViewLink + "<br /> <br /> Note: Make sure all the details are correct and job title is as per job description. <br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail(ReportingManagerEmail, message, FullName, type);
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

		[HttpGet]
		public ActionResult ReportingManagerApproval(string id, string mng)
		{
			string DecryptedManager = Classes.Decrypt(mng);
			string teamMember = System.Web.HttpContext.Current.User.Identity.Name;
			var user = teamMember.Remove(0, 6);
			var approver = Classes.SearchActiveDirectoryForViewStatusApprovers3(user);
			if (approver != null || teamMember.Contains(DecryptedManager))
			{
				string ID = Classes.Decrypt(id);
				int DecryptedID = Convert.ToInt32(ID);
				TempData["ID"] = DecryptedID;
				Table_NewRequest table = KC.Table_NewRequest.Find(DecryptedID);
				TempData["FullName"] = table.FirstName + " " + table.LastName;
				TempData["ManagerEmail"] = table.ManagerEmail;
				TempData["ReportingManager"] = table.ReportingMngName;
				TempData["ManagerName"] = table.ManagerName;
				TempData["Requester"] = table.Requester;
				TempData["RequesterEmail"] = table.RequesterEmail;
				TempData["Manager"] = table.ManagerUsername;
				TempData["Type"] = table.NewOrRenew;
				if (table == null)
				{
					return View();
				}

				var Action = table.StatusOfRequest;
				if (Action != "Pending Reporting Manager Approval")
				{
					ViewBag.Hide = true;
				}
				return View(table);
			}
			else
			{
				return RedirectToAction("AccessDenied", "Home", null);
			}
		}

		[HttpPost]
		public ActionResult ReportingManagerApproval(Table_NewRequest table_NewRequest, string Submit)
		{
			if (Submit == "Reject")
			{
				table_NewRequest.ApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
				table_NewRequest.ApprovedOn = DateTime.Now.ToString();
				table_NewRequest.StatusOfRequest = "Rejected by Manager";
				table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
				KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
				KC.SaveChanges();
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string FullName = Convert.ToString(TempData["FullName"]);
				string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
				string Requester = Convert.ToString(TempData["Requester"]);
				string Type = Convert.ToString(TempData["Type"]);
				string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by your manager. The reason for rejection is <br /> <font color='red'>" + table_NewRequest.Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);
				return RedirectToAction("ReportingManagerRejected");
			}
			else
			{
				table_NewRequest.ApprovedBy = System.Web.HttpContext.Current.User.Identity.Name;
				table_NewRequest.ApprovedOn = DateTime.Now.ToString();
				table_NewRequest.StatusOfRequest = "Pending Messaging Team Approval";
				table_NewRequest.ID = Convert.ToInt32(TempData["ID"]);
				KC.Entry(table_NewRequest).State = System.Data.Entity.EntityState.Modified;
				KC.SaveChanges();
				string EncryptedID = Classes.Encrypt(table_NewRequest.ID.ToString());
				string FullName = Convert.ToString(TempData["FullName"]);
				string Type = Convert.ToString(TempData["Type"]);
				string ManagerEmail = Convert.ToString(TempData["ManagerEmail"]);
				string Requester = Convert.ToString(TempData["Requester"]);
				string ViewLink = String.Format("<a href=\"{0}Home/TeamApproval?id={1}\">Click here</a>", HttpLink, EncryptedID);
				string message = "Dear Messaging Team, <br /><br /> KAUST Connect Account request has been submitted by <u>" + Requester + " </u> for <strong>" + FullName + "</strong> please review and approve the request by visit URL below: <br /> <br /> " + ViewLink + "<br /> <br /> Regards, <br /> Messaging Team";
				Classes.SendMail("messagingservices@kaust.edu.sa", message, FullName, Type);
				return RedirectToAction("ReportingManagerApproved");

			}

		}

		public ActionResult ReportingManagerApproved()
		{
			return View();
		}

		//public void ReportingManagerApprove(string Job)
		//{
		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string EncryptedID = Classes.Encrypt(ID.ToString());
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	KC.sp_MngApproval(ID, "Pending Manager Approval", Job, ActionBy, DateAndTime);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string ManagerEmail = Convert.ToString(TempData["ManagerEmail"]);
		//	string ReportingManager = Convert.ToString(TempData["ReportingManager"]);
		//	string ManagerName = Convert.ToString(TempData["ManagerName"]);
		//	string Manager = Convert.ToString(TempData["Manager"]);
		//	string EncryptedManager = Classes.Encrypt(Manager);
		//	string ViewLink = String.Format("<a href=\"{0}Home/ManagerApproval?id={1}&mng={2}\">Click here</a>", HttpLink, EncryptedID, EncryptedManager);
		//	string message = "Dear " + ManagerName +
		//		",<br /><br /> KAUST Connect Account request has been submitted for <font color='darkblue'> <strong>"
		//		+ FullName + " </strong></font> and has been approved by <strong>"
		//		+ ReportingManager + "</strong>,  please review and approve the request by visit URL below: <br /> <br /> "
		//		+ ViewLink + "<br /> <br /> Note: Make sure all the details are correct and job title is as per job description. <br /> <br /> Regards, <br /> Messaging Team";
		//	Classes.SendMail(ManagerEmail, message, FullName, Type);
		//}

		//public void ReportingManagerRejection(string Comments)
		//{

		//	int ID = Convert.ToInt32(TempData["ID"]);
		//	string ActionBy = System.Web.HttpContext.Current.User.Identity.Name;
		//	string DateAndTime = DateTime.Now.ToString();
		//	KC.sp_MngRejection(ID, "Rejected by Reporting Manager", ActionBy, DateAndTime, Comments);
		//	string FullName = Convert.ToString(TempData["FullName"]);
		//	string RequesterEmail = Convert.ToString(TempData["RequesterEmail"]);
		//	string Requester = Convert.ToString(TempData["Requester"]);
		//	string Type = Convert.ToString(TempData["Type"]);
		//	string RequesterMessage = "Dear " + Requester + ", <br /> <br /> KAUST Connect Account Request Submitted for <strong>" + FullName + "</strong> has been rejected by your reporting manager. The reason for rejection is <br /> <font color='red'>" + Comments + "</font> <br /> <br /> Regards, <br /> Messaging Team";
		//	Classes.SendMail(RequesterEmail, RequesterMessage, FullName, Type);

		//}

		public ActionResult ReportingManagerRejected()
		{
			return View();
		}
	}
}