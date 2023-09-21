using System;
using System.Collections.Generic;
using System.Text;

namespace PsebPrimaryMiddle.Helpers
{
    /// <summary>
    /// State the application is deployed at
    /// </summary>
    public enum ApplicationState
    {
        /// <summary>
        /// When the application is on SNPL Development server
        /// </summary>        
        Development,

        /// <summary>
        /// When the application is on Client staging Server 
        /// </summary>
        Staging,

        /// <summary>
        /// When the application is deployed on the Client product server
        /// </summary>
        Production
    }

    /// <summary>
    /// Check for WWW status
    /// </summary>
    public enum WWWStatus
    {
        Ignore,
        Require,
        Remove
    }

    /// <summary>
    /// Check for SSL 
    /// </summary>
    public enum SSLSettings
    {
        Ignore,
        All, 
        Password
    }

    /// <summary>
    /// Describes the Lokkup categories
    /// </summary>
    public enum LookUpCategoryKey
    {
        /// <summary>
        /// Not set for nullable type
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Different type of cultures supported by the application
        /// </summary>
        CultureCodes = 10,

        /// <summary>
        /// List of products in the marykay list
        /// </summary>
        Products = 20
    }

    /// <summary>
    /// Type of sharing 
    /// </summary>
    public enum ShareTypes : short
    {
        None = 0,

        TellAFriend = 1,

        Facebook = 2,

        Twitter = 3, 

        Vote = 4
    }

    /// <summary>
    /// Type of exceptions
    /// </summary>
    public enum ExceptionTypes
    {
        /// <summary>
        /// General Exceptions
        /// </summary>
        General = 1,

        /// <summary>
        /// Page Load Exception
        /// </summary>
        PageLoad = 2,

        /// <summary>
        /// Database Exception
        /// </summary>
        Database = 3,

        /// <summary>
        /// Page Not Found 
        /// </summary>
        PageNotFound = 4,

        /// <summary>
        /// Resource Not found
        /// </summary>
        ResourceNotFound = 5,

        /// <summary>
        /// Access Denied
        /// </summary>
        AccessDenied = 6,

        /// <summary>
        /// User Control Load Exception
        /// </summary>
        ControlLoad = 7,

        /// <summary>
        /// All Exceptions
        /// </summary>
        AllEexception = 8,

        /// <summary>
        /// Web services exception
        /// </summary>
        WebService = 9
    }

    /// <summary>
    /// Type of Action 
    /// </summary>
    public enum ActivityEnum : byte
    {
        Added = 1,
        Deleted = 3,
        Updated = 2,
        Viewed = 4,
        PublishingStatus = 5,
    }

   
    /// <summary>
    /// Modules for Audit History
    /// </summary>
    public enum AuditModulesEnum
    {
        Unknown = 0
    }

    /// <summary>
    /// Judging publishing Status
    /// </summary>
    public enum JudgingStatus : short
    {
        Pending = 1,
        Judged = 2,
        Saved = 3
    }

    /// <summary>
    /// Publishing status of data
    /// </summary>
    public enum PublishingStatus : short
    {
        /// <summary>
        /// Initial no status
        /// </summary>
        None = 0,

        /// <summary>
        /// Draft mode
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Pending approval
        /// </summary>
        PendingApproval = 2, 

        /// <summary>
        /// Active
        /// </summary>
        Active = 3,

        /// <summary>
        /// Archived
        /// </summary>
        Archived = 4,

        /// <summary>
        /// Pending Deletion
        /// </summary>
        PendingDeletion = 5
    }

    /// <summary>
    /// Type of file extensions supported
    /// </summary>
    public enum FileExtensions : short
    {
        doc = 2,
        jpg = 1,
        pdf = 5,
        txt = 3,
        xls = 4
    }

    /// <summary>
    /// Describes the news type
    /// </summary>
    public enum MailFormat : short
    {
        /// <summary>
        /// "HTML" 
        /// </summary>
        Html = 1,

        /// <summary>
        /// "TEXT" 
        /// </summary>
        Text = 2,
    }

    /// <summary>
    /// Describes the message format
    /// </summary>
    public enum MessageTypeEnum
    {
        Success, 
        Information,
        Error,
        Warning
    }

    public enum WebMethodStatus
    {
        Ok = 0,
        Failure = 1,
    }

    /// <summary>
    /// The sort order direction for sorting the collection
    /// </summary>
    public enum SortOrder : int
    {
        /// <summary>
        /// Ascending
        /// </summary>
        Ascending = 1,

        /// <summary>
        /// Descending
        /// </summary>
        Descending = 2
    };


    public enum RowStatus
    {
        /// <summary>
        /// "In Active" 
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// "Active" 
        /// </summary>
        Active = 1,
       
    }

}
