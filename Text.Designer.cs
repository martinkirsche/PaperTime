﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PaperTime {
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
    internal class Text {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Text() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PaperTime.Text", typeof(Text).Assembly);
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
        ///   Looks up a localized string similar to Exit.
        /// </summary>
        internal static string ContextMenuExit {
            get {
                return ResourceManager.GetString("ContextMenuExit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Press any key to continue . . ..
        /// </summary>
        internal static string ContinuePrompt {
            get {
                return ResourceManager.GetString("ContinuePrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Report.txt.
        /// </summary>
        internal static string DefaultFileName {
            get {
                return ResourceManager.GetString("DefaultFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Report-{0:yyyyMM}.md.
        /// </summary>
        internal static string DefaultFileNamePattern {
            get {
                return ResourceManager.GetString("DefaultFileNamePattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        internal static string Error {
            get {
                return ResourceManager.GetString("Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A record&apos;s begin time ({0}) must be prior to its end time ({1})..
        /// </summary>
        internal static string ErrorBeginPriorEnd {
            get {
                return ResourceManager.GetString("ErrorBeginPriorEnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A record&apos;s category must not be empty..
        /// </summary>
        internal static string ErrorCategoryEmpty {
            get {
                return ResourceManager.GetString("ErrorCategoryEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Format error in line {0}: {1}.
        /// </summary>
        internal static string ErrorInLine {
            get {
                return ResourceManager.GetString("ErrorInLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A record&apos;s duration must not be longer than {0:0.00} hours..
        /// </summary>
        internal static string ErrorMaxDurationLimit {
            get {
                return ResourceManager.GetString("ErrorMaxDurationLimit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .LOG
        ///
        ///This is an example time keeping file to explain the format:
        ///
        ///{0}
        ///{1:t} {1:d}	Pets/Cat
        ///
        ///Here I was just petting the cat.
        ///
        ///{2:t} {2:d}
        ///{0}
        ///{3:t} {3:d}	Groceries
        ///
        ///Between petting the cat and doing groceries I did a 1 hour break.
        ///
        ///{4:t} {4:d}	Pets/GuineaPig
        ///
        ///Right after I did groceries I continued at the guinea pigs.
        ///
        ///{5:t} {5:d}
        ///{0}
        ///{6:t} {6:d}	Miscellaneous
        ///
        ///You can remove the lines from above but I would recommend to keep the .LOG or use the F5 key to insert the current timestamp [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ExampleFileContent {
            get {
                return ResourceManager.GetString("ExampleFileContent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Actual Work Time
        ///: {0:0.00} h.
        /// </summary>
        internal static string ReportActualTimeEntry {
            get {
                return ResourceManager.GetString("ReportActualTimeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Category                                .
        /// </summary>
        internal static string ReportCategoryColumn1 {
            get {
                return ResourceManager.GetString("ReportCategoryColumn1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work Time.
        /// </summary>
        internal static string ReportCategoryColumn2 {
            get {
                return ResourceManager.GetString("ReportCategoryColumn2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Distribution (this report).
        /// </summary>
        internal static string ReportCategoryColumn3 {
            get {
                return ResourceManager.GetString("ReportCategoryColumn3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Distribution (overall).
        /// </summary>
        internal static string ReportCategoryColumn4 {
            get {
                return ResourceManager.GetString("ReportCategoryColumn4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Categories.
        /// </summary>
        internal static string ReportCategoryHeading {
            get {
                return ResourceManager.GetString("ReportCategoryHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Report from {0:d} to {1:d}.
        /// </summary>
        internal static string ReportDatePeriodHeading {
            get {
                return ResourceManager.GetString("ReportDatePeriodHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:D}
        ///: {1}.
        /// </summary>
        internal static string ReportHolidaysEntry {
            get {
                return ResourceManager.GetString("ReportHolidaysEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Holidays.
        /// </summary>
        internal static string ReportHolidaysHeading {
            get {
                return ResourceManager.GetString("ReportHolidaysHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Date      .
        /// </summary>
        internal static string ReportRecordColumn1 {
            get {
                return ResourceManager.GetString("ReportRecordColumn1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begin.
        /// </summary>
        internal static string ReportRecordColumn2 {
            get {
                return ResourceManager.GetString("ReportRecordColumn2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to End  .
        /// </summary>
        internal static string ReportRecordColumn3 {
            get {
                return ResourceManager.GetString("ReportRecordColumn3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work Time.
        /// </summary>
        internal static string ReportRecordColumn4 {
            get {
                return ResourceManager.GetString("ReportRecordColumn4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Category.
        /// </summary>
        internal static string ReportRecordColumn5 {
            get {
                return ResourceManager.GetString("ReportRecordColumn5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Records.
        /// </summary>
        internal static string ReportRecordHeading {
            get {
                return ResourceManager.GetString("ReportRecordHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Saldo Work Time
        ///: {3:0.00} h (= {0:0.00} h + {1:0.00} h - {2:0.00} h).
        /// </summary>
        internal static string ReportSaldoTimeEntry {
            get {
                return ResourceManager.GetString("ReportSaldoTimeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Report of {0:MMMM} {0:yyyy}.
        /// </summary>
        internal static string ReportSingleMonthHeading {
            get {
                return ResourceManager.GetString("ReportSingleMonthHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Target &amp; Actual Work Time.
        /// </summary>
        internal static string ReportTargetActualHeading {
            get {
                return ResourceManager.GetString("ReportTargetActualHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Target Work Time
        ///: {2:0.00} h (= {0} Tage * {1:0.##} h).
        /// </summary>
        internal static string ReportTargetTimeEntry {
            get {
                return ResourceManager.GetString("ReportTargetTimeEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to **{1}** work days {0}.
        /// </summary>
        internal static string ReportWorkdaysDescription1 {
            get {
                return ResourceManager.GetString("ReportWorkdaysDescription1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to in **{0:MMMM} {0:yyyy}**.
        /// </summary>
        internal static string ReportWorkdaysDescription2 {
            get {
                return ResourceManager.GetString("ReportWorkdaysDescription2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to between **{0:d}** and **{1:d}**.
        /// </summary>
        internal static string ReportWorkdaysDescription3 {
            get {
                return ResourceManager.GetString("ReportWorkdaysDescription3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workdays.
        /// </summary>
        internal static string ReportWorkdaysHeading {
            get {
                return ResourceManager.GetString("ReportWorkdaysHeading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully created time report.
        /// </summary>
        internal static string SuccessfullyRendered {
            get {
                return ResourceManager.GetString("SuccessfullyRendered", resourceCulture);
            }
        }
    }
}
