import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { ToastService } from '../../../core/services/toast';
import { InvoiceResponse, MakePaymentDto } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';

@Component({
  selector: 'app-make-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinner, AppIcon],
  templateUrl: './make-payment.html',
  styleUrl: './make-payment.css'
})
export class MakePayment implements OnInit {
  invoiceId = 0;
  paymentMethod = 'Card';

  fetchingInvoice = true;
  loading = false;
  paymentSuccess = false;
  isFirstPayment = false;
  invoice: InvoiceResponse | null = null;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService,
    private location: Location
  ) { }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('invoiceId');
    if (id !== null) {
      this.invoiceId = +id;
      this.api.get<InvoiceResponse>(`payment/invoice/${this.invoiceId}`).subscribe({
        next: (res) => {
          this.invoice = res;
          this.fetchingInvoice = false;
          // Activation logic: Typically the first installment (period 1) activates the policy
          this.isFirstPayment = res.periodNumber === 1;
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
        this.loading = false;
        this.paymentSuccess = true;
        this.toast.show('Payment processed successfully!', 'success');
      },
      error: (err: any) => {
        this.loading = false;
        this.toast.show(ApiService.getErrorMessage(err), 'error');
      }
    });
  }

  downloadInvoice(): void {
    if (!this.invoice) return;

    const win = window.open('', '_blank');
    if (!win) return;

    const html = `
      <html>
        <head>
          <title>Invoice - ${this.invoice.invoiceNumber}</title>
          <style>
            body { font-family: 'Inter', sans-serif; padding: 40px; color: #1e293b; line-height: 1.5; }
            .header { border-bottom: 2px solid #e2e8f0; padding-bottom: 20px; margin-bottom: 30px; display: flex; justify-content: space-between; align-items: flex-end; }
            .logo { font-size: 24px; font-weight: 800; color: #f97316; }
            .invoice-label { font-size: 32px; font-weight: 900; text-transform: uppercase; letter-spacing: -0.025em; }
            .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; margin-bottom: 40px; }
            .label { font-size: 10px; font-weight: 800; text-transform: uppercase; color: #64748b; margin-bottom: 4px; }
            .value { font-size: 14px; font-weight: 600; }
            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
            th { text-align: left; padding: 12px; border-bottom: 1px solid #e2e8f0; font-size: 10px; font-weight: 800; text-transform: uppercase; color: #64748b; }
            td { padding: 16px 12px; border-bottom: 1px solid #f1f5f9; font-size: 14px; }
            .total-row td { border-top: 2px solid #e2e8f0; font-weight: 800; font-size: 18px; }
            .footer { margin-top: 60px; text-align: center; font-size: 12px; color: #94a3b8; }
            @media print { .no-print { display: none; } }
          </style>
        </head>
        <body>
          <div class="header">
            <div>
              <div class="logo">NexaLife Core</div>
              <div style="font-size: 12px; color: #64748b; mt-1">Life Insurance Management System</div>
            </div>
            <div class="invoice-label">Invoice</div>
          </div>

          <div class="grid">
            <div>
              <div class="label">Billed To</div>
              <div class="value">Policy Holder</div>
              <div class="value">Policy: ${this.invoice.policyNumber}</div>
            </div>
            <div style="text-align: right;">
              <div class="label">Invoice Number</div>
              <div class="value">${this.invoice.invoiceNumber}</div>
              <div class="label" style="margin-top: 15px;">Date Paid</div>
              <div class="value">${new Date().toLocaleDateString()}</div>
            </div>
          </div>

          <table>
            <thead>
              <tr>
                <th>Description</th>
                <th style="text-align: right;">Amount</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Insurance Premium - Year ${this.invoice.periodYear} / Installment #${this.invoice.periodNumber}</td>
                <td style="text-align: right;">₹${this.invoice.amountDue.toLocaleString()}</td>
              </tr>
              <tr class="total-row">
                <td>Total Amount Paid</td>
                <td style="text-align: right;">₹${this.invoice.amountDue.toLocaleString()}</td>
              </tr>
            </tbody>
          </table>

          <div style="margin-top: 40px; padding: 20px; background: #f8fafc; border-radius: 12px; font-size: 12px;">
            <div class="label">Payment Method</div>
            <div class="value">${this.paymentMethod}</div>
            <div class="label" style="margin-top: 10px;">Status</div>
            <div class="value" style="color: #10b981;">PAID SUCCESSFULLY</div>
          </div>

          <div class="footer">
            <p>This is a computer-generated document. No signature is required.</p>
            <p>&copy; 2026 NexaLife Core. All rights reserved.</p>
          </div>

          <div class="no-print" style="margin-top: 40px; text-align: center;">
            <button onclick="window.print()" style="padding: 12px 24px; background: #f97316; color: white; border: none; border-radius: 8px; font-weight: 700; cursor: pointer;">Print / Download PDF</button>
          </div>
        </body>
      </html>
    `;

    win.document.write(html);
    win.document.close();
  }

  goToDashboard(): void {
    this.router.navigate(['/app/dashboard/customer']);
  }
}
