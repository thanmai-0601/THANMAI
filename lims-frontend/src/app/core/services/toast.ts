import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ToastMessage {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toastsSubject = new BehaviorSubject<ToastMessage[]>([]);
  toasts$ = this.toastsSubject.asObservable();
  private counter = 0;

  success(message: string): void { this.show(message, 'success'); }
  error(message: string): void { this.show(message, 'error'); }
  warning(message: string): void { this.show(message, 'warning'); }
  info(message: string): void { this.show(message, 'info'); }

  public show(message: string, type: ToastMessage['type']): void {
    const id = ++this.counter;
    const toast: ToastMessage = { id, message, type };
    const current = this.toastsSubject.value;
    this.toastsSubject.next([...current, toast]);

    // Auto-remove after 4 seconds
    setTimeout(() => this.remove(id), 4000);
  }

  remove(id: number): void {
    const current = this.toastsSubject.value.filter(t => t.id !== id);
    this.toastsSubject.next(current);
  }
}
