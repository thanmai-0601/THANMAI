import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PolicyResponse, AgentPremiumCalcDto, PremiumCalcResultDto, PolicyDecisionDto } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-agent-policy-review',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinner, StatusBadge],
  templateUrl: './agent-policy-review.html',
  styleUrl: './agent-policy-review.css'
})
export class AgentPolicyReview implements OnInit {
  policyId = 0;
  policy: PolicyResponse | null = null;
  loading = true;

  // Premium Calc Form
  calcForm: AgentPremiumCalcDto = { riskCategory: 'Standard', remarks: '' };
  calculating = false;
  calcResult: PremiumCalcResultDto | null = null;

  // Decision Form
  decisionForm: PolicyDecisionDto = { isApproved: false, rejectionReason: '' };
  submittingDecision = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.policyId = +id;
      this.loadPolicy();
    } else {
      this.loading = false;
    }
  }

  loadPolicy(): void {
    this.api.get<PolicyResponse>(`policy/${this.policyId}`).subscribe({
      next: (r: PolicyResponse) => {
        this.policy = r;
        this.autoEvaluateRiskCategory(r);
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  private autoEvaluateRiskCategory(policy: PolicyResponse): void {
    // If a risk category is already assigned (e.g., from a previous manual save or preview), keep it.
    if (policy.riskCategory) {
      this.calcForm.riskCategory = policy.riskCategory;
      return;
    }

    let riskScore = 0;

    // 1. Age Factor
    if (policy.customerAge) {
      if (policy.customerAge > 50) riskScore += 3;
      else if (policy.customerAge >= 40) riskScore += 2;
      else if (policy.customerAge >= 30) riskScore += 1;
      // Age < 30 adds 0 points (Low risk age bracket)
    }

    // 2. Sum Assured Factor
    if (policy.sumAssured > 5000000) riskScore += 3; // > 50 Lakhs
    else if (policy.sumAssured >= 2000000) riskScore += 2; // 20 Lakhs to 50 Lakhs
    else if (policy.sumAssured >= 1000000) riskScore += 1; // 10 Lakhs to 20 Lakhs
    // < 10 Lakhs adds 0 points (Low risk exposure)

    // 3. Tenure Factor
    if (policy.tenureYears >= 25) riskScore += 2;
    else if (policy.tenureYears >= 15) riskScore += 1;
    // < 15 years adds 0 points

    // 4. Income-to-Debt Exposure Factor
    if (policy.annualIncome) {
      if (policy.annualIncome < 300000 && policy.sumAssured > 1000000) {
        riskScore += 3; // Very high risk: Low income, high coverage
      } else if (policy.annualIncome < 500000 && policy.sumAssured > 2500000) {
        riskScore += 2; // Moderate risk
      }
    }

    // 5. Determine Final Category based on Total Score
    if (riskScore >= 5) {
      this.calcForm.riskCategory = 'High';
    } else if (riskScore >= 2) {
      this.calcForm.riskCategory = 'Standard';
    } else {
      // Score of 0 or 1
      this.calcForm.riskCategory = 'Low';
    }
  }

  calculatePremium(): void {
    if (!this.calcForm.remarks) {
      this.toast.show('Please provide evaluation remarks.', 'warning');
      return;
    }

    this.calculating = true;
    this.api.post<PremiumCalcResultDto>(`policy/${this.policyId}/calculate-premium`, this.calcForm).subscribe({
      next: (res: PremiumCalcResultDto) => {
        this.calcResult = res;
        this.calculating = false;
        this.toast.show('Premium calculated successfully.', 'success');
        this.loadPolicy(); // Refresh to see updated premiumAmount
      },
      error: (err: any) => {
        this.calculating = false;
        this.toast.show(err.error?.message || 'Failed to calculate premium', 'error');
      }
    });
  }

  submitDecision(status: string): void {
    const isApproved = status === 'Approve';
    this.decisionForm.isApproved = isApproved;
    if (isApproved) {
      if (!this.calcForm.remarks) {
        this.toast.show('Please provide evaluation remarks to accept the policy.', 'warning');
        return;
      }
      this.decisionForm.riskCategory = this.calcForm.riskCategory;
      this.decisionForm.agentRemarks = this.calcForm.remarks;
    } else {
      if (!this.calcForm.remarks) {
        this.toast.show('Remarks are required for rejection.', 'warning');
        return;
      }
      this.decisionForm.rejectionReason = this.calcForm.remarks;
    }

    this.submittingDecision = true;
    this.api.post(`policy/${this.policyId}/decision`, this.decisionForm).subscribe({
      next: () => {
        this.toast.show(`Policy ${status}d successfully!`, 'success');
        this.submittingDecision = false;
        this.loadPolicy();
      },
      error: (err: any) => {
        this.submittingDecision = false;
        this.toast.show(err.error?.message || `Failed to ${status} policy`, 'error');
      }
    });
  }
}
