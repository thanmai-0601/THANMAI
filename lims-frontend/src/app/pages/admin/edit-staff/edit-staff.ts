import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { UserListDto, UpdateStaffDto } from '../../../core/models/auth.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-edit-staff',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, LoadingSpinner],
  templateUrl: './edit-staff.html'
})
export class EditStaff implements OnInit {
  editStaffForm: FormGroup;
  loading = true;
  submitting = false;
  user: UserListDto | null = null;
  userId: string | null = null;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.editStaffForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      role: ['Agent', Validators.required]
    });
  }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (this.userId) {
      this.api.get<UserListDto>(`admin/users/${this.userId}`).subscribe({
        next: (res) => {
          this.user = res;

          if (res.role === 'Admin' || res.role === 'Customer') {
            this.toast.show(`Cannot edit ${res.role} accounts from here.`, 'error');
            this.router.navigate(['/app/admin/users']);
            return;
          }

          this.editStaffForm.patchValue({
            fullName: res.fullName,
            email: res.email,
            phoneNumber: res.phoneNumber,
            role: res.role
          });
          this.loading = false;
        },
        error: () => {
          this.toast.show('Failed to load user details.', 'error');
          this.loading = false;
        }
      });
    } else {
      this.loading = false;
    }
  }

  submit(): void {
    if (this.editStaffForm.invalid || !this.userId) {
      this.toast.show('Please fill in all details.', 'warning');
      this.editStaffForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const formValue = this.editStaffForm.value;
    const payload: UpdateStaffDto = {
      fullName: formValue.fullName,
      email: formValue.email,
      phoneNumber: formValue.phoneNumber,
      role: formValue.role
    };

    this.api.put(`admin/users/${this.userId}`, payload).subscribe({
      next: () => {
        this.toast.show(`Staff details updated successfully!`, 'success');
        this.router.navigate(['/app/admin/users']);
      },
      error: (err) => {
        this.submitting = false;
        const msg = err.error?.message || 'Failed to update staff details.';
        this.toast.show(msg, 'error');
      }
    });
  }
}
