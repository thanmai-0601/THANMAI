import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { ToastService } from '../../../core/services/toast';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  user: any = null;
  loading = true;
  editing = false;
  submitting = false;
  today = new Date().toISOString().split('T')[0];
  
  editForm: any = {
    fullName: '',
    phoneNumber: '',
    dateOfBirth: '',
    bankAccountName: '',
    bankAccountNumber: '',
    bankIfscCode: ''
  };

  constructor(
    private auth: AuthService,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.auth.getCurrentUserFromApi().subscribe({
      next: (res) => {
        this.user = res;
        this.loading = false;
        // Sync local edit form
        this.syncEditForm();
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Failed to load profile', 'error');
        this.loading = false;
      }
    });
  }

  private syncEditForm(): void {
    if (!this.user) return;
    const rawDob = this.user.dateOfBirth ? this.user.dateOfBirth.split('T')[0] : '';
    this.editForm = {
      fullName: this.user.fullName,
      phoneNumber: this.user.phoneNumber,
      dateOfBirth: rawDob === '0001-01-01' ? '' : rawDob,
      bankAccountName: this.user.bankAccountName || '',
      bankAccountNumber: this.user.bankAccountNumber || '',
      bankIfscCode: this.user.bankIfscCode || ''
    };
  }

  toggleEdit(): void {
    this.editing = !this.editing;
    if (this.editing) {
      this.syncEditForm();
    }
  }

  saveProfile(): void {
    if (!this.editForm.fullName || !this.editForm.phoneNumber || !this.editForm.dateOfBirth) {
      this.toast.show('Full Name, Phone, and DOB are required.', 'warning');
      return;
    }

    if (new Date(this.editForm.dateOfBirth) >= new Date()) {
      this.toast.show('Date of Birth must be in the past.', 'warning');
      return;
    }

    this.submitting = true;
    this.auth.updateProfile(this.editForm).subscribe({
      next: (res) => {
        this.toast.show('Profile updated successfully!', 'success');
        this.editing = false;
        this.submitting = false;
        this.loadProfile(); // Reload to get fresh data
      },
      error: (err) => {
        this.toast.show(err.error?.message || 'Update failed', 'error');
        this.submitting = false;
      }
    });
  }
}
