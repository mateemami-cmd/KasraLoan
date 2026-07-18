namespace KasraLoan.API.Authorization
{
    public static class LoanPolicies
    {
        public const string AdminOnly = "AdminOnly";

        public const string EmployeeOnly = "EmployeeOnly";

        public const string AdminOrEmployee = "AdminOrEmployee";
    }
}