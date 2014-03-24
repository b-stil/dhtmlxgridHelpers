using System;
using System.Collections.Generic;
using System.Linq;

namespace DHXHelperDemo.Code.DHX
{
    /// <summary>
    /// Borrowed from McYntire's mvc. datatables impl.
    /// </summary>
    public class ColDef
    {
        private int? _columnWidth;
        private string _alignment;
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public string Width { get; set; }
        public Type Type { get; set; }
        public bool IsVisible { get; set; }
        public string DataClass { get; set; }
        public string DataHide { get; set; }

        public string Sort { get; set; }

        public int? ColumnWidth
        {
            get { return _columnWidth.HasValue ? _columnWidth : 50; }
            set { _columnWidth = value; }
        }

        public string Alignment
        {
            get { return String.IsNullOrWhiteSpace(_alignment) ? "center" : _alignment; }
            set { _alignment = value; }
        }

        public static ColDef Create(string name, string p1, Type propertyType, bool visible, int? width = null)
        {
            return new ColDef()
            {
                Name = name,
                DisplayName = p1,
                Type = propertyType,
                IsVisible = visible,
                ColumnWidth = width
            };
        }
    }

    public class DHXGridVm
    {
        //////////////////START DHX
        /// <summary>
        /// The CSS ID of the div that the grid should be rendered within
        /// </summary>
        public string TargetDivID { get; set; }

        /// <summary>
        /// Enable or Disable multiline in the grid row
        /// </summary>
        public bool MultiLine { get; set; }

        /// <summary>
        /// Enable or Disable ColumnAutoSizing
        /// </summary>
        public bool ColumnAutoSize { get; set; }

        /// <summary>
        /// Enable or Disable Column Reordering in the UI
        /// </summary>
        public bool AllowColumnReorder { get; set; }

        /// <summary>
        /// Enable or Disable Column Hiding in the UI
        /// </summary>
        public bool AllowColumnVisibility { get; set; }

        /// <summary>
        /// Enable or Disable Column Auto Sizing
        /// </summary>
        public bool AllowColumnAutoSizing { get; set; }

        /// <summary>
        /// Number of Left Columns that should remain fixed in the table
        /// </summary>
        public int? FixedLeftColumns { get; set; }

        public DHXGridVm(string id, string ajaxUrl, IEnumerable<ColDef> columns, string aaSort = "")
        {
            AjaxUrl = ajaxUrl;
            TargetDivID = id;
            Columns = columns;
            FilterTypeRules = new FilterRuleList();
            FilterTypeRules.AddRange(StaticFilterTypeRules);
            DefaultSort = aaSort;
            AllowSorting = false;
            AllowColumnVisibility = false;
            AllowColumnReorder = false;
            AllowColumnAutoSizing = false;
        }
        //////////////////END DHX specific setup

        public string RowClickUrl { get; set; }

        public bool AllowSorting { get; set; }

        private Dictionary<string, string> defaultFilters = new Dictionary<string, string>();

        /// <summary>
        /// Url to get the data for an ajax call
        /// </summary>
        public string AjaxUrl { get; set; }

        public IEnumerable<ColDef> Columns { get; private set; }

        /// <summary>
        /// Allow Column filters
        /// </summary>
        public bool ColumnFilter { get; set; }


        public bool RowSelection { get; set; }

        /// <summary>
        /// Auto adjust the width of the table column to fit parent
        /// </summary>
        public bool AutoWidth { get; set; }

        #region NotImplemented
        public string ColumnFiltersString
        {
            get
            {
                var result = string.Join(",", Columns.Select(c => GetFilterType(c.Name, c.Type)));
                return result;
            }
        }

        public Dictionary<string, string> DefaultFilters
        {
            get { return defaultFilters; }
        }


        public string GetFilterType(string columnName, Type type)
        {
            foreach (Func<string, Type, string> filterTypeRule in FilterTypeRules)
            {
                var rule = filterTypeRule(columnName, type);

                if (columnName == "RowSelector")
                {
                    rule = "{type: 'text', bSearchable: false}";
                }

                if (rule != null) return rule;
            }
            return "null";
        }

        public FilterRuleList FilterTypeRules;

        /// <summary>
        /// Mod TPE - jquery column filters creates non-reentrant datatables using divs dynamically created.
        /// </summary>
        public static FilterRuleList StaticFilterTypeRules = new FilterRuleList()
        {
            //(c, t) => (DateTypes.Contains(t)) ? "{type: 'date-range'}" : null,
            //(c, t) => t == typeof(bool) ? "{type: 'checkbox', values : ['True', 'False']}" : null,
            //(c, t) => t.IsEnum ?  ("{type: 'checkbox', values : ['" + string.Join("','", Enum.GetNames(t)) + "']}") : null,
            (c, t) => "{type: 'text'}", //by default, text filter on everything
        };

        private static List<Type> DateTypes = new List<Type>
        {
            typeof (DateTime),
            typeof (DateTime?),
            typeof (DateTimeOffset),
            typeof (DateTimeOffset?)
        };

        public class _FilterOn<TTarget>
        {
            private readonly TTarget _target;
            private readonly FilterRuleList _list;
            private readonly Func<string, Type, bool> _predicate;

            public _FilterOn(TTarget target, FilterRuleList list, Func<string, Type, bool> predicate)
            {
                _target = target;
                _list = list;
                _predicate = predicate;
            }

            public TTarget Select(params string[] options)
            {
                var escapedOptions = options.Select(o => o.Replace("'", "\\'"));
                AddRule("{type: 'select', values: ['" + string.Join("','", escapedOptions) + "']}");
                return _target;
            }

            public TTarget NumberRange()
            {
                AddRule("{type: 'number-range'}");
                return _target;
            }

            public TTarget DateRange()
            {
                AddRule("{type: 'date-range'}");
                return _target;
            }

            public TTarget Number()
            {
                AddRule("{type: 'number'}");
                return _target;
            }

            public TTarget CheckBoxes(params string[] options)
            {
                var escapedOptions = options.Select(o => o.Replace("'", "\\'"));
                AddRule("{type: 'checkbox', values: ['" + string.Join("','", escapedOptions) + "']}");
                return _target;
            }

            public TTarget None()
            {
                AddRule("null");
                return _target;
            }

            private void AddRule(string result)
            {
                _list.Insert(0, (c, t) => _predicate(c, t) ? result : null);
            }
        }

        public _FilterOn<DHXGridVm> FilterOn<T>()
        {
            return new _FilterOn<DHXGridVm>(this, this.FilterTypeRules, (c, t) => t == typeof(T));
        }

        public _FilterOn<DHXGridVm> FilterOn(string columnName)
        {
            return new _FilterOn<DHXGridVm>(this, this.FilterTypeRules, (c, t) => c == columnName);
        }


        public string DefaultSort { get; set; }
    }

    public class FilterRuleList : List<Func<string, Type, string>>
    {
    }
        #endregion
}