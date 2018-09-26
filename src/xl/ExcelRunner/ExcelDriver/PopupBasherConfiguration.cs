//------------------------------------------------------------------------------
// <copyright file="PopupBasherConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Retreives popup bashing configuration from .config file
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Configuration;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Excel.Win32;

    /// <summary>
    ///   <para>Class representing the XML configuration of the popup basher.</para>
    /// </summary>
    public class PopupBasherConfiguration
    {
        /// <summary>
        /// Default time between popup checks
        /// </summary>
        private const int CHECKSTATUSPERIODDEFAULT = 2000;

        /// <summary>
        /// Singleton instance representing the configuration
        /// </summary>
        private static PopupBasherConfiguration myConfiguration;

        /// <summary>
        /// Period specified in the config
        /// </summary>
        private int myPeriod;

        /// <summary>
        /// Configuration for popup windows in the config
        /// </summary>
        private PopupConfigWindows myWindows;

        /// <summary>
        ///   <para>Initializes a new instance of the PopupBasherConfiguration class.</para>
        /// </summary>
        public PopupBasherConfiguration()
        {
        }

        /// <summary>
        ///   <para>Type of action to do on the popup.</para>
        /// </summary>
        public enum ActionType
        {
            /// <summary>
            ///   <para>Do the default action.</para>
            /// </summary>
            DoDefault,

            /// <summary>
            ///   <para>Just check for existance.</para>
            /// </summary>
            Exist
        }

        /// <summary>
        ///   <para>Mode in which to interpret the search strings.</para>
        /// </summary>
        public enum SearchMode
        {
            /// <summary>
            ///   <para>Search for exact match only.</para>
            /// </summary>
            Exact,

            /// <summary>
            ///   <para>Interpret search string as a RegEx.</para>
            /// </summary>
            RegEx
        }

        /// <summary>
        ///   <para>Enumeration of system roles.</para>
        /// </summary>
        public enum RoleSystem : uint
        {
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_NONE = 0x0,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_TITLEBAR = 0x1,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_MENUBAR = 0x2,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SCROLLBAR = 0x3,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_GRIP = 0x4,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SOUND = 0x5,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CURSOR = 0x6,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CARET = 0x7,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_ALERT = 0x8,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_WINDOW = 0x9,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CLIENT = 0xa,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_MENUPOPUP = 0xb,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_MENUITEM = 0xc,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_TOOLTIP = 0xd,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_APPLICATION = 0xe,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_DOCUMENT = 0xf,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PANE = 0x10,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CHART = 0x11,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_DIALOG = 0x12,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_BORDER = 0x13,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_GROUPING = 0x14,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SEPARATOR = 0x15,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_TOOLBAR = 0x16,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_STATUSBAR = 0x17,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_TABLE = 0x18,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_COLUMNHEADER = 0x19,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_ROWHEADER = 0x1a,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_COLUMN = 0x1b,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_ROW = 0x1c,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CELL = 0x1d,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_LINK = 0x1e,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_HELPBALLOON = 0x1f,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CHARACTER = 0x20,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_LIST = 0x21,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_LISTITEM = 0x22,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_OUTLINE = 0x23,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_OUTLINEITEM = 0x24,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PAGETAB = 0x25,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PROPERTYPAGE = 0x26,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_INDICATOR = 0x27,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_GRAPHIC = 0x28,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_STATICTEXT = 0x29,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_TEXT = 0x2a,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PUSHBUTTON = 0x2b,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CHECKBUTTON = 0x2c,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_RADIOBUTTON = 0x2d,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_COMBOBOX = 0x2e,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_DROPLIST = 0x2f,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PROGRESSBAR = 0x30,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_DIAL = 0x31,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_HOTKEYFIELD = 0x32,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SLIDER = 0x33,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SPINBUTTON = 0x34,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_DIAGRAM = 0x35,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_ANIMATION = 0x36,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_EQUATION = 0x37,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_BUTTONDROPDOWN = 0x38,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_BUTTONMENU = 0x39,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_BUTTONDROPDOWNGRID = 0x3a,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_WHITESPACE = 0x3b,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_PAGETABLIST = 0x3c,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_CLOCK = 0x3d,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_SPLITBUTTON = 0x3e,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_IPADDRESS = 0x3f,
			/// <summary>
			///   <para />
			/// </summary>
            ROLE_SYSTEM_OUTLINEBUTTON = 0x40
        }

        /// <summary>
        ///   <para>Gets the singleton of AVConfiguration created during construction.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [XmlIgnore()]
        public static PopupBasherConfiguration Instance
        {
            get
            {
                if (null == myConfiguration)
                {
                    Init();
                }

                return myConfiguration;
            }
        }

        /// <summary>
        ///   <para>Gets or sets period between checks for open popups to bash.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [XmlAttribute("Period")]
        public int Period
        {
            get
            {
                return this.myPeriod;
            }

            set
            {
                this.myPeriod = value;
            }
        }

        /// <summary>
        ///   <para>Gets or sets the windows to look for node.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [XmlElement("Windows")]
        public PopupConfigWindows Windows
        {
            get
            {
                return this.myWindows;
            }

            set
            {
                this.myWindows = value;
            }
        }

        /// <summary>
        /// Initializes the configuration section by parsing the XML
        /// </summary>
        private static void Init()
        {
            try
            {
                if (null == myConfiguration)
                {
                    // Fix bug 21191: call Configuration.GetSection instead of ConfigurationManager.GetSection
                    // to read config file. ConfigurationManager.GetSection may throw ConfigurationErrorsException
                    // on .Net 4.0. Related KB: http://support.microsoft.com/?kbid=2580188
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (config != null)
                    {
                        ConfigurationSection section = config.GetSection("PopupBasherConfiguration");
                        if (section != null)
                        {
                            XmlDocument xdoc = new XmlDocument() { XmlResolver = null };
                            xdoc.LoadXml(section.SectionInformation.GetRawXml());
                            XmlNodeReader xreader = new XmlNodeReader(xdoc.DocumentElement);

                            XmlSerializer serializer = new XmlSerializer(typeof(PopupBasherConfiguration));
                            myConfiguration = (PopupBasherConfiguration)serializer.Deserialize(xreader);
                        }
                    }
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                // use default configuration (happens in finally block)
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, ex.ToString());
            }
            finally
            {
                // Sometimes GetSection returns null instead of throwing an exception, so regardless of exception catching, use
                // a finally block to check if configuration was loaded and load defaults if none was loaded.
                if (myConfiguration == null)
                {
                    // Couldn't load the config file, so set defaults
                    myConfiguration = new PopupBasherConfiguration();
                    myConfiguration.Period = CHECKSTATUSPERIODDEFAULT;
                }
            }
        }

        /// <summary>
        ///   <para>Represents the popup windows node.</para>
        /// </summary>
        public class PopupConfigWindows
        {
            /// <summary>
            /// Set of popup window configs
            /// </summary>
            private PopupConfigWindow[] myPopupWindows;

            /// <summary>
            ///   <para>Gets or sets the set of popup window configs.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlElement("Window")]
            public PopupConfigWindow[] Windows
            {
                get
                {
                    return this.myPopupWindows;
                }

                set
                {
                    this.myPopupWindows = value;
                }
            }
        }

        /// <summary>
        ///   <para>Class representing a popup window.</para>
        /// </summary>
        public class PopupConfigWindow
        {
            /// <summary>
            /// Title of the popup window
            /// </summary>
            private string popupTitle;

            /// <summary>
            /// Name of the class of the popup window
            /// </summary>
            private string popupClassName;

            /// <summary>
            /// Children of the window
            /// </summary>
            private PopupConfigChild[] popupChildren;

            /// <summary>
            ///   <para>Gets or sets the title of the popup window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Title")]
            public string Title
            {
                get
                {
                    return this.popupTitle;
                }

                set
                {
                    this.popupTitle = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the name of the class of the popup window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Class")]
            public string ClassName
            {
                get
                {
                    return this.popupClassName;
                }

                set
                {
                    this.popupClassName = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the children of the window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlElement("Child")]
            public PopupConfigChild[] Children
            {
                get
                {
                    return this.popupChildren;
                }

                set
                {
                    this.popupChildren = value;
                }
            }
        }

        /// <summary>
        ///   <para>Class representing the configuration specified in the child of the popup window.</para>
        /// </summary>
        public class PopupConfigChild
        {
            /// <summary>
            /// Search mode in config
            /// </summary>
            private SearchMode childSearchMode;

            /// <summary>
            /// Title of window
            /// </summary>
            private string childTitle;

            /// <summary>
            /// Class of window
            /// </summary>
            private string childClass;

            /// <summary>
            /// Role of window
            /// </summary>
            private RoleSystem childRole;

            /// <summary>
            /// Action to be taken
            /// </summary>
            private ActionType childAction;

            /// <summary>
            ///   <para>Gets or sets the title of the child window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Title")]
            public string Title
            {
                get
                {
                    return this.childTitle;
                }

                set
                {
                    this.childTitle = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the class of the child window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Class")]
            public string ClassName
            {
                get
                {
                    return this.childClass;
                }

                set
                {
                    this.childClass = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the role of the child window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Role")]
            public RoleSystem Role
            {
                get
                {
                    return this.childRole;
                }

                set
                {
                    this.childRole = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the action to be taken on the child window.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Action")]
            public ActionType Action
            {
                get
                {
                    return this.childAction;
                }

                set
                {
                    this.childAction = value;
                }
            }

            /// <summary>
            ///   <para>Gets or sets the search mode.</para>
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            [XmlAttribute("Search")]
            public SearchMode Search
            {
                get
                {
                    return this.childSearchMode;
                }

                set
                {
                    this.childSearchMode = value;
                }
            }
        }
    }
}
