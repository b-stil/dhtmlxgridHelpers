using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace DHXHelperDemo.Code.DHX
{
    /// <summary>
    /// Parses the request values from a query from the DataTables jQuery pluggin
    /// </summary>
    /// <typeparam name="T">List data type</typeparam>
    /// <typeparam name="U">Projected data type</typeparam>
    public class DHXParser<T, U>
    {
        /*

         */
        private const string ASCENDING_SORT = "asc";

        private IQueryable<T> _queriable;
        private readonly HttpRequestBase _httpRequest;
        private readonly Type _type;
        private readonly PropertyInfo[] _properties;
        //private readonly PropertyInfo[] _projectedProperties;
        private readonly string[] _columnsToIterate;

        private readonly bool _suppressPagingAndFiltering;        

        public DHXParser(HttpRequestBase httpRequest, IQueryable<T> queriable)
            : this(httpRequest, queriable, false)
        {            
        }

        public DHXParser(HttpRequestBase httpRequest, IQueryable<T> queriable, bool suppressPagingAndFiltering)
        {
            this.DateTimeFormatString = "d";
            this.BooleanTrueValueString = "Yes";
            this.BooleanFalseValueString = "No";
            this.DecimalFormatString = "c";

            _queriable = queriable;
            _httpRequest = httpRequest;
            _type = typeof(T);
            _suppressPagingAndFiltering = suppressPagingAndFiltering;            
            _properties = GetPropertyInfos();
        }

        public DHXParser(HttpRequestBase httpRequest, IQueryable<T> queriable, string[] columnsToIterate)
        {
            this.DateTimeFormatString = "d";
            this.BooleanTrueValueString = "Yes";
            this.BooleanFalseValueString = "No";
            this.DecimalFormatString = "c";

            _columnsToIterate = columnsToIterate;
            _queriable = queriable;
            _httpRequest = httpRequest;
            _type = typeof(T);
            _properties = GetPropertyInfos();
        }

        public DHXParser(HttpRequest httpRequest, IQueryable<T> queriable)
            : this(new HttpRequestWrapper(httpRequest), queriable)
        { }

        private PropertyInfo[] GetPropertyInfos()
        {
            // Need to set _properties to only include properties that we project on to U
            PropertyInfo[] projectedProperties = typeof(U).GetProperties();

            IQueryable<PropertyInfo> propsQueryable = _type.GetProperties().AsQueryable<PropertyInfo>();
            List<string> projectedNames = projectedProperties.Select(pi => pi.Name).ToList();

            var temp = from p in propsQueryable
                       where projectedNames.Contains(p.Name)
                       select p;
            return temp.ToArray();
        }

        private DHXJsonResponse<T> Parse(DHXJsonResponse<T> list)
        {
            DHXJsonResponse<T> emptyList = list;
            
            try
            {
                // parse the echo property (must be returned as int to prevent XSS-attack)
                //list.sEcho = int.Parse(_httpRequest[ECHO]);

                // count the record BEFORE filtering
                list.total_count = TotalRecordCount.HasValue ? TotalRecordCount.Value : _queriable.Count();

                // apply the sort, if there is one
                //ApplySort();

                // parse the paging values
                //if (!_suppressPagingAndFiltering)
                //{
                //    int skip = 0, take = 10;
                //    int.TryParse(_httpRequest[DISPLAY_START], out skip);
                //    int.TryParse(_httpRequest[DISPLAY_LENGTH], out take);

                //    ApplyWhere();
                //    if (ColumnWasFiltered)
                //        list.total_count = _queriable.Where(IndividualPropertySearch).Count();
                //    else
                //        list.total_count = list.total_count;

                //    if (list.total_count > take)
                //        // apply skip and take only if total records exceeds the take (this is a perf optimization)
                //        ApplySkipAndTake(skip, take);
                //}

                //Just turn the object into a List<T> that will be JSONified
                list.data = _queriable.ToList();

                //old code that was doing way too much to deal with the formatting of the view model
                //that should be done when the view model is populated
                //list.aaData = _queriable
                //                .Select(SelectPropertiesForObjectAsListOfString)
                //                .ToList();

                if (_suppressPagingAndFiltering)
                {
                    list.total_count = list.total_count;
                }

                return list;
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
                // Fail safe, if all else fails, return an empty list
                emptyList.data = new List<T>();
                return emptyList;
            }
        }

        public DHXJsonResponse<T> Parse()
        {
            return Parse(_properties.Select(x => x.Name).ToArray());
        }

        public DHXJsonResponse<T> Parse(string[] properties)
        {
            var list = new DHXJsonResponse<T>();

            // import property names
            //list.Import(properties);

            return Parse(list);
        }

        /// <summary>
        /// Gets or sets the total record count. Use this for performance reasons, typically
        /// over very large lists.
        /// </summary>
        /// <value>The total record count.</value>
        public int? TotalRecordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is detached list. Use this to apply case-insensitive sorting
        /// to a list that is not coming from the database.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is detached list; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetachedList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not apply skip and take. Use this for performance improvement if
        /// you know you will have a relatively small list (less than 100 items).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if you do not want skip and take; otherwise, <c>false</c>.
        /// </value>
        public bool DoNotApplySkipAndTake { get; set; }

        /// <summary>
        /// Gets or sets the DateTime format string used in this DataTable. The default is the Short Date Pattern "d".
        /// </summary>
        public string DateTimeFormatString { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the text to display if the value is true.
        /// </summary>
        public string BooleanTrueValueString { get; set; }

        /// <summary>
        /// Gets or sets the Decimal format string used in this DataTable. The default is the Currency pattern "c".
        /// </summary>
        public string DecimalFormatString { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the text to display if the value is false.
        /// </summary>
        public string BooleanFalseValueString { get; set; }

        private void ApplySort()
        {
            // Get all iSortCol indexes, in order from 0..n
            IQueryable<string> temp = _httpRequest.Params.AllKeys.AsQueryable();
            //List<string> sortedSortColumns = (from k in temp
            //                                  where k.StartsWith(INDIVIDUAL_SORT_KEY_PREFIX)
            //                                  orderby k
            //                                  select k).ToList<string>();

            //// Get the order of the columns from the client (in case they used ColReorder to reorder)
            //string[] columnOrderFromClient = _httpRequest.Params[SCOLUMNS].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //if (columnOrderFromClient.Length == 0)
            //    throw new ArgumentException("You must configure the DataTables aoColumns parameter on the client-side.");

            //foreach (string key in sortedSortColumns)
            //{
            //    // column number to sort (same as the array)
            //    int sortcolumn = int.Parse(_httpRequest[key]);

            //    // ignore malformatted values
            //    if (sortcolumn < 0 || sortcolumn >= _properties.Length)
            //        break;

            //    // get the direction of the sort
            //    string sortdir = _httpRequest[INDIVIDUAL_SORT_DIRECTION_KEY_PREFIX + key.Replace(INDIVIDUAL_SORT_KEY_PREFIX, string.Empty)];

            //    // get the property name to sort on
            //    string columnToSort = columnOrderFromClient[sortcolumn];
            //    //string columnToSrt = _properties[sortcolumn].Name;

            //    if (key == sortedSortColumns[0])
            //    {
            //        // Very first sort
            //        if (string.IsNullOrEmpty(sortdir) || sortdir.Equals(ASCENDING_SORT, StringComparison.OrdinalIgnoreCase))
            //            _queriable = _queriable.OrderBy(columnToSort);
            //        else
            //            _queriable = _queriable.OrderByDescending(columnToSort);
            //    }
            //    else
            //    {
            //        // Second and subsequent sorts
            //        if (string.IsNullOrEmpty(sortdir) || sortdir.Equals(ASCENDING_SORT, StringComparison.OrdinalIgnoreCase))
            //            _queriable = _queriable.ThenBy(columnToSort);
            //        else
            //            _queriable = _queriable.ThenByDescending(columnToSort);
            //    }
            //}
        }

        //private void ApplyWhere()
        //{
        //    _queriable = _queriable.Where(IndividualPropertySearch);
        //}

        private void ApplySkipAndTake(int skip, int take)
        {
            if (DoNotApplySkipAndTake)
                return;
            else
                _queriable = _queriable.Skip(skip).Take(take);
        }

        /// <summary>
        /// Expression that returns a list of string values, which correspond to the values
        /// of each property in the list type
        /// </summary>
        /// <remarks>This implementation does not allow indexers</remarks>
        private Func<T, List<string>> SelectPropertiesForObjectAsListOfString
        {
            get
            {
                return value =>
                {
                    var stringList = new List<string>();
                    foreach (var prop in _properties)
                    {
                        if (prop.GetIndexParameters().Length == 0)
                        {
                            string fs = null;
                            if (prop.CustomAttributes != null && prop.CustomAttributes.Count() > 0)
                            {
                                var attrib2 = prop.GetCustomAttribute<DHXGridColumnAttribute>();
                                if (attrib2 != null)
                                    fs = attrib2.FormatString;
                            }

                            if (prop.PropertyType == typeof(DateTime))
                            {
                                var dt = (DateTime)(prop.GetValue(value, new object[0]));
                                stringList.Add(dt.ToString(string.IsNullOrEmpty(fs) ? this.DateTimeFormatString : fs));
                            }
                            else if (prop.PropertyType == typeof(DateTime?))
                            {
                                var dt = (DateTime?)(prop.GetValue(value, new object[0]));
                                stringList.Add(dt.HasValue ? dt.Value.ToString(string.IsNullOrEmpty(fs) ? this.DateTimeFormatString : fs) : string.Empty);
                            }
                            else if (prop.PropertyType == typeof(bool))
                            {
                                var dt = (bool)(prop.GetValue(value, new object[0]));
                                stringList.Add(dt == true ? this.BooleanTrueValueString : this.BooleanFalseValueString);
                            }
                            else if (prop.PropertyType == typeof(decimal))
                            {
                                var dt = (decimal)(prop.GetValue(value, new object[0]));
                                stringList.Add(dt.ToString(string.IsNullOrEmpty(fs) ? this.DecimalFormatString : fs));
                            }
                            else if (prop.PropertyType == typeof(decimal?))
                            {
                                var dt = (decimal?)(prop.GetValue(value, new object[0]));
                                stringList.Add(dt.HasValue ? dt.Value.ToString(string.IsNullOrEmpty(fs) ? this.DecimalFormatString : fs) : string.Empty);
                            }
                            else
                                stringList.Add((prop.GetValue(value, new object[0]) ?? string.Empty).ToString());
                        }
                        else if (prop.GetIndexParameters().Length == 1)
                        {
                            foreach (string column in _columnsToIterate)
                            {
                                stringList.Add((prop.GetValue(value, new object[] { column }) ?? string.Empty).ToString());
                            }
                        }
                    }
                    return stringList;
                };
            }
        }


        ///// <summary>
        ///// Compound predicate expression with the individual search predicates that will filter the results
        ///// per an individual column
        ///// </summary>
        //private Expression<Func<T, bool>> IndividualPropertySearch
        //{
        //    get
        //    {
        //        var paramExpr = Expression.Parameter(typeof(T), "val");
        //        Expression whereExpr = Expression.Constant(true); // default is val => True

        //        // This expression perfroms a logical AND operation
        //        // on its two arguments, but if the first argument is false,
        //        // then the second arument is not evaluated.
        //        // Both arguments must be of the boolean type.
        //        Expression andAlsoExpr = Expression.AndAlso(Expression.Constant(false),Expression.Constant(true));

        //        IQueryable<string> temp = _httpRequest.Params.AllKeys.AsQueryable();
        //        List<string> sortedSortColumns = (from k in temp
        //                                          where k.StartsWith(INDIVIDUAL_SEARCH_KEY_PREFIX)
        //                                          orderby k
        //                                          select k).ToList<string>();

        //        // Get the order of the columns from the client (in case they used ColReorder to reorder)
        //        string[] columnOrderFromClient = _httpRequest.Params[SCOLUMNS].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        //        foreach (string key in sortedSortColumns)
        //        {
        //            // parse the property number
        //            int propertyIndex = -1;

        //            bool wasSuccessful = int.TryParse(key.Replace(INDIVIDUAL_SEARCH_KEY_PREFIX, string.Empty), out propertyIndex);
        //            string stringFilter = _httpRequest[key];

        //            if (!wasSuccessful || stringFilter == null || stringFilter.Length == 0)
        //                continue; // go to next individual property search

        //            ColumnWasFiltered = true;
        //            string query = _httpRequest[key];

        //            //DARRELL START
        //            Type propertyType = _properties.Where(x => x.Name == columnOrderFromClient[propertyIndex]).First().PropertyType;//_properties[propertyIndex].PropertyType;
        //            MemberExpression property = Expression.Property(paramExpr, typeof(T).GetProperty(_properties[propertyIndex].Name));

        //            if (propertyType == typeof(string)) // For string
        //            {
        //                string queryInTypeForm = query;
        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                if (!IsDetachedList)
        //                {
        //                    // This works for a database-backed query
        //                    whereExpr = Expression.And(whereExpr,
        //                                               Expression.Call(property,
        //                                                               typeof(string).GetMethod("Contains"),
        //                                                               queryValue
        //                                                              )
        //                        );
        //                }
        //                else
        //                {
        //                    // perform a case insensitive comparison for a Linq-to-Entities query that does not hit the database
        //                    Expression zero = Expression.Constant(0);
        //                    Expression caseInsensitive = Expression.Constant(StringComparison.InvariantCultureIgnoreCase);
        //                    MethodInfo mi = typeof(string).GetMethod("IndexOf", new Type[] { typeof(string), typeof(StringComparison) });
        //                    whereExpr = Expression.And(whereExpr,
        //                                               Expression.GreaterThanOrEqual(Expression.Call(property,
        //                                                                                             mi,
        //                                                                                             queryValue,
        //                                                                                             caseInsensitive
        //                                                                                            ),
        //                                                                              zero)
        //                                                );
        //                }
        //            }
        //            else if (propertyType == typeof(int)) // For numeric
        //            {
        //                int queryInTypeForm;
        //                if (!int.TryParse(query, out queryInTypeForm))
        //                    continue;

        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                whereExpr = Expression.And(whereExpr,
        //                                           Expression.GreaterThanOrEqual(property, queryValue)
        //                            );
        //            }
        //            else if (propertyType == typeof(bool)) // for bool
        //            {
        //                if (string.Compare(this.BooleanTrueValueString.Substring(0, 1), query, true) == 0
        //                    || string.Compare(this.BooleanTrueValueString, query, true) == 0)
        //                {
        //                    query = "true";
        //                }
        //                else if (string.Compare(this.BooleanFalseValueString.Substring(0,1), query, true) == 0
        //                    || string.Compare(this.BooleanFalseValueString, query, true) == 0)
        //                {
        //                    query = "false";
        //                }


        //                bool queryInTypeForm;
        //                if (!bool.TryParse(query, out queryInTypeForm))
        //                    continue;

        //                //var queryInTypeForm = Convert.ChangeType(query, propertyType);
        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                whereExpr = Expression.And(whereExpr,
        //                                           Expression.Equal(property, queryValue)
        //                            );
        //            }
        //            else if ((propertyType == typeof(DateTime)) || (propertyType == typeof(DateTime?)))
        //            {
        //                DateTime queryInTypeForm;
        //                if (!DateTime.TryParse(query, out queryInTypeForm))
        //                    continue;

        //                //var queryInTypeForm = Convert.ChangeType(query, propertyType);
        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                whereExpr = Expression.And(whereExpr,
        //                                           Expression.GreaterThanOrEqual(property, queryValue)
        //                            );
        //            }
        //            else if (propertyType == typeof(Decimal))
        //            {
        //                Decimal queryInTypeForm;
        //                if (!Decimal.TryParse(query, out queryInTypeForm))
        //                    continue;

        //                //var queryInTypeForm = Convert.ChangeType(query, propertyType);
        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                whereExpr = Expression.And(whereExpr,
        //                                           Expression.GreaterThanOrEqual(property, queryValue)
        //                            );
        //            }
        //            else if (propertyType == typeof(double))
        //            {
        //                double queryInTypeForm;
        //                if (!double.TryParse(query, out queryInTypeForm))
        //                    continue;

        //                //var queryInTypeForm = Convert.ChangeType(query, propertyType);
        //                ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);

        //                whereExpr = Expression.And(whereExpr,
        //                                           Expression.GreaterThanOrEqual(property, queryValue)
        //                            );
        //            }
        //            else
        //            {
        //                var propType = propertyType; // if you get here, please let Darrell know what propertyType is and what page you're on
        //            }

        //            // DARRELL END

        //            #region old code
        //            // ORIGINAL START
        //            // val.{PropertyName}.ToString().ToLower().Contains({query})
        //            //var toStringCall = Expression.Call(
        //            //                        Expression.Call(
        //            //                                Expression.Property(paramExpr, _properties[property]),
        //            //                                "ToString",
        //            //                                new Type[0]
        //            //                            ),
        //            //                        typeof(string).GetMethod("ToLower", new Type[0])
        //            //                    );

        //            //// reset where expression to also require the current contraint
        //            //whereExpr = Expression.And(whereExpr,
        //            //                           Expression.Call(Expression.Property(paramExpr, _properties[property]),
        //            //                                           typeof(string).GetMethod("Contains"),
        //            //                                           Expression.Constant(query)));
        //            // ORIGINAL END
        //            #endregion

        //        }

        //        return Expression.Lambda<Func<T, bool>>(whereExpr, paramExpr);
        //    }
        //}

        private bool ColumnWasFiltered { get; set; }

        //private List<string> SortedSortColumns
        //{
        //    get
        //    {
        //        IQueryable<string> temp = _httpRequest.Params.AllKeys.AsQueryable();
        //        List<string> sortedSortColumns = (from k in temp
        //                                          where k.StartsWith(INDIVIDUAL_SEARCH_KEY_PREFIX)
        //                                          orderby k
        //                                          select k).ToList<string>();
        //        return sortedSortColumns;
        //    }
        //}

        private void getStuff(Type propertyType, string query)
        {
            var queryInTypeForm = Convert.ChangeType(query, propertyType);
            ConstantExpression queryValue = Expression.Constant(queryInTypeForm, propertyType);
        }

        #region Unused
        // needs to be revised ...
        /// <summary>
        /// Expression for an all column search, which will filter the result based on this criterion
        /// </summary>
        //private Expression<Func<T, bool>> ApplyGenericSearch
        //{
        //    get
        //    {
        //        string search = _httpRequest["sSearch"];

        //        // default value
        //        if (string.IsNullOrEmpty(search) || _properties.Length == 0)
        //            return x => true;

        //        // invariant expressions
        //        var searchExpression = Expression.Constant(search.ToLower());
        //        var paramExpression = Expression.Parameter(typeof(T), "val");

        //        // query all properties and returns a Contains call expression 
        //        // from the ToString().ToLower()
        //        var propertyQuery = (from property in _properties
        //                            let tostringcall = Expression.Call(
        //                                                    Expression.Call(
        //                                                        Expression.Property(paramExpression, property), 
        //                                                        "ToString", 
        //                                                        new Type[0]
        //                                                        ),
        //                                                    typeof(string).GetMethod("ToLower", new Type[0])
        //                                               )
        //                            select Expression.Call(tostringcall, typeof(string).GetMethod("Contains"), searchExpression)).ToArray();

        //        // we now need to compound the expression by starting with the first
        //        // expression and build through the iterator
        //        Expression compoundExpression = propertyQuery[0];

        //        // add the other expressions
        //        for (int i = 1; i < propertyQuery.Length; i++)
        //            compoundExpression = Expression.Or(compoundExpression, propertyQuery[i]);

        //        // compile the expression into a lambda 
        //        return Expression.Lambda<Func<T, bool>>(compoundExpression, paramExpression);
        //    }
        //}
        #endregion

    }


#region old code

    //public enum Operand
    //{
    //    Equal,
    //    NotEqual,
    //    LessThan,
    //    LessThanEqual,
    //    GreaterThan,
    //    GreaterThanEqual
    //}

    //public static class Extensions
    //{

    //    public static IQueryable<T> AddClause<T, V>(this IQueryable<T> queryable,
    //      string propertyName, Operand operand, V propertyValue)
    //    {
    //        IQueryable<T> query = queryable.Where<T>(
    //          MakeExpression<T, V>(propertyName, operand, propertyValue));

    //        return (query);
    //    }

    //    private static Expression<Func<T, bool>> MakeExpression<T, V>(string propertyName, Operand operand,
    //      V propertyValue)
    //    {
    //        ParameterExpression pe = Expression.Parameter(typeof(T), "p");
            
    //        Func<Expression, Expression, bool, MethodInfo, BinaryExpression>
    //          fn = GetFuncForOperand(operand);

    //        MemberExpression me = Expression.Property(pe, typeof(T).GetProperty(propertyName));
    //        ConstantExpression ce = Expression.Constant(propertyValue, typeof(V));

            
    //        Expression<Func<T, bool>> e =
    //          Expression.Lambda<Func<T, bool>>( 
    //                  fn(me, ce, false, null),
    //                  new ParameterExpression[] { pe }
    //              );

    //        return (e);
    //    }

    //    private static Func<Expression, Expression, bool, MethodInfo, BinaryExpression> GetFuncForOperand(Operand operand)
    //    {
    //        Func<Expression, Expression, bool, MethodInfo, BinaryExpression> func = null;
            
    //        switch (operand)
    //        {
    //            case Operand.Equal:
    //                func = Expression.Equal;
    //                break;
    //            case Operand.NotEqual:
    //                func = Expression.NotEqual;
    //                break;
    //            case Operand.LessThan:
    //                func = Expression.LessThan;
    //                break;
    //            case Operand.LessThanEqual:
    //                func = Expression.LessThanOrEqual;
    //                break;
    //            case Operand.GreaterThan:
    //                func = Expression.GreaterThan;
    //                break;
    //            case Operand.GreaterThanEqual:
    //                func = Expression.GreaterThanOrEqual;
    //                break;
    //            default:
    //                break;
    //        }
    //        return (func);
    //    }
    //}
#endregion

}
