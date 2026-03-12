import { Component } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { RegisterDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AppIcon],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  registerForm: FormGroup;
  loading = false;
  maxDate: string;
  currentStep = 1;

  constructor(
    private auth: AuthService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder
  ) {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    this.maxDate = today.toISOString().split('T')[0];
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      dateOfBirth: ['', [Validators.required, this.minAgeValidator(18)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      bankAccountName: ['', Validators.required],
      bankAccountNumber: ['', [Validators.required, Validators.pattern('^[0-9]{9,20}$')]],
      bankIfscCode: ['', [Validators.required, Validators.pattern('^[A-Z]{4}0[A-Z0-9]{6}$')]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  minAgeValidator(minAge: number) {
    return (control: any) => {
      if (control.value) {
        const selectedDate = new Date(control.value);
        const minDate = new Date();
        minDate.setFullYear(minDate.getFullYear() - minAge);
        minDate.setHours(0, 0, 0, 0);
        if (selectedDate > minDate) {
          return { 'underage': true, requiredAge: minAge };
        }
      }
      return null;
    };
  }

  goToNextStep(): void {
    const step1Controls = ['fullName', 'email', 'phoneNumber', 'dateOfBirth', 'password', 'confirmPassword'];
    let isValid = true;

    for (const ctrl of step1Controls) {
      const control = this.registerForm.get(ctrl);
      control?.markAsTouched();
      if (control?.invalid) isValid = false;
    }

    const passwordsMatch = !this.registerForm.errors?.['mismatch'];

    if (!isValid) {
      this.toast.error('Please fill in all personal details correctly.');
      return;
    }
    
    if (!passwordsMatch) {
      this.toast.error('Passwords do not match');
      return;
    }

    this.currentStep = 2;
  }

  goToPrevStep(): void {
    this.currentStep = 1;
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
      dateOfBirth: formValue.dateOfBirth,
      password: formValue.password,
      bankAccountName: formValue.bankAccountName,
      bankAccountNumber: formValue.bankAccountNumber,
      bankIfscCode: formValue.bankIfscCode
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
        this.toast.error(ApiService.getErrorMessage(err));
      }
    });
  }
}
