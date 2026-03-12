import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-edit-plan',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinner, AppIcon],
  templateUrl: './edit-plan.html',
  styleUrl: './edit-plan.css'
})
export class EditPlan implements OnInit {
  planId = 0;
  planForm: FormGroup;
  loadingData = true;
  saving = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService,
    private fb: FormBuilder
  ) {
    this.planForm = this.fb.group({
      planName: ['', Validators.required],
      planType: ['TermLife', Validators.required],
      description: ['', Validators.required],
      minSumAssured: [0, [Validators.required, Validators.min(0)]],
      maxSumAssured: [0, [Validators.required, Validators.min(0)]],
      minEntryAge: [0, [Validators.required, Validators.min(0)]],
      maxEntryAge: [0, [Validators.required, Validators.min(0)]],
      minAnnualIncome: [0, [Validators.required, Validators.min(0)]],
      baseRatePer1000: [0, [Validators.required, Validators.min(0)]],
      lowRiskMultiplier: [1.0, [Validators.required, Validators.min(0)]],
      mediumRiskMultiplier: [1.25, [Validators.required, Validators.min(0)]],
      highRiskMultiplier: [1.6, [Validators.required, Validators.min(0)]],
      commissionPercentage: [5, [Validators.required, Validators.min(0)]],
      isActive: [true, Validators.required],
      tenureInput: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id !== null) {
      this.planId = +id;
      this.api.get<PlanResponse>(`policy/plans/${this.planId}`).subscribe({
        next: (res) => {
          this.planForm.patchValue({
            planName: res.planName,
            planType: res.planType,
            description: res.description,
            minSumAssured: res.minSumAssured,
            maxSumAssured: res.maxSumAssured,
            minEntryAge: res.minEntryAge,
            maxEntryAge: res.maxEntryAge,
            minAnnualIncome: res.minAnnualIncome,
            baseRatePer1000: res.baseRatePer1000,
            lowRiskMultiplier: res.lowRiskMultiplier,
            mediumRiskMultiplier: res.mediumRiskMultiplier,
            highRiskMultiplier: res.highRiskMultiplier,
            commissionPercentage: res.commissionPercentage,
            isActive: res.isActive,
            tenureInput: res.tenureOptions.join(', ')
          });
          this.loadingData = false;
        },
        error: () => this.loadingData = false
      });
    } else {
      this.loadingData = false;
    }
  }

  submit(): void {
    if (this.planForm.invalid) {
      this.toast.show('Please fill in required fields correctly.', 'warning');
      this.planForm.markAllAsTouched();
      return;
    }

    const payload = this.planForm.value;

    payload.tenureOptions = payload.tenureInput
      .split(',')
      .map((s: string) => parseInt(s.trim(), 10))
      .filter((n: number) => !isNaN(n) && n > 0);

    if (payload.tenureOptions.length === 0) {
      this.toast.show('Please enter at least one valid tenure option.', 'warning');
      return;
    }

    this.saving = true;
    this.api.put(`policy/plans/${this.planId}`, payload).subscribe({
      next: () => {
        this.toast.show('Insurance Plan updated successfully!', 'success');
        this.router.navigate(['/app/admin/plans']);
      },
      error: (err: any) => {
        this.saving = false;
        this.toast.show(err.error?.message || 'Failed to update plan', 'error');
      }
    });
  }
}
