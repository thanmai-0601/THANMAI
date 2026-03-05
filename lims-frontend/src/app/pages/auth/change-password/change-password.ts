import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { ChangePasswordDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePassword {

  form: ChangePasswordDto = {
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  };

  loading = false;
confirmPassword: any;

  constructor(
    private auth: AuthService,
    private toast: ToastService
  ) {}

  onSubmit(): void {

    if (!this.form.currentPassword || 
        !this.form.newPassword || 
        !this.form.confirmNewPassword) {
      this.toast.error('All fields are required');
      return;
    }

    if (this.form.newPassword !== this.form.confirmNewPassword) {
      this.toast.error('Passwords do not match');
      return;
    }

    this.loading = true;

    this.auth.changePassword(this.form).subscribe({
      next: () => {
        this.loading = false;

        this.toast.success('Password updated successfully');

        // Proper reset
        this.form = {
          currentPassword: '',
          newPassword: '',
          confirmNewPassword: ''
        };
      },
      error: (err) => {
        this.loading = false;
        this.toast.error(
          err.error?.message || 'Failed to update password'
        );
      }
    });
  }
}