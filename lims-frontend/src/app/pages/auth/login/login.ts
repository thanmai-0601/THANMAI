import { Component } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { LoginDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AppIcon],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  form: LoginDto = {
    email: '',
    password: ''
  };

  loading = false;
  showPassword = false;
  captchaText = '';
  userInputCaptcha = '';

  constructor(
    private auth: AuthService,
    private toast: ToastService,
    private router: Router
  ) { 
    this.generateCaptcha();
  }

  generateCaptcha(): void {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    let result = '';
    for (let i = 0; i < 6; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.captchaText = result;
    this.userInputCaptcha = '';
  }

  onSubmit(): void {

    if (!this.form.email || !this.form.password) {
      this.toast.error('Please enter email and password');
      return;
    }

    if (this.userInputCaptcha.toUpperCase() !== this.captchaText) {
      this.toast.error('Invalid CAPTCHA code. Please try again.');
      this.generateCaptcha();
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
        this.toast.error(ApiService.getErrorMessage(err));
        this.generateCaptcha();
      }
    });
  }
}