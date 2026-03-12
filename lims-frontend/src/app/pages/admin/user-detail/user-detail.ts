import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { UserListDto } from '../../../core/models/auth.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, AppIcon],
  templateUrl: './user-detail.html',
  styleUrl: './user-detail.css'
})
export class UserDetail implements OnInit {
  user: UserListDto | null = null;
  userPolicies: any[] = [];
  userClaims: any[] = [];
  loading = true;
  fetchingAssignments = false;

  constructor(private api: ApiService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id !== null) {
      this.loadUser(id);
    } else {
      this.loading = false;
    }
  }

  loadUser(id: string): void {
    this.api.get<UserListDto>(`admin/users/${id}`).subscribe({
      next: (res) => {
        this.user = res;
        this.loading = false;
        this.fetchAssignments(id);
      },
      error: () => this.loading = false
    });
  }

  fetchAssignments(id: string): void {
    this.fetchingAssignments = true;
    this.api.get<any>(`admin/users/${id}/assignments`).subscribe({
      next: (data) => {
        this.userPolicies = data.policies || [];
        this.userClaims = data.claims || [];
        this.fetchingAssignments = false;
      },
      error: () => this.fetchingAssignments = false
    });
  }
}
