import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { InvoiceResponse, MakePaymentDto } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-make-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinner],
  templateUrl: './make-payment.html',
  styleUrl: './make-payment.css'
})
export class MakePayment implements OnInit {
  invoiceId = 0;
  paymentMethod = 'Card';

  fetchingInvoice = true;
  loading = false;
  invoice: InvoiceResponse | null = null;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('invoiceId');
    if (id !== null) {
      this.invoiceId = +id;
      this.api.get<InvoiceResponse>(`payment/invoice/${this.invoiceId}`).subscribe({
        next: (res) => {
          this.invoice = res;
          this.fetchingInvoice = false;
        },
        error: () => {
          this.fetchingInvoice = false;
          this.toast.show('Failed to fetch invoice details.', 'warning');
        }
      });
    } else {
      this.fetchingInvoice = false;
    }
  }

  processPayment(): void {
    if (!this.invoice) {
      this.toast.show('No invoice loaded.', 'warning');
      return;
    }

    this.loading = true;
    const payload: MakePaymentDto = {
      invoiceId: this.invoiceId,
      paymentMethod: this.paymentMethod
    };

    this.api.post('payment/pay', payload).subscribe({
      next: () => {
        this.toast.show('Payment processed successfully!', 'success');
        this.router.navigate(['/app/dashboard/customer']);
      },
      error: (err: any) => {
        this.loading = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
    });
  }
}
