using System;

namespace DHXHelperDemo.Code.DHX
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DHXGridColumnAttribute : Attribute
    {
        /// <summary>
        /// Any valid int value
        /// </summary>
        public int ColumnWidth { get; set; }
        
        /// <summary>
        /// Should the column be shown?
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Order to display the column
        /// </summary>
        public int DisplayOrder { get; set; }
        
        public bool SetSortOrder { get; set; }
        
        public int PositionInSortOrder { get; set; }
        
        public SortOrder Sort { get; set; }

        public string FormatString { get; set; }
		
        public DataClass ExpandIcon { get; set; }
		
        public DataHide HideColumnWhen { get; set; }

        public DHXGridColumnAttribute()
        {
            DisplayOrder = -1;
        }
    }

    public enum SortOrder
    {
        Ascending = 1,
        Descending = 2,
    }

    public static class SortOrderConvert
    {
        public static string FromEnum(SortOrder sortOrderEnum)
        {
            return sortOrderEnum == SortOrder.Ascending ? "asc" : "desc";
        }
    }

	public enum DataClass
	{		
		None = 0x00,
		Expand = 0x01
	}

	public enum DataHide
	{
		AlwaysVisible = 0x00,
		HideWhenPhone = 0x01,
		HideWhenTablet = 0x02,
		HideWhenPhoneAndTablet = 0x04
	}
}