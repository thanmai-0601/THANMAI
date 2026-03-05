import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { LoginDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  form: LoginDto = {
    email: '',
    password: ''
  };

  loading = false;

  constructor(
    private auth: AuthService,
    private toast: ToastService,
    private router: Router
  ) { }

  onSubmit(): void {

    if (!this.form.email || !this.form.password) {
      this.toast.error('Please enter email and password');
      return;
    }

    this.loading = true;

    this.auth.login(this.form).subscribe({
      next: (res) => {
        this.loading = false;

        this.toast.success(`Welcome back, ${res.fullName}!`);

        // Role-based navigation handled by AuthService
        this.router.navigate([this.auth.getDashboardRoute()]);
      },
      error: (err) => {
        this.loading = false;
        this.toast.error(
          err.error?.message ||
          'Login failed. Please check your credentials.'
        );
      }
    });
  }
}