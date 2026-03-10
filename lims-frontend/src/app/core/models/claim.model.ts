export interface RaiseClaimDto {
  policyNumber: string;
  causeOfDeath: string;
  dateOfDeath: string; // ISO date string
  nomineeName: string;
  nomineeRelationship: string;
  nomineeIdNumber: string;
  bankAccountName: string;
  bankAccountNumber: string;
  bankIfscCode: string;
  deathCertificate: ClaimDocumentDto;
  nomineeIdProof: ClaimDocumentDto;
}

export interface ClaimNominee {
  fullName: string;
  relationship: string;
  age: number;
  contactNumber: string;
}

export interface ClaimResponse {
  claimId: number;
  claimNumber: string;
  status: string;
  claimType: string;
  policyId: number;
  policyNumber: string;
  planName: string;
  sumAssured: number;
  tenureYears: number;
  riskCategory: string | null;
  premiumAmount: number | null;
  policyActiveFrom: string | null;
  policyActiveTo: string | null;
  customerId: number;
  customerName: string;
  customerEmail: string;
  claimsOfficerId: number | null;
  claimsOfficerName: string | null;
  claimReason: string;
  claimAmount: number | null;
  settledAmount: number | null;
  officerRemarks: string | null;
  rejectionReason: string | null;
  // Bank details
  bankAccountName: string | null;
  bankAccountNumber: string | null;
  bankIfscCode: string | null;
  transferReference: string | null;
  // Nominees from the policy
  nominees: ClaimNominee[];
  // Payment summary
  totalInvoices: number;
  paidInvoices: number;
  overdueInvoices: number;
  totalPremiumPaid: number;
  // Documents
  documents: ClaimDocumentResponse[];
  submittedAt: string;
  assignedAt: string | null;
  reviewStartedAt: string | null;
  approvedAt: string | null;
  settledAt: string | null;
  rejectedAt: string | null;
}

export interface ClaimDocumentDto {
  documentType: string;
  fileName: string;
  fileBase64: string;
}

export interface ClaimDocumentResponse {
  documentId: number;
  documentType: string;
  fileName: string;
  filePath: string;
  status: string;
  uploadedAt: string;
}

export interface ClaimDecisionDto {
  isApproved: boolean;
  settledAmount: number | null;
  rejectionReason?: string;
  officerRemarks?: string;
}
