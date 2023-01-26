using System.ComponentModel.DataAnnotations;
using System.Web;

namespace KTechTemplate.Helpers
{
    public static class Utilities
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null)
                return String.Empty;

            var enumType = value.GetType();
            var enumName = Enum.GetName(enumType, value);
            var member = enumType.GetMember(enumName)[0];

            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);

            return ((DisplayAttribute)attributes[0]).ResourceType != null
                                ? ((DisplayAttribute)attributes[0]).GetName()
                                : ((DisplayAttribute)attributes[0]).Name;
        }

        public static string UpdateQueryString(string urlPath, object routeData = null)
        {
            if (routeData == null)
                return urlPath;

            var index = urlPath.IndexOf("?");
            var path = index >= 0 ? urlPath.Substring(0, index) : urlPath;
            var queryString = index >= 0 ? urlPath.Substring(index) : String.Empty;
            var currentQueryData = HttpUtility.ParseQueryString(queryString);
            var routeValues = new RouteValueDictionary(routeData);

            foreach (var key in routeValues.Keys)
                currentQueryData.Set(key, routeValues[key].ToString());

            return $"{path}?{currentQueryData.ToString()}";
        }
    }
}
