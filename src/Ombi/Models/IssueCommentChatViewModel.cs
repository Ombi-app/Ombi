using System;

namespace Ombi.Models
{
    public class IssueCommentChatViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public string Username { get; set; }
        public bool AdminComment { get; set; }
    }
}