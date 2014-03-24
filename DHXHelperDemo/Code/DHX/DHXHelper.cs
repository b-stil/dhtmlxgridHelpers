using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DHXHelperDemo.Code.DHX
{
    /// <summary>
    /// Based on Mcintyre's helper for DataTables
    /// </summary>
    public static class DHXHelper
    {
        /// <summary>
        /// Dynamically builds the datatable view model based on the calls passed in.
        /// NOTE:  A curreent deviation from the original added filterId ffor supporting
        /// /Controller/View/Id routes however we should be able to get the route value (id)
        /// from the Expression.  Will come back to this.
        /// 
        /// Constraint:  Only supports single route values (aka Id)
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="html"></param>
        /// <param name="id"></param>
        /// <param name="exp"></param>
        /// <param name="columns"></param>
        /// <param name="filterId"></param>
        /// <returns></returns>
        public static DHXGridVm DhxGridVm<TController, TResult>(this HtmlHelper html, string id, Expression<Func<TController, DHXResult<TResult>>> exp, IEnumerable<ColDef> columns = null, int? filterId = null, string filterIdAsString = null)
        {
            string aaSort = string.Empty;
            if (columns == null || !columns.Any())
            {
                var propInfos = typeof(TResult).GetProperties().Where(p => p.GetGetMethod() != null).ToList();
                var columnList = new List<ColDef>();
                var sortList = new List<ColumnSortConfiguration>();
                int counter = 0;
                int displayOrder = 0;

                foreach (var propertyInfo in propInfos)
                {
                    var displayNameAttribute = (DisplayNameAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();
                    var displayName = displayNameAttribute == null ? propertyInfo.Name : displayNameAttribute.DisplayName;

                    bool columnIsVisible = true;
                    int? sWidth = null;
                    string dataHide = string.Empty;
                    string dataClass = string.Empty;

                    var attribute = propertyInfo.GetCustomAttributes<DHXGridColumnAttribute>(false).FirstOrDefault();
                    if (attribute != null)
                    {
                        displayOrder = (attribute.DisplayOrder >= 0) ? attribute.DisplayOrder : (counter + 1);
                        columnIsVisible = !(attribute.IsHidden);
                        sWidth = attribute.ColumnWidth;
                        if (attribute.SetSortOrder)
                        {
                            sortList.Add(new ColumnSortConfiguration()
                            {
                                ColumnIndex = counter,
                                PositionInSortOrder = attribute.PositionInSortOrder,
                                SortOrder = SortOrderConvert.FromEnum(attribute.Sort)
                            });
                        }

                        switch (attribute.HideColumnWhen)
                        {
                            case DataHide.AlwaysVisible:
                                dataHide = string.Empty;
                                break;
                            case DataHide.HideWhenPhone:
                                dataHide = "data-hide=phone";
                                break;
                            case DataHide.HideWhenTablet:
                                dataHide = "data-hide=tablet";
                                break;
                            case DataHide.HideWhenPhoneAndTablet:
                                dataHide = "data-hide=phone,tablet";
                                break;
                        }

                        switch (attribute.ExpandIcon)
                        {
                            case DataClass.None:
                                dataClass = string.Empty;
                                break;
                            case DataClass.Expand:
                                dataClass = "data-class=expand";
                                break;
                        }
                    }

                    columnList.Add(new ColDef()
                    {
                        Name = propertyInfo.Name,
                        DisplayName = displayName,
                        DisplayOrder = displayOrder,
                        Type = propertyInfo.PropertyType,
                        IsVisible = columnIsVisible,
                        ColumnWidth = sWidth,
                        DataClass = dataClass,
                        DataHide = dataHide
                    });

                    displayOrder++;
                    counter++;
                }
                //sort the columns based on their display order to be ascending
                columns = columnList.OrderBy(o => o.DisplayOrder).ToArray();
                if (sortList.Count > 0)
                {
                    aaSort = GetSortingStringFromList(sortList);
                }
            }

            var mi = exp.MethodInfo();
            var controllerName = typeof(TController).Name;
            controllerName = controllerName.Substring(0, controllerName.LastIndexOf("Controller"));
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var ajaxUrl = urlHelper.Action(mi.Name, controllerName);
            // 
            // TPE - added support for ID values.  Need to come back to this however, because I should be abe to realize it from the expression.
            // 
            if (filterId.HasValue)
                ajaxUrl = urlHelper.Action(mi.Name, controllerName, new { id = filterId });
            else if (!string.IsNullOrEmpty(filterIdAsString))
                ajaxUrl = urlHelper.Action(mi.Name, controllerName, new { id = filterIdAsString });

            return new DHXGridVm(id, ajaxUrl, columns, aaSort);
        }

        public static IHtmlString DatatableDefaultColumnFilters(this HtmlHelper html, DHXGridVm vm)
        {
            var result = new StringBuilder();
            if (vm.DefaultFilters.Count == 0)
                return html.Raw(string.Empty);
            result.Append("\"aoSearchCols\":[");
            foreach (var col in vm.Columns)
            {
                if (vm.DefaultFilters.ContainsKey(col.Name))
                {
                    result.AppendFormat("{{ \"sSearch\":\"{0}\"}},", vm.DefaultFilters[col.Name]);
                }
                else
                {
                    result.Append("null,");
                }
            }
            result.Length -= 1;
            result.Append("],");
            return html.Raw(result.ToString());
        }

        /// <summary>
        /// This will generate the header text.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IHtmlString DHXGridHeaderDefinitions(this HtmlHelper html, DHXGridVm vm)
        {
            var output = new StringBuilder();
            foreach (var col in vm.Columns)
            {
                output.AppendFormat("{0},", col.DisplayName);
            }
            //remove the last comma
            output.Length -= 1;
            return html.Raw(output.ToString());
        }

        /// <summary>
        /// This will generate the ColumnIDs
        /// </summary>
        /// <param name="html"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IHtmlString DHXGridColumnIDDefinitions(this HtmlHelper html, DHXGridVm vm)
        {
            var output = new StringBuilder();
            foreach (var col in vm.Columns)
            {
                output.AppendFormat("{0},", col.Name);
            }
            //remove the last comma
            output.Length -= 1;
            return html.Raw(output.ToString());
        }

        /// <summary>
        /// This will generate the Column Widths
        /// </summary>
        /// <param name="html"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IHtmlString DHXGridColumnWidthDefinitions(this HtmlHelper html, DHXGridVm vm)
        {
            var output = new StringBuilder();
            foreach (var col in vm.Columns)
            {
                output.AppendFormat("{0},", col.ColumnWidth);
            }
            //remove the last comma
            output.Length -= 1;
            return html.Raw(output.ToString());
        }

        /// <summary>
        /// This will generate the Column Alignment
        /// </summary>
        /// <param name="html"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IHtmlString DHXGridColumnAlignmentDefinitions(this HtmlHelper html, DHXGridVm vm)
        {
            var output = new StringBuilder();
            foreach (var col in vm.Columns)
            {
                output.AppendFormat("{0},", col.Alignment);
            }
            //remove the last comma
            output.Length -= 1;
            return html.Raw(output.ToString());
        }

        /// <summary>
        /// This will build the object initialization columns options to setup the grid.
        /// **Currently not working**
        /// </summary>
        /// <param name="html"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static IHtmlString DHXGridColumnDefinitions(this HtmlHelper html, DHXGridVm vm)
        {
            var output = new StringBuilder();
            var counter = 0;
            foreach (var col in vm.Columns)
            {
                output.AppendFormat("{{ label: \"{0}\", id: \"{1}\", type: \"{2}\", sort: \"{3}\", align: \"{4}\"",
                                                               col.DisplayName, col.Name, "ro", "str", "center");

                if (col.ColumnWidth.HasValue)
                    output.AppendFormat(", width: {0}", col.ColumnWidth);

                output.AppendFormat("}},");

                counter++;
                
            }

            output.Length -= 1;
            return html.Raw(output.ToString());
        }

        public static DHXGridVm DataTableVm(this HtmlHelper html, string id, string ajaxUrl, params string[] columns)
        {
            return new DHXGridVm(id, ajaxUrl, columns.Select(c => ColDef.Create(c, (string)null, typeof(string), true)));
        }


        private class ColumnSortConfiguration
        {
            public int ColumnIndex { get; set; }
            public int PositionInSortOrder { get; set; }
            public string SortOrder { get; set; }
        }

        private static string GetSortingStringFromList(IEnumerable<ColumnSortConfiguration> sortList)
        {
            var query = from item in sortList
                        orderby item.PositionInSortOrder
                        select item;

            var builder = new StringBuilder();
            builder.Append("[");
            foreach (var column in query)
            {
                builder.AppendFormat("[{0}, '{1}'],", column.ColumnIndex, column.SortOrder);
            }
            builder.Length -= 1;
            builder.Append("]");
            return builder.ToString();
        }


    }
}