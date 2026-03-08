import { PaymentResponse } from './payment.model';

export interface AdminDashboardDto {
  // Policy Overview
  totalPolicies: number;
  activePolicies: number;
  submittedPolicies: number;
  underReviewPolicies: number;
  rejectedPolicies: number;
  suspendedPolicies: number;
  lapsedPolicies: number;

  // Claims Overview
  totalClaims: number;
  submittedClaims: number;
  underReviewClaims: number;
  settledClaims: number;
  rejectedClaims: number;
  totalSettledAmount: number;

  // Revenue
  totalPremiumCollected: number;
  totalCommissionPaid: number;
  totalPendingCommission: number;
  monthlyRevenue: MonthlyRevenue[];

  // Users
  totalCustomers: number;
  totalAgents: number;
  totalClaimsOfficers: number;

  // Agent Performance
  agentPerformance: AgentPerformance[];

  // Plan Distribution
  planDistribution: PlanDistribution[];

  // Endorsements
  totalEndorsements: number;
  pendingEndorsements: number;

  // Payments
  recentPayments: PaymentResponse[];
}

export interface MonthlyRevenue {
  year: number;
  month: number;
  monthName: string;
  premiumCollected: number;
  commissionPaid: number;
  policiesActivated: number;
}

export interface AgentPerformance {
  agentId: number;
  agentName: string;
  totalPoliciesAssigned: number;
  activePolicies: number;
  approvedPolicies: number;
  rejectedPolicies: number;
  totalCommissionEarned: number;
  conversionRate: number;
}

export interface PlanDistribution {
  planId: number;
  planName: string;
  totalPolicies: number;
  activePolicies: number;
  totalSumAssured: number;
}

export interface AgentDashboardDto {
  // Policy Summary
  totalAssignedPolicies: number;
  submittedPolicies: number;
  underReviewPolicies: number;
  activePolicies: number;
  rejectedPolicies: number;

  // Commission Summary
  totalCommissionEarned: number;
  pendingCommission: number;
  thisMonthCommission: number;
  lastMonthCommission: number;
  recentCommissions: PolicyCommission[];

  // Endorsements
  pendingEndorsements: any[];
}

export interface PolicyCommission {
  policyNumber: string;
  customerName: string;
  premiumAmount: number;
  commissionAmount: number;
  commissionPercentage: number;
  status: string;
  earnedOn?: string;
}


export interface CustomerDashboardDto {
  // Policy Summary
  totalPolicies: number;
  activePolicies: number;
  pendingPolicies: number;
  rejectedPolicies: number;

  // Claims Summary
  totalClaims: number;
  openClaims: number;
  settledClaims: number;
  totalSettledAmount: number;

  // Payment Summary
  overdueInvoices: number;
  upcomingInvoices: number;
  totalPaidAmount: number;
  totalOutstandingAmount: number;

  // Endorsements
  pendingEndorsements: number;

  // Recent Activity
  recentActivity: RecentActivity[];
  recentPayments: PaymentResponse[];
}

export interface RecentActivity {
  type: string;
  description: string;
  date: string;
}

export interface ClaimsOfficerDashboardDto {
  totalAssignedClaims: number;
  submittedClaims: number;
  underReviewClaims: number;
  settledClaims: number;
  rejectedClaims: number;
  totalSettledAmount: number;
  thisMonthSettledAmount: number;
}

export interface UserRoleCount {
  role: string;
  count: number;
}

export interface ClaimStatusCount {
  status: string;
  count: number;
}

export interface PolicyStatusCount {
  status: string;
  count: number;
}
