using System;
using System.Collections.Generic;
using X.PagedList;

namespace KTechTemplate.ViewModels
{
    public class PagingModel
    {
        public IPagedList Model { get; set; }
        public int Page { get; set; }
        public int Rows { get; set; }
        public List<Parameter> ParameterList { get; set; }

        public PagingModel()
        {
            this.Page = 1;
            this.Rows = 10;
            this.ParameterList = new List<Parameter>();
        }

        public PagingModel(IPagedList _Model, int _Page, int _Rows, List<Parameter> _parameters)
        {
            this.Model = _Model;
            this.Page = _Page;
            this.Rows = _Rows;
            this.ParameterList = _parameters;
        }
    }

    public class Parameter
    {
        public string RouteName { get; set; }
        public string RouteValue { get; set; }
    }
}
