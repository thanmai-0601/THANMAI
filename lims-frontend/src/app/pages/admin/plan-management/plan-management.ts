import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-management',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, AppIcon],
  templateUrl: './plan-management.html',
  styleUrl: './plan-management.css'
})
export class PlanManagement implements OnInit {
  plans: PlanResponse[] = [];
  loading = true;
  selectedType: string = 'All';

  get filteredPlans(): PlanResponse[] {
    if (this.selectedType === 'All') return this.plans;
    return this.plans.filter(p => p.planType === this.selectedType);
  }

  constructor(private api: ApiService, private toast: ToastService) { }

  ngOnInit(): void {
    this.loadPlans();
  }

  loadPlans(): void {
    this.loading = true;
    this.api.get<PlanResponse[]>('policy/plans').subscribe({
      next: (res) => {
        this.plans = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }



  deletePlan(plan: PlanResponse): void {
    if (!confirm(`Are you sure you want to completely DELETE ${plan.planName}? This cannot be undone.`)) return;

    this.api.delete(`policy/plans/${plan.planId}`).subscribe({
      next: () => {
        this.toast.show('Plan deleted successfully.', 'success');
        this.plans = this.plans.filter(p => p.planId !== plan.planId);
      },
      error: () => this.toast.show('Failed to delete plan.', 'error')
    });
  }
}
