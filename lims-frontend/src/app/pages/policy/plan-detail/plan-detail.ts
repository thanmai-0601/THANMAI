import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner],
  templateUrl: './plan-detail.html',
  styleUrl: './plan-detail.css'
})
export class PlanDetail implements OnInit {
  plan: PlanResponse | null = null;
  loading = true;
  isCustomer = false;
  hasSettledClaim = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private auth: AuthService
  ) { }

  ngOnInit(): void {
    this.isCustomer = this.auth.getUserRole() === 'Customer';
    if (this.isCustomer) {
      this.api.get<any>('dashboard/summary').subscribe({
        next: (data) => this.hasSettledClaim = data.hasSettledDeathClaim
      });
    }
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.api.get<PlanResponse>(`policy/plans/${id}`).subscribe({
        next: (r: PlanResponse) => {
          this.plan = r;
          this.loading = false;
        },
        error: () => this.loading = false
      });
    } else {
      this.loading = false;
    }
  }
}
