using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDotNetCode.Models.Blog
{
    public class Blog
    {
        public string Username { get; set; }

        public int ApplicationUserId { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
