﻿using System.Web;
using System.Web.Optimization;

namespace scrilla.js.Web
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.UseCdn = true;
			bundles.Add(new StyleBundle(Links.Bundles.Styles.app).IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.app.styles.Url()), "*.css", false));
			bundles.Add(new StyleBundle(Links.Bundles.Styles.bootstrap).IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.bootstrap_3_0_0.Url()), "*.css", false));
			bundles.Add(new StyleBundle(Links.Bundles.Styles.chosen).IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.chosen_1_0_0.Url()), "*.css", false));

			bundles.Add(new ScriptBundle(Links.Bundles.Scripts.app).IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.app.Url()), "*.js", false));
			bundles.Add(new ScriptBundle(Links.Bundles.Scripts.angularjs, "//ajax.googleapis.com/ajax/libs/angularjs/1.0.8/angular.min.js") { CdnFallbackExpression = "window.angular" }
				.IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.angularjs_1_0_8.Url()), "*.js", false));
			bundles.Add(new ScriptBundle(Links.Bundles.Scripts.bootstrap, "//netdna.bootstrapcdn.com/bootstrap/3.0.0/js/bootstrap.min.js") { CdnFallbackExpression = "$.fn.button" }
				.IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.bootstrap_3_0_0.Url()), "*.js", false));
			bundles.Add(new ScriptBundle(Links.Bundles.Scripts.chosen)
				.IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.chosen_1_0_0.Url()), "*.js", false));
			bundles.Add(new ScriptBundle(Links.Bundles.Scripts.jquery, "//code.jquery.com/jquery-2.0.3.min.js") { CdnFallbackExpression = "window.jQuery" }
				.IncludeDirectory(VirtualPathUtility.ToAppRelative(Links.lib.jquery_2_0_3.Url()), "*.js", false));
		}
	}
}

namespace Links
{
	public static partial class Bundles
	{
		public static partial class Styles
		{
			public static readonly string app = "~/bundles/styles/app";
			public static readonly string bootstrap = "~/bundles/styles/bootstrap";
			public static readonly string chosen = "~/bundles/styles/chosen";
		}

		public static partial class Scripts
		{
			public static readonly string app = "~/bundles/scripts/app";
			public static readonly string angularjs = "~/bundles/scripts/angularjs";
			public static readonly string bootstrap = "~/bundles/scripts/bootstrap";
			public static readonly string chosen = "~/bundles/scripts/chosen";
			public static readonly string jquery = "~/bundles/scripts/jquery";
		}
	}
}

