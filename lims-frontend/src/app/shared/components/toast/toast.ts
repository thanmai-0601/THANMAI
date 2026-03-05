import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../../core/services/toast';

@Component({
  selector: 'app-toast',
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrl: './toast.css'
})
export class Toast {
  constructor(public toastService: ToastService) {}

  getToastBgClass(type: string): string {
    switch(type) {
      case 'success': return 'bg-success';
      case 'error': return 'bg-error';
      case 'warning': return 'bg-warning';
      case 'info': return 'bg-primary';
      default: return 'bg-slate-800';
    }
  }
}
