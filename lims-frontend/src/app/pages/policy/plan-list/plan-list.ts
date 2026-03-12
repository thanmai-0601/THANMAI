import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinner, AppIcon],
  templateUrl: './plan-list.html',
  styleUrl: './plan-list.css'
})
export class PlanList implements OnInit {
  plans: PlanResponse[] = [];
  filtered: PlanResponse[] = [];
  loading = true;
  searchTerm = '';
  selectedPlanType = '';
  hasSettledClaim = false;

  constructor(public api: ApiService) { }

  ngOnInit(): void {
    const user = JSON.parse(localStorage.getItem('NexaLife_user') || '{}');
    if (user.role === 'Customer') {
      this.api.get<any>('dashboard/summary').subscribe({
        next: (data) => this.hasSettledClaim = data.hasSettledDeathClaim
      });
    }

    this.api.get<PlanResponse[]>('policy/plans').subscribe({
      next: (r: PlanResponse[]) => {
        this.plans = r;
        this.filtered = r;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  setPlanType(type: string): void {
    this.selectedPlanType = type;
    this.search();
  }

  search(): void {
    this.filtered = this.plans.filter(p => {
      const matchesText = p.planName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        p.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesType = !this.selectedPlanType || p.planType === this.selectedPlanType;

      return matchesText && matchesType;
    });
  }
}
