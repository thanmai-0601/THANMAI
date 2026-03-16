import { Component, OnInit, Input } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, AuthService } from '../../../core/services';
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
  @Input() hideBackButton = false;
  plans: PlanResponse[] = [];
  filtered: PlanResponse[] = [];
  loading = true;
  searchTerm = '';
  selectedPlanType = '';
  hasSettledClaim = false;
  role = '';

  constructor(public api: ApiService, private auth: AuthService, private location: Location) { }

  goBack(): void {
    this.location.back();
  }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

  ngOnInit(): void {
    this.role = this.auth.getUserRole() || '';
    if (this.role === 'Customer') {
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
