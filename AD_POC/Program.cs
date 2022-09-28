using System;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

namespace AD_POC
{

    public static class ADExtensionMethods
    {
        public static string GetPropertyValue(this SearchResult sr, string propertyName)
        {
            string ret = string.Empty;

            if (sr.Properties[propertyName].Count > 0)
                ret = sr.Properties[propertyName][0].ToString();

            return ret;
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start..");

            if (AuthenticateUser("yanivl", "Aa123456"))
            {
                Console.WriteLine("Authentication Success");

                GetAdUser("yanivl");

            }
            else
                Console.WriteLine("Authentication Failed");

            GetAllUsers();

            GetAllGroups();

        }

        private static DirectoryEntry GetDirectoryObject()
        {
            DirectoryEntry oDE;
            oDE = new DirectoryEntry("LDAP://10.0.0.9", "onpremapi", "Aa123456", AuthenticationTypes.Secure);
            return oDE;
        }


        private static bool AuthenticateUser(string userName, string password)
        {
            bool ret = false;

            try
            {
                DirectoryEntry de = new DirectoryEntry("LDAP://10.0.0.9", userName, password);
                DirectorySearcher dsearch = new DirectorySearcher(de);
                SearchResult results = null;

                results = dsearch.FindOne();


                ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        private static DirectorySearcher BuildUserSearcher(DirectoryEntry de)
        {
            DirectorySearcher ds = null;

            ds = new DirectorySearcher(de);

            // Full Name
            ds.PropertiesToLoad.Add("name");

            // Email Address
            ds.PropertiesToLoad.Add("mail");

            // First Name
            ds.PropertiesToLoad.Add("givenname");

            // Last Name (Surname)
            ds.PropertiesToLoad.Add("sn");

            // Login Name
            ds.PropertiesToLoad.Add("userPrincipalName");

            // Distinguished Name
            ds.PropertiesToLoad.Add("distinguishedName");

            return ds;
        }

        private static void GetAdUser(string userName)
        {
            DirectorySearcher ds = null;
            DirectoryEntry de = GetDirectoryObject();
            SearchResult sr;

            // Build User Searcher
            ds = BuildUserSearcher(de);
            // Set the filter to look for a specific user
            ds.Filter = "(&(objectCategory=User)(objectClass=person)(name=" + userName + "))";

            sr = ds.FindOne();

            if (sr != null)
            {
                Console.WriteLine("name= " + sr.GetPropertyValue("name"));
                Console.WriteLine("mail= " + sr.GetPropertyValue("mail"));
                Console.WriteLine("givenname= " + sr.GetPropertyValue("givenname"));
                Console.WriteLine("sn= " + sr.GetPropertyValue("sn"));
                Console.WriteLine("userPrincipalName= " + sr.GetPropertyValue("userPrincipalName"));
                Console.WriteLine("distinguishedName= " + sr.GetPropertyValue("distinguishedName"));
            }
        }

        private static void GetAdUserGroups(string userName)
        {
            try
            {
                SearchResultCollection results;
                DirectorySearcher ds = null;
                DirectoryEntry de = GetDirectoryObject();

                ds = new DirectorySearcher(de);
                ds.Filter = "(&(objectCategory=User)(objectClass=person))";

                results = ds.FindAll();

                foreach (SearchResult sr in results)
                {
                    Console.WriteLine("Member of" + sr.GetPropertyValue("memberOf"));

                    Console.WriteLine("-------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private static void GetAllUsers()
        {
            try
            {
                SearchResultCollection results;
                DirectorySearcher ds = null;
                DirectoryEntry de = GetDirectoryObject();

             
                ds = new DirectorySearcher(de);
                ds.Filter = "(&(objectCategory=User)(objectClass=person))";

                results = ds.FindAll();

                foreach (SearchResult sr in results)
                {
                    Console.WriteLine(sr.GetPropertyValue("name"));
                    Console.WriteLine(sr.GetPropertyValue("mail"));
                    Console.WriteLine(sr.GetPropertyValue("givenname"));
                    Console.WriteLine(sr.GetPropertyValue("sn"));
                    Console.WriteLine(sr.GetPropertyValue("userPrincipalName"));
                    Console.WriteLine(sr.GetPropertyValue("distinguishedName"));
                    Console.WriteLine("Member of" +  sr.GetPropertyValue("memberOf"));

                    Console.WriteLine("-------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private static void GetAllGroups()
        {
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = GetDirectoryObject();

            ds = new DirectorySearcher(de);
            // Sort by name
            ds.Sort = new SortOption("name", SortDirection.Ascending);
            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("memberof");
            ds.PropertiesToLoad.Add("member");

            ds.Filter = "(&(objectCategory=Group))";

            results = ds.FindAll();

            foreach (SearchResult sr in results)
            {
                if (sr.Properties["name"].Count > 0)
                    Console.WriteLine(sr.Properties["name"][0].ToString());

                if (sr.Properties["memberof"].Count > 0)
                {
                    Console.WriteLine("  Member of...");
                    foreach (string item in sr.Properties["memberof"])
                    {
                        Console.WriteLine("    " + item);
                    }
                }
                if (sr.Properties["member"].Count > 0)
                {
                    Console.WriteLine("  Members");
                    foreach (string item in sr.Properties["member"])
                    {
                        Console.WriteLine("    " + item);
                    }
                }
            }
        }



    }
}
