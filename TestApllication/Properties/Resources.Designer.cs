﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestApllication.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TestApllication.Properties.Resources", typeof(Resources).Assembly);
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
        
        /// <summary>
        ///   Looks up a localized string similar to Data Source=localhost\Test_DB;Initial Catalog=COMMON;User ID=sa;Password=Administrator@;.
        /// </summary>
        internal static string ConnectionString {
            get {
                return ResourceManager.GetString("ConnectionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to COMMON.
        /// </summary>
        internal static string DataLogCommon {
            get {
                return ResourceManager.GetString("DataLogCommon", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đăng nhập thất bại: Sai tên đăng nhập hoặc mật khẩu.
        /// </summary>
        internal static string MSG_ERR_001 {
            get {
                return ResourceManager.GetString("MSG_ERR_001", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đăng nhập thất bại: Có lỗi phát sinh khi đăng nhập.
        /// </summary>
        internal static string MSG_ERR_002 {
            get {
                return ResourceManager.GetString("MSG_ERR_002", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tạo account thất bại: Username đã tồn tại trong hệ thống.
        /// </summary>
        internal static string MSG_ERR_003 {
            get {
                return ResourceManager.GetString("MSG_ERR_003", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tạo account thất bại: Lỗi không xác định.
        /// </summary>
        internal static string MSG_ERR_004 {
            get {
                return ResourceManager.GetString("MSG_ERR_004", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Đăng nhập thất bại: Hãy nhập tài khoản/Mật khẩu.
        /// </summary>
        internal static string MSG_ERR_005 {
            get {
                return ResourceManager.GetString("MSG_ERR_005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to dbo.
        /// </summary>
        internal static string Schema {
            get {
                return ResourceManager.GetString("Schema", resourceCulture);
            }
        }
    }
}
