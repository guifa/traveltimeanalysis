﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OSMUtils.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TestData {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TestData() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OSMUtils.Tests.TestData", typeof(TestData).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static byte[] osm_invalid_root_element {
            get {
                object obj = ResourceManager.GetObject("osm_invalid_root_element", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_node_with_tag {
            get {
                object obj = ResourceManager.GetObject("osm_node_with_tag", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_node_with_tag_and_unknown_element {
            get {
                object obj = ResourceManager.GetObject("osm_node_with_tag_and_unknown_element", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_node_with_tags {
            get {
                object obj = ResourceManager.GetObject("osm_node_with_tags", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_relation {
            get {
                object obj = ResourceManager.GetObject("osm_relation", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_relation_with_tag {
            get {
                object obj = ResourceManager.GetObject("osm_relation_with_tag", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_simple_node {
            get {
                object obj = ResourceManager.GetObject("osm_simple_node", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_simple_relation {
            get {
                object obj = ResourceManager.GetObject("osm_simple_relation", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_simple_way {
            get {
                object obj = ResourceManager.GetObject("osm_simple_way", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_unknown_inner_element {
            get {
                object obj = ResourceManager.GetObject("osm_unknown_inner_element", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] osm_way_with_tags {
            get {
                object obj = ResourceManager.GetObject("osm_way_with_tags", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] real_osm_file {
            get {
                object obj = ResourceManager.GetObject("real_osm_file", resourceCulture);
                return ((byte[])(obj));
            }
        }
    }
}