import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';
import { RegisterDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  registerForm: FormGroup;
  loading = false;

  constructor(
    private auth: AuthService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.toast.error('Please fill in all fields correctly.');
      // Mark all as touched to trigger validation messages in UI if needed
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.registerForm.errors?.['mismatch']) {
      this.toast.error('Passwords do not match');
      return;
    }

    this.loading = true;

    // Map form value to DTO
    const formValue = this.registerForm.value;
    const registerDto: RegisterDto = {
      fullName: formValue.fullName,
      email: formValue.email,
      phoneNumber: formValue.phoneNumber,
      password: formValue.password
    };

    this.auth.register(registerDto).subscribe({
      next: (res) => {
        this.loading = false;

        // If your backend returns token on register
        if (res?.token) {
          // You may want to auto-login:
          localStorage.setItem('NexaLife_user', JSON.stringify(res));
          localStorage.setItem('NexaLife_token', res.token);
        }

        this.toast.success('Registration successful!');

        // Navigate based on role if needed
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        if (err.error?.errors) {
          const firstError = Object.values(err.error.errors)[0] as string[];
          this.toast.error(firstError[0]);
        } else {
          this.toast.error(err.error?.message || 'Registration failed');
        }
      }
    });
  }
}
