using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace KTechTemplate.Helpers
{
	public static class RouteExtensions
	{
		public static string RecreateCurrentUrl(this IUrlHelperFactory urlHelperFactory, ViewContext viewContext, object routeData = null, string action = null)
		{
			var currentQueryData = viewContext.HttpContext.Request.Query;
			var currentRouteData = viewContext.RouteData.Values;
			var currentController = currentRouteData["controller"] as string;
			var currentAction = currentRouteData["action"] as string;
			var urlHelper = urlHelperFactory.GetUrlHelper(viewContext);
			var newAction = String.IsNullOrWhiteSpace(action) ? currentAction : action;

			var routeValues = routeData == null
				? currentRouteData
				: new RouteValueDictionary(routeData);

			if (routeData == null)
				routeData = new Dictionary<string, string>();

			foreach (string key in currentQueryData.Keys)
			{
				if (!routeValues.ContainsKey(key))
					routeValues.Add(key, currentQueryData[key]);
			}

			return urlHelper.Action(newAction, currentController, routeValues);
		}
	}
}