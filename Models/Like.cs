//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Wenba.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Like
    {
        public int id { get; set; }
        public int UserId { get; set; }
        public Nullable<int> CommentId { get; set; }
        public Nullable<int> NoteId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int LastUpdatedBy { get; set; }
        public System.DateTime LastUpdateDate { get; set; }
    }
}