namespace ClaimPro.Models
{
    public enum ClaimStatus
    {
        Pending,        // Claim is pending
        Approved,       // Claim is approved
        Rejected,       // Claim is rejected
        InvoiceGenerated, // New status for invoice generation
        Paid            // Claim has been paid (optional)
    }
}

