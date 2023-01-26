namespace KTechTemplate.Helpers
{
    public static class AppUtilities
    {
        //Application Roles
        public const string Role_Admin = "Admin";
        public const string Role_User_Indi = "Individual"; //External / Company (Schools)
        public const string Role_Student = "Student";
        public const string Role_Staff = "Staff";

        //Payment Statuses
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";

        //Estimated Delivery Date
        public static DateTime DelivaryStartDate = DateTime.Now.AddDays(3);
        public static DateTime DelivaryEndDate = DateTime.Now.AddDays(6);
    }
}
