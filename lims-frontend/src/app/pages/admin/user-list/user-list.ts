import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { UserListDto } from '../../../core/models/auth.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css'
})
export class UserList implements OnInit {
  users: UserListDto[] = [];
  loading = true;

  constructor(private api: ApiService, private toast: ToastService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.api.get<UserListDto[]>('admin/users').subscribe({
      next: (res: UserListDto[]) => {
        this.users = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  toggleStatus(user: UserListDto): void {
    // Assuming a generic PUT or PATCH endpoint for toggling active status
    // If not standard, we can mock or do a specific endpoint
    // Usually it might be PUT /api/admin/users/{id}/toggle-status
    this.api.put(`admin/users/${user.userId}/toggle-status`, {}).subscribe({
      next: () => {
        this.toast.show(`User ${user.fullName} status updated.`, 'success');
        user.isActive = !user.isActive; // Optimistic UI update
      },
      error: () => this.toast.show('Failed to toggle status.', 'error')
    });
  }
}
