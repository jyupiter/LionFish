using LionFishWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LionFishWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer(new ApplicationDbContextInitializer());
        }

		void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();
			
			if (ex is HttpRequestValidationException)
			{
				Response.Clear();
				Response.StatusCode = 200;
				Response.Write(@"
					<html><head><title>HTML Not Allowed</title>
					<script language='JavaScript'><!--
					function back() { history.go(-1); } //--></script></head>
					<body style='font-family: Arial, Sans-serif;'>
					<h1>Oops!</h1>
					<p>When you try to XSS our site</p>
					<img src='https://i.imgur.com/RUdPyQP.jpg'></img>
					<p><a href='javascript:back()'>Go back</a></p>
					</body></html>
					");
				Response.End();
			}
			else if (ex is SqlException)
			{
                Response.Clear();
                Response.StatusCode = 200;
                Response.Write(@"
					<html><head><title>HTML Not Allowed</title>
					<script language='JavaScript'><!--
					function back() { history.go(-1); } //--></script></head>
					<body style='font-family: Arial, Sans-serif;'>
					<h1><i>Notices your attempt at entering an illegal input</i></h1>
					<h1>ÒwÓ What's this?</h1>
					<p>If you're seeing this, you have probably removed the required field from my inputs. Don't do that</p>
					<p><a href='javascript:back()'>I'm sorry</a></p>
					</body></html>
					");
                Response.End();
            }
		}
	}
}
