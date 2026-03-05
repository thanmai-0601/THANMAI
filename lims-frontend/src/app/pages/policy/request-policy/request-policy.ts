import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { RequestPolicyDto, PlanResponse } from '../../../core/models/policy.model';

@Component({
  selector: 'app-request-policy',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './request-policy.html',
  styleUrl: './request-policy.css'
})
export class RequestPolicy implements OnInit {
  planId: number = 0;
  plan: PlanResponse | null = null;
  loading = true;
  submitting = false;

  form: RequestPolicyDto = {
    insurancePlanId: 0,
    sumAssured: 500000,
    tenureYears: 10,
    customerAge: 30,
    annualIncome: 500000,
    occupation: '',
    address: '',
    selectedRiders: ''
  };

  availableRidersList: string[] = [];
  selectedRidersList: string[] = [];

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('planId');
    if (id) {
      this.planId = +id;
      this.form.insurancePlanId = this.planId;

      this.api.get<PlanResponse>(`policy/plans/${this.planId}`).subscribe({
        next: (p: PlanResponse) => {
          this.plan = p;
          // Set defaults based on plan constraints
          this.form.sumAssured = p.minSumAssured;
          this.form.tenureYears = p.tenureOptions.length > 0 ? p.tenureOptions[0] : 10;
          this.form.customerAge = p.minEntryAge;
          this.form.annualIncome = Math.max(500000, p.minAnnualIncome);

          if (p.availableRiders) {
            this.availableRidersList = p.availableRiders.split(',').map(r => r.trim()).filter(r => r.length > 0);
          }

          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }

  toggleRider(rider: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.selectedRidersList.includes(rider)) {
        this.selectedRidersList.push(rider);
      }
    } else {
      this.selectedRidersList = this.selectedRidersList.filter(r => r !== rider);
    }
    this.form.selectedRiders = this.selectedRidersList.join(', ');
  }

  submit(): void {
    if (!this.form.occupation || this.form.occupation.trim() === '') {
      this.toast.show('Please enter your occupation.', 'warning');
      return;
    }

    if (this.plan) {
      if (this.form.sumAssured < this.plan.minSumAssured || this.form.sumAssured > this.plan.maxSumAssured) {
        this.toast.show(`Sum assured must be between ₹${this.plan.minSumAssured} and ₹${this.plan.maxSumAssured}.`, 'error');
        return;
      }
      if (!this.plan.tenureOptions.includes(this.form.tenureYears)) {
        this.toast.show(`Tenure must be one of: ${this.plan.tenureOptions.join(', ')} years.`, 'error');
        return;
      }
      if (this.form.customerAge < this.plan.minEntryAge || this.form.customerAge > this.plan.maxEntryAge) {
        this.toast.show(`Age must be between ${this.plan.minEntryAge} and ${this.plan.maxEntryAge} years.`, 'error');
        return;
      }
    }

    this.submitting = true;
    this.api.post<{ message: string; policyId: number }>('policy/request', this.form).subscribe({
      next: (res: { message: string; policyId: number }) => {
        this.toast.show('Policy requested successfully!', 'success');
        this.router.navigate(['/app/policy', res.policyId]);
      },
      error: () => this.submitting = false
    });
  }
}
