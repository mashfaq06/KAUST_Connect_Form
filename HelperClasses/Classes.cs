using System;
using System.DirectoryServices;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace KAUSTConnectForm.HelperClasses
{
	public class Classes
	{
		public void SendMail(string Email, string body, string name, string type)
		{
			MailMessage mailmessage = new MailMessage("Messaging Services <no-reply@kaust.edu.sa>", Email);
			mailmessage.Subject = "KAUST Connect Account Request - " + name + " (" + type + ")";
			mailmessage.Body = body;
			mailmessage.IsBodyHtml = true;
			SmtpClient smtpClient = new SmtpClient();
			smtpClient.EnableSsl = true;
			smtpClient.Send(mailmessage);
		}

		public void ITMail(string Email, string body, string name, string type)
		{
			MailMessage mailmessage = new MailMessage("ithelpdesk@kaust.edu.sa", Email);
			mailmessage.Subject = "KAUST Connect Account Request - " + name + " (" + type + ")";
			mailmessage.Body = body;
			mailmessage.IsBodyHtml = true;
			SmtpClient smtpClient = new SmtpClient();
			smtpClient.EnableSsl = true;
			smtpClient.Send(mailmessage);
		}

		public static string Decrypt(string cipherText)
		{
			string EncryptionKey = "qwertyuiopasdflkjhg";
			cipherText = cipherText.Replace(" ", "+");
			cipherText = cipherText.Replace("_", "/");
			cipherText = cipherText.Replace("!", "+");
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			//string decrypt = Encoding.UTF8.GetString(cipherBytes);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(cipherBytes, 0, cipherBytes.Length);
						cs.Close();
					}
					cipherText = Encoding.UTF32.GetString(ms.ToArray());
				}
			}
			return cipherText;
		}

		public static string Encrypt(string clearText)
		{
			string EncryptionKey = "qwertyuiopasdflkjhg";

			byte[] clearBytes = Encoding.UTF32.GetBytes(clearText);
			//string Encrypt = Convert.ToBase64String(clearBytes);
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(clearBytes, 0, clearBytes.Length);
						cs.Close();
					}
					clearText = Convert.ToBase64String(ms.ToArray());
				}
			}
			clearText = clearText.Replace("/", "_");
			clearText = clearText.Replace("+", "!");
			return clearText;
		}

		public SearchResult SearchActiveDirectoryByUserName(string userName)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("givenName"); // First Name

					searcher.PropertiesToLoad.Add("sn"); // Last Name

					searcher.PropertiesToLoad.Add("initials"); //Middle Name

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("company");

					searcher.Filter = "(samaccountname=" + userName + ")";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryByIDno(string ID)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("givenName"); // First Name

					searcher.PropertiesToLoad.Add("sn"); // Last Name

					searcher.PropertiesToLoad.Add("initials"); //Middle Name

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("company");

					searcher.Filter = "(extensionAttribute15=" + ID + ")";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryBymail(string Mail)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("givenName"); // First Name

					searcher.PropertiesToLoad.Add("sn"); // Last Name

					searcher.PropertiesToLoad.Add("initials"); //Middle Name

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("company");

					searcher.Filter = "(mail=" + Mail + ")";

					result = searcher.FindOne();
				}
			}
			return result;
		}


		public SearchResult SearchActiveDirectoryforManager(string Manager)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=STAFF,OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("telephoneNumber"); //Job Title

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.Filter = "(samaccountname=" + Manager + ")";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryforManagerKID(string ManagerKID)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=STAFF,OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("telephoneNumber"); //Job Title

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.Filter = "(extensionAttribute15=" + ManagerKID + ")";

					result = searcher.FindOne();
				}
			}

			return result;
		}

		public SearchResult SearchActiveDirectoryforManagerMail(string ManagerMail)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://OU=STAFF,OU=KAUST USERS,DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("telephoneNumber"); //Job Title

					searcher.PropertiesToLoad.Add("displayname"); // DisplayName

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.Filter = "(mail=" + ManagerMail + ")";

					result = searcher.FindOne();
				}
			}

			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartners(string userName)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectPartnerApprovers,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(samaccountname=" + userName + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartnersMail(string Mail)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectPartnerApprovers,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(mail=" + Mail + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartnersId(string ID)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectPartnerApprovers,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(extensionAttribute15=" + ID + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTApprovers(string userName)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectApprovers,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(samaccountname=" + userName + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartnerApprovers(string userName)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf=CN=KConnectPartnerMgrApprover,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(samaccountname=" + userName + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartnersApproverId(string ID)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectPartnerMgrApprover,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(extensionAttribute15=" + ID + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}

		public SearchResult SearchActiveDirectoryForKAUSTPartnersApproverMail(string Mail)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.PropertiesToLoad.Add("displayName");

					searcher.PropertiesToLoad.Add("title"); //Job Title

					searcher.PropertiesToLoad.Add("department"); // Department

					searcher.PropertiesToLoad.Add("extensionAttribute15"); // KAUST ID

					searcher.PropertiesToLoad.Add("mail"); // Email

					searcher.Filter = "(&(memberOf= CN=KConnectPartnerMgrApprover,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA)(mail=" + Mail + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}


		public static SearchResult SearchActiveDirectoryForViewStatusApprovers1(string userName)
		{
			SearchResult result;
			var CN = "CN=Bridges,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA";
			result = SearchActiveDirectoryForViewStatusApproverList(userName, CN);
			return result;
		}

		public static  SearchResult SearchActiveDirectoryForViewStatusApprovers2(string userName)
		{
			SearchResult result;
			var CN = "CN=IT Service Desk Team,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA";
			result = SearchActiveDirectoryForViewStatusApproverList(userName, CN);
			return result;
		}

		public static SearchResult SearchActiveDirectoryForViewStatusApprovers3(string userName)
		{
			SearchResult result;
			var CN = "CN=Messaging Team,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA";
			result = SearchActiveDirectoryForViewStatusApproverList(userName, CN);
			return result;
		}

		public static SearchResult SearchActiveDirectoryForViewStatusApprovers4(string userName)
		{
			SearchResult result;
			var CN = "IT Windows Team,OU=IT GROUPS,OU=KAUST GROUP,DC=KAUST,DC=EDU,DC=SA";
			result = SearchActiveDirectoryForViewStatusApproverList(userName, CN);
			return result;
		}

		public static SearchResult SearchActiveDirectoryForViewStatusApproverList(string userName, string CN)
		{
			SearchResult result;
			using (var entry = new DirectoryEntry("LDAP://DC=KAUST,DC=EDU,DC=SA"))
			{
				using (var searcher = new DirectorySearcher(entry, "(objectClass=person)"))
				{
					searcher.PropertiesToLoad.Add("samaccountname");

					searcher.Filter = "(&(memberOf=" + CN + ")(samaccountname=" + userName + "))";

					result = searcher.FindOne();
				}
			}
			return result;
		}
	}
}