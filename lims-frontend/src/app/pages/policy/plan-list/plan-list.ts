import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PlanResponse } from '../../../core/models/policy.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-plan-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, LoadingSpinner],
  templateUrl: './plan-list.html',
  styleUrl: './plan-list.css'
})
export class PlanList implements OnInit {
  plans: PlanResponse[] = [];
  filtered: PlanResponse[] = [];
  loading = true;
  searchTerm = '';

  constructor(public api: ApiService) { }

  ngOnInit(): void {
    this.api.get<PlanResponse[]>('policy/plans').subscribe({
      next: (r: PlanResponse[]) => {
        this.plans = r;
        this.filtered = r;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  search(): void {
    this.filtered = this.plans.filter(p =>
      p.planName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      p.description.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
}
