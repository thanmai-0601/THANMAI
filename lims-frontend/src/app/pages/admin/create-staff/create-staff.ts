import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { CreateStaffDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-create-staff',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './create-staff.html',
  styleUrl: './create-staff.css'
})
export class CreateStaff {
  staffForm: FormGroup;
  loading = false;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private router: Router,
    private fb: FormBuilder
  ) {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    this.maxDate = today.toISOString().split('T')[0];

    this.staffForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      role: ['Agent', Validators.required],
      dateOfBirth: ['', [Validators.required]],
      bankAccountName: ['', Validators.required],
      bankAccountNumber: ['', [Validators.required, Validators.pattern('^[0-9]{9,20}$')]],
      bankIfscCode: ['', [Validators.required, Validators.pattern('^[A-Z]{4}0[A-Z0-9]{6}$')]]
    });
  }

  maxDate: string;

  submit(): void {
    if (this.staffForm.invalid) {
      this.toast.show('Please fill in all details including password.', 'warning');
      this.staffForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const formValue = this.staffForm.value;
    const payload: CreateStaffDto = {
      fullName: formValue.fullName,
      email: formValue.email,
      phoneNumber: formValue.phoneNumber,
      password: formValue.password,
      dateOfBirth: formValue.dateOfBirth,
      bankAccountName: formValue.bankAccountName,
      bankAccountNumber: formValue.bankAccountNumber,
      bankIfscCode: formValue.bankIfscCode
    };

    const endpoint = `admin/create-staff/${formValue.role}`;

    this.api.post(endpoint, payload).subscribe({
      next: () => {
        this.toast.show(`${formValue.role} account created successfully!`, 'success');
        this.router.navigate(['/app/admin/users']);
      },
      error: (err: any) => {
        this.loading = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
    });
  }
}
