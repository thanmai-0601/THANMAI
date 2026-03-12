import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { ResetPasswordDto } from '../../../core/models/auth.model';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink, AppIcon],
    templateUrl: './forgot-password.html',
    styleUrl: './forgot-password.css'
})
export class ForgotPassword {
    form: ResetPasswordDto = {
        email: '',
        newPassword: ''
    };

    loading = false;

    constructor(
        private auth: AuthService,
        private toast: ToastService,
        private router: Router
    ) { }

    onSubmit(): void {
        if (!this.form.email || !this.form.newPassword) {
            this.toast.error('Please enter your email and a new password.');
            return;
        }

        if (this.form.newPassword.length < 6) {
            this.toast.error('Password must be at least 6 characters long.');
            return;
        }

        this.loading = true;

        this.auth.resetPassword(this.form).subscribe({
            next: (res) => {
                this.loading = false;
                this.toast.success('Password reset successfully. You can now log in.');
                this.router.navigate(['/login']);
            },
            error: (err) => {
                this.loading = false;
                this.toast.error(
                    err.error?.message ||
                    'Failed to reset password. Please verify your email is correct.'
                );
            }
        });
    }
}
