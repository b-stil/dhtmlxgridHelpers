using System.Web;
using System.Web.Optimization;

namespace DHXHelperDemo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/dhtmlxgrid").Include(
                        "~/Scripts/dhtml/dhtmlxcommon.js"
                        , "~/Scripts/dhtml/dhtmlxgrid.js"
                        , "~/Scripts/dhtml/dhtmlxgridcell.js"
                        //, "~/Scripts/dhtml/dhtmlxdataprocessor.js"
                        //, "~/Scripts/dhtml/dhtmlxdataprocessor_debug.js"
                        , "~/Scripts/dhtml/ext/dhtmlxgrid_filter.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_json.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_mcol.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_srnd.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_pgn.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_splt.js"
                        //, "~/Scripts/dhtml/ext/dhtmlxgrid_hmenu.js"
                        //, "~/Scripts/dhtml/excells/dhtmlxgrid_excell_link.js"
                        ));

            bundles.Add(new StyleBundle("~/Scripts/dhtml/dhtmlxgrid").Include(
            "~/Scripts/dhtml/dhtmlxgrid.css"
            , "~/Scripts/dhtml/skins/dhtmlxgrid_dhx_skyblue.css"
            , "~/Scripts/dhtml/dhtmlx_custom.css"
            ));
        }
    }
}
