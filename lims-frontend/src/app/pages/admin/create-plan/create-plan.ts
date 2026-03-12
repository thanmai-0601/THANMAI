import { Component } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-create-plan',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AppIcon],
  templateUrl: './create-plan.html',
  styleUrl: './create-plan.css'
})
export class CreatePlan {
  planForm: FormGroup;
  loading = false;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.planForm = this.fb.group({
      planName: ['', Validators.required],
      description: ['', Validators.required],
      minSumAssured: [500000, [Validators.required, Validators.min(0)]],
      maxSumAssured: [10000000, [Validators.required, Validators.min(0)]],
      minEntryAge: [18, [Validators.required, Validators.min(0)]],
      maxEntryAge: [55, [Validators.required, Validators.min(0)]],
      minAnnualIncome: [200000, [Validators.required, Validators.min(0)]],
      baseRatePer1000: [1.5, [Validators.required, Validators.min(0)]],
      lowRiskMultiplier: [1.0, [Validators.required, Validators.min(0)]],
      mediumRiskMultiplier: [1.25, [Validators.required, Validators.min(0)]],
      highRiskMultiplier: [1.6, [Validators.required, Validators.min(0)]],
      commissionPercentage: [5, [Validators.required, Validators.min(0)]],
      tenureInput: ['10,15,20', Validators.required]
    });
  }

  submit(): void {
    if (this.planForm.invalid) {
      this.toast.show('Please fill in correctly all required fields.', 'warning');
      this.planForm.markAllAsTouched();
      return;
    }

    const payload = this.planForm.value;

    // Parse tenure input string to array
    payload.tenureOptions = payload.tenureInput
      .split(',')
      .map((s: string) => parseInt(s.trim(), 10))
      .filter((n: number) => !isNaN(n) && n > 0);

    if (payload.tenureOptions.length === 0) {
      this.toast.show('Please enter at least one valid tenure option.', 'warning');
      return;
    }

    this.loading = true;
    this.api.post('policy/plans', payload).subscribe({
      next: () => {
        this.toast.show('Insurance Plan created successfully!', 'success');
        this.router.navigate(['/app/admin/plans']);
      },
      error: (err: any) => {
        this.loading = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
    });
  }
}
