namespace ItAuth.Service.ItAuth
{
    public class ItAuthFlow
    {
        public OperationIntent Intent { get; set; }
        public string finalChoice { get; set; }

        public TypeOfAccess? Access { get; set; }
        public string ServerName { get; set; }
        public string SelectedTicketNumber { get; set; }
    }
}