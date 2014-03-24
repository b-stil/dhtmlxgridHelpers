using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DHXHelperDemo.Code.DHX
{
    /// <summary>
    /// Action result for data tables. Required for use with DataTableVm
    /// so we can correctly Type the View MOdel (which is derived from the DataTablesResult).
    /// 
    /// </summary>
    /// <typeparam name="T">The ViewModel Type we're gridifying</typeparam>
    public class DHXResult<T> : JsonResult
    {
        private readonly DHXJsonResponse<T> _data;
        private readonly HttpRequestBase _request;

        public DHXResult(IQueryable<T> data, HttpRequestBase request) :
            this(data, request, false, null) { }

        public DHXResult(IQueryable<T> data, HttpRequestBase request, bool suppressPagingAndFiltering) :
            this(data, request, suppressPagingAndFiltering, null) { }

        public DHXResult(IQueryable<T> data, HttpRequestBase request,
            bool suppressPagingAndFiltering, int? totalRecordsAvailable)
        {
            _request = request;

            if (!DHXRequest.ValidateDhxRequest(_request))
                throw new ArgumentException("There was a problem with the data you posted.");

            // dbContext is disposed once we start executing, need to parse and get our data in constructor
            var parser = new DHXParser<T, T>(_request, data, suppressPagingAndFiltering);
            parser.TotalRecordCount = totalRecordsAvailable;
            _data = parser.Parse();

        }

        public override void ExecuteResult(ControllerContext context)
        {
            ContentType = "application/json";
            Data = _data;
            ContentEncoding = null;
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            base.ExecuteResult(context);
        }
    }
}