﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASCOM.Skybadger.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM8")]
        public string DomeCommPort {
            get {
                return ((string)(this["DomeCommPort"]));
            }
            set {
                this["DomeCommPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("19200,n,8,1")]
        public string DomeCommSetting {
            get {
                return ((string)(this["DomeCommSetting"]));
            }
            set {
                this["DomeCommSetting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("19200,n,8,1")]
        public string FixedCommSetting {
            get {
                return ((string)(this["FixedCommSetting"]));
            }
            set {
                this["FixedCommSetting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("180")]
        public int ParkPosition {
            get {
                return ((int)(this["ParkPosition"]));
            }
            set {
                this["ParkPosition"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("180")]
        public int SlitAzimuth {
            get {
                return ((int)(this["SlitAzimuth"]));
            }
            set {
                this["SlitAzimuth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM7")]
        public string ObboCommPort {
            get {
                return ((string)(this["ObboCommPort"]));
            }
            set {
                this["ObboCommPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("61")]
        public int I2CMagnetometerAddr {
            get {
                return ((int)(this["I2CMagnetometerAddr"]));
            }
            set {
                this["I2CMagnetometerAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("176")]
        public int I2CMotorCtrlAddr {
            get {
                return ((int)(this["I2CMotorCtrlAddr"]));
            }
            set {
                this["I2CMotorCtrlAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("198")]
        public int I2CLCDAddr {
            get {
                return ((int)(this["I2CLCDAddr"]));
            }
            set {
                this["I2CLCDAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("180")]
        public string DomeHomeLocation {
            get {
                return ((string)(this["DomeHomeLocation"]));
            }
            set {
                this["DomeHomeLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int DomeSlewMode {
            get {
                return ((int)(this["DomeSlewMode"]));
            }
            set {
                this["DomeSlewMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AtPark {
            get {
                return ((bool)(this["AtPark"]));
            }
            set {
                this["AtPark"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AtHome {
            get {
                return ((bool)(this["AtHome"]));
            }
            set {
                this["AtHome"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("85")]
        public int I2CObboProxyAddr {
            get {
                return ((int)(this["I2CObboProxyAddr"]));
            }
            set {
                this["I2CObboProxyAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("85")]
        public int I2CDomeProxyAddr {
            get {
                return ((int)(this["I2CDomeProxyAddr"]));
            }
            set {
                this["I2CDomeProxyAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("90")]
        public int SlitAltitude {
            get {
                return ((int)(this["SlitAltitude"]));
            }
            set {
                this["SlitAltitude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public float SynchroOffsetAzimuth {
            get {
                return ((float)(this["SynchroOffsetAzimuth"]));
            }
            set {
                this["SynchroOffsetAzimuth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public float SynchroOffsetAltitude {
            get {
                return ((float)(this["SynchroOffsetAltitude"]));
            }
            set {
                this["SynchroOffsetAltitude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int I2CVoltsAddr {
            get {
                return ((int)(this["I2CVoltsAddr"]));
            }
            set {
                this["I2CVoltsAddr"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int MagneticDeclination {
            get {
                return ((int)(this["MagneticDeclination"]));
            }
            set {
                this["MagneticDeclination"] = value;
            }
        }
    }
}
