export interface PlanResponse {
  planId: number;
  planName: string;
  description: string;
  minSumAssured: number;
  maxSumAssured: number;
  tenureOptions: number[];
  availableRiders: string;
  minEntryAge: number;
  maxEntryAge: number;
  minAnnualIncome: number;
  baseRatePer1000: number;
  lowRiskMultiplier: number;
  mediumRiskMultiplier: number;
  highRiskMultiplier: number;
  commissionPercentage: number;
  isActive: boolean;
  createdAt: string;
  totalPoliciesCount: number;
}

export interface CreatePlanDto {
  planName: string;
  description: string;
  minSumAssured: number;
  maxSumAssured: number;
  tenureOptions: number[];
  availableRiders: string;
  minEntryAge: number;
  maxEntryAge: number;
  minAnnualIncome: number;
  baseRatePer1000: number;
  lowRiskMultiplier: number;
  mediumRiskMultiplier: number;
  highRiskMultiplier: number;
  commissionPercentage: number;
}

export interface UpdatePlanDto {
  planName: string;
  description: string;
  minSumAssured: number;
  maxSumAssured: number;
  tenureOptions: number[];
  availableRiders: string;
  minEntryAge: number;
  maxEntryAge: number;
  minAnnualIncome: number;
  baseRatePer1000: number;
  lowRiskMultiplier: number;
  mediumRiskMultiplier: number;
  highRiskMultiplier: number;
  commissionPercentage: number;
  isActive: boolean;
}

export interface RequestPolicyDto {
  insurancePlanId: number;
  sumAssured: number;
  tenureYears: number;
  customerAge: number;
  annualIncome: number;
  occupation: string;
  address: string;
  selectedRiders: string;
}

export interface PolicyResponse {
  policyId: number;
  policyNumber: string;
  status: string;
  insurancePlanId: number;
  planName: string;
  sumAssured: number;
  tenureYears: number;
  selectedRiders: string;
  customerId: number;
  customerName: string;
  customerEmail: string;
  agentId: number | null;
  agentName: string | null;
  agentEmail: string | null;
  customerAge: number | null;
  annualIncome: number | null;
  occupation: string | null;
  riskCategory: string | null;
  premiumAmount: number | null;
  agentRemarks: string | null;
  rejectionReason: string | null;
  createdAt: string;
  submittedAt: string | null;
  agentAssignedAt: string | null;
  approvedAt: string | null;
  activeFrom: string | null;
  activeTo: string | null;
  nominees: NomineeResponse[];
  documents: DocumentResponse[];
  hasSettledClaim: boolean;
}

export interface PremiumPreviewDto {
  policyNumber: string;
  planName: string;
  sumAssured: number;
  tenureYears: number;
  riskCategory: string;
  annualPremium: number;
  monthlyPremium: number;
  quarterlyPremium: number;
  totalPayableOverTenure: number;
  sumAssuredOnDeath: number;
  sumAssuredOnMaturity: number;
  agentRemarks: string;
  status: string;
  benefits: string[];
}

export interface NomineeResponse {
  nomineeId: number;
  fullName: string;
  relationship: string;
  age: number;
  contactNumber: string;
  allocationPercentage: number;
}

export interface AddNomineeDto {
  fullName: string;
  relationship: string;
  age: number;
  contactNumber: string;
  allocationPercentage: number;
}

export interface SubmitNomineesDto {
  nominees: AddNomineeDto[];
}

export interface DocumentResponse {
  documentId: number;
  documentType: string;
  fileName: string;
  filePath: string;
  status: string;
  uploadedAt: string;
}

export interface UploadDocumentDto {
  documentType: string;
  fileName: string;
  fileBase64: string;
}

export interface AgentPremiumCalcDto {
  riskCategory: string;
  remarks: string;
}

export interface PremiumCalcResultDto {
  sumAssured: number;
  tenureYears: number;
  riskCategory: string;
  baseRatePer1000: number;
  riskMultiplierApplied: number;
  annualPremium: number;
  monthlyPremium: number;
  quarterlyPremium: number;
  totalPremiumOverTenure: number;
  commissionAmount: number;
  commissionPercentage: number;
}

export interface PolicyDecisionDto {
  isApproved: boolean;
  rejectionReason?: string;
  riskCategory?: string;
  agentRemarks?: string;
}
