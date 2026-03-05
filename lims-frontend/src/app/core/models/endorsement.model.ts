export interface EndorsementResponse {
  endorsementId: number;
  policyId: number;
  policyNumber: string;
  type: string;
  status: string;
  changeRequested: string;
  oldValue: string;
  customerName: string;
  agentName: string | null;
  agentRemarks: string | null;
  rejectionReason: string | null;
  requestedAt: string;
  reviewedAt: string | null;
  approvedAt: string | null;
}

export interface RequestAddressChangeDto {
  policyId: number;
  newAddress: string;
}

export interface RequestNomineeChangeDto {
  policyId: number;
  changes: string;
}

export interface RequestSumAssuredIncreaseDto {
  policyId: number;
  newSumAssured: number;
}

export interface EndorsementDecisionDto {
  isApproved: boolean;
  rejectionReason?: string;
  agentRemarks?: string;
}
