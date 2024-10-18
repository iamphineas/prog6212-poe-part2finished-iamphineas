# prog6212-poe-part2finished-iamphineas
login details
Lecturer
james
worthy
jamesworthy@gmail.com 
James2@@1

Coordinator
luke
sabth
lukesabth@gmail.com
Luke2@@1


Manager
David
peterson
Davidpeterson@gmail.com
David2@@1

Feedback
Assumptions: Payment Cycle: The institution operates on a monthly payment cycle for contractor claims. Claim Submission Deadline: There's a specific deadline each month for submitting claims (e.g., the 25th of each month). Approval Hierarchy: There's a defined hierarchy for claim approvals (e.g., Programme Coordinator first, then Academic Manager). Claim Limits: There's a maximum number of hours or amount that can be claimed per month. Contract Terms: Each lecturer has a contract specifying their hourly rate and maximum hours per month. Constraints: Claim Period: The system must only allow claims for the current or previous month, not for future months. Double Claiming Prevention: The system must prevent lecturers from submitting multiple claims for the same work or time period. Approval Workflow: Claims must follow a specific approval workflow and cannot skip steps. Supporting Documentation: Certain types of claims may require mandatory supporting documentation. Claim Modification: Once submitted, lecturers cannot modify their claims. Only admin users can make adjustments. Notification System: The system must send notifications at key points in the claim process (submission, approval, rejection). Audit Trail: The system must maintain a detailed audit trail of all claim activities for compliance and dispute resolution. Conflict of Interest: The system should prevent users from approving their own claims or claims where there's a conflict of interest. Currency: All financial calculations and displays must be in the institution's official currency. Rounding Rules: The system must apply specific rounding rules for financial calculations (e.g., always round to the nearest cent). Concurrent Sessions: The system should handle scenarios where a lecturer might teach multiple sessions concurrently. Rate Changes: The system must be able to handle changes in hourly rates, possibly even mid-month. Claim Adjustments: There should be a process for adjusting or correcting claims after the initial submission and approval.

Feedback:
no github repo submitted Figma/Screenshot & the document should have been committed to the GitHub repo as well to achieve the 5 commits required

How I implemented:

In implementing the feedback from my lecturer, I focused on several key areas of the Contract Monthly Claim System (CMCS) to enhance its functionality and user experience. Here’s how I approached it:

Claims Submission Process: I ensured that the claims submission form captures all necessary details from the lecturers, including hours worked, hourly rate, and additional notes. This aligns with the lecturer's emphasis on the importance of clear input fields to avoid incomplete submissions. Each property in the Claim model is now mandatory, enforcing the requirement that no fields can be left null.

File Uploads: To address the feedback regarding supporting documentation, I implemented an upload feature that allows lecturers to attach relevant files (e.g., .pdf, .docx, .xlsx). This functionality includes validation for file types and size limits, ensuring that only appropriate documents are submitted, which supports the lecturer's point on double-claim prevention and secure documentation.

Comments on Claims: A significant addition is the ability for Coordinators and Managers to leave comments when they reject claims. This provides clarity to the lecturers about the reasons for rejection and fosters transparent communication. The comments are visible to lecturers, ensuring they are informed about any decisions made on their claims.

Claim Approval Workflow: I implemented a structured approval workflow where claims can be verified and either approved or rejected by Coordinators and Managers. This hierarchy follows the lecturer's suggestion for an organized approval system. The status of each claim is tracked in real time, and I included a specific section in the user interface for viewing pending claims.

Claim History and Pending Claims: I structured the user interface to clearly separate pending claims from the claim history. This distinction allows users to easily navigate between the two and facilitates quicker access to the claims that require immediate attention.

Database Integration: I ensured that all properties of the Claim model, including the new OriginalFileName and Comments properties, are migrated to the database. This integration supports the system’s functionality and ensures data persistence.

Role Management: I maintained a centralized User table with role management to control access to various functionalities within the system. Lecturers can only view their claims, while Coordinators and Managers have access to all claims, aligning with the lecturer's emphasis on managing sensitive data appropriately.

By implementing these changes, I aimed to create a more user-friendly and functional system that adheres to the lecturer's feedback and meets the needs of all users involved in the claims process.
