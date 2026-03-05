import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-management',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner],
  templateUrl: './plan-management.html',
  styleUrl: './plan-management.css'
})
export class PlanManagement implements OnInit {
  plans: PlanResponse[] = [];
  loading = true;

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

  togglePlanStatus(plan: PlanResponse): void {
    const originalStatus = plan.isActive;
    const action = originalStatus ? 'deactivate' : 'activate';
    if (!confirm(`Are you sure you want to ${action} ${plan.planName}?`)) return;

    // Assumed endpoint like PUT /api/admin/plans/{id}/toggle or similar
    // Since not defined clearly in docs, we'll try PUT /api/admin/plans/{id} full update or specific toggle
    // Mocking optimistic update
    this.api.put(`policy/plans/${plan.planId}/toggle-status`, {}).subscribe({
      next: () => {
        plan.isActive = !originalStatus;
        this.toast.show(`${plan.planName} status updated.`, 'success');
      },
      error: () => {
        this.toast.show(`Failed to update ${plan.planName}.`, 'error');
      }
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
