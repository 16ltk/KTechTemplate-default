using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Routing;

namespace KTechTemplate.Helpers
{
	[HtmlTargetElement("th", Attributes = SortFieldAttributeName + "," + CurrentSortAttributeName)]
	public class SortableColumnTagHelper : TagHelper
	{
		private const string SortFieldAttributeName = "sort-field";
		private const string CurrentSortAttributeName = "sort-current";

		[HtmlAttributeName(SortFieldAttributeName)]
		public string SortField { get; set; }
		[HtmlAttributeName(CurrentSortAttributeName)]
		public string CurrentSort { get; set; }

		[ViewContext]
		public ViewContext ViewContext { get; set; }

		private readonly IUrlHelperFactory urlHelperFactory;

		public SortableColumnTagHelper(IUrlHelperFactory urlHelperFactory)
		{
			this.urlHelperFactory = urlHelperFactory;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var routeData = new RouteValueDictionary();

			var i = new TagBuilder("i");


			if (String.Equals(SortField + "_asc", CurrentSort, StringComparison.OrdinalIgnoreCase))
			{
				routeData.Add("sort", SortField + "_desc");
				i.AddCssClass("fas fa-sort-up");
			}
			else if (String.Equals(SortField + "_desc", CurrentSort, StringComparison.OrdinalIgnoreCase))
			{
				routeData.Add("sort", SortField + "_asc");
				i.AddCssClass("fas fa-sort-down");
			}
			else
			{
				routeData.Add("sort", SortField + "_asc");
				i.AddCssClass("fas fa-sort");
			}

			routeData.Add("page", 1);

			var url = urlHelperFactory.RecreateCurrentUrl(ViewContext, routeData);
			var content = await output.GetChildContentAsync();

            var a = new TagBuilder("a");
            a.MergeAttribute("href", url);
            a.MergeAttribute("class", "text-dark pe-1");
            a.InnerHtml.AppendHtml(content);
            output.AddClass("sortable", HtmlEncoder.Default);
            output.AddClass("p-2", HtmlEncoder.Default);
            output.Content.AppendHtml("<div style=\"whitespace:nowrap\" class=\"align-middle\">");
            output.Content.AppendHtml(a);
            output.Content.AppendHtml(i);
            output.Content.AppendHtml("</div>");
        }
	}
}