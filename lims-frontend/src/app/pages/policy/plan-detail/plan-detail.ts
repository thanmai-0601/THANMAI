import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AuthService } from '../../../core/services/auth';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, AppIcon],
  templateUrl: './plan-detail.html',
  styleUrl: './plan-detail.css'
})
export class PlanDetail implements OnInit {
  plan: PlanResponse | null = null;
  loading = true;
  isCustomer = false;
  hasSettledClaim = false;
  customerDetails: any = null;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private auth: AuthService,
    private location: Location
  ) { }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    this.isCustomer = this.auth.getUserRole() === 'Customer';
    if (this.isCustomer) {
      this.auth.getCurrentUserFromApi().subscribe({
        next: (data) => this.customerDetails = data
      });
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

  get customerAge(): number {
    if (!this.customerDetails?.dateOfBirth) return 0;
    const birthDate = new Date(this.customerDetails.dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    return age;
  }

  get isAgeBelowMin(): boolean {
    if (!this.plan || !this.customerDetails) return false;
    return this.customerAge < this.plan.minEntryAge;
  }

  get isAgeAboveMax(): boolean {
    if (!this.plan || !this.customerDetails) return false;
    return this.customerAge > this.plan.maxEntryAge;
  }
}
