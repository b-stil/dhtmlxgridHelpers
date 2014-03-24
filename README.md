dhtmlxgridHelpers
=================
Disclaimer:
The basis of this code is based on the DataTables MVC Helpers by Mcintyre.
It has been modified and added upon to support dhtmlxgrid.
The Standard Edition v.3.6 of dhtmlxgrid which is issued under GNU GPL v2 is included.
This is a work in progress and is in no way complete functionality.

C# MVC Helpers to render a dhtmlxgrid from within a view using ajax data sources with a simple syntax.

To render the grid within the #table-demo div use this code snippet:
```C#
<div id="table-demo"></div>
@{
    var vm = Html.DhxGridVm("table-demo", (HomeController h) => h.GetDemoJson());
    vm.FixedLeftColumns = 1;
    vm.AllowColumnReorder = true;
}
@Html.Partial("_DXHGridStandard", vm)
```
