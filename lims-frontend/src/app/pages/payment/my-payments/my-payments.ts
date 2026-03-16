import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, AuthService, ToastService } from '../../../core/services';
import { PaymentResponse } from '../../../core/models/payment.model';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { StatusBadge } from '../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-my-payments',
  standalone: true,
  imports: [CommonModule, RouterLink, LoadingSpinner, StatusBadge, AppIcon],
  templateUrl: './my-payments.html',
  styleUrl: './my-payments.css'
})
export class MyPayments implements OnInit {
  payments: PaymentResponse[] = [];
  loading = true;

  constructor(
    private api: ApiService,
    private toast: ToastService,
    private auth: AuthService
  ) { }

  get dashboardRoute(): string {
    return this.auth.getDashboardRoute();
  }

  ngOnInit(): void {
    this.api.get<PaymentResponse[]>('payment/my').subscribe({
      next: (res) => {
        this.payments = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toast.show('Failed to load payments.', 'error');
      }
    });
  }

  downloadInvoice(payment: PaymentResponse): void {
    // Generate a simple PDF invoice
    const win = window.open('', '_blank');
    if (!win) return;

    const html = `
      <html>
        <head>
          <title>Invoice - ${payment.transactionReference}</title>
          <style>
            body { font-family: 'Inter', sans-serif; padding: 40px; color: #1e293b; line-height: 1.5; }
            .header { border-bottom: 2px solid #e2e8f0; padding-bottom: 20px; margin-bottom: 30px; display: flex; justify-content: space-between; align-items: flex-end; }
            .logo { font-size: 24px; font-weight: 800; color: #2563eb; }
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
              <div class="logo">LIMS NEXALINK</div>
              <div style="font-size: 12px; color: #64748b; mt-1">AI-Powered Life Insurance Management</div>
            </div>
            <div class="invoice-label">Invoice</div>
          </div>

          <div class="grid">
            <div>
              <div class="label">Billed To</div>
              <div class="value">Customer ID: #RESTRICTED</div>
              <div class="value">Policy: ${payment.policyNumber}</div>
            </div>
            <div style="text-align: right;">
              <div class="label">Transaction ID</div>
              <div class="value">${payment.transactionReference || 'N/A'}</div>
              <div class="label" style="margin-top: 15px;">Date Paid</div>
              <div class="value">${new Date(payment.paymentDate).toLocaleDateString()}</div>
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
                <td>Insurance Premium Payment - ${payment.policyNumber}</td>
                <td style="text-align: right;">₹${payment.amountPaid.toLocaleString()}</td>
              </tr>
              <tr class="total-row">
                <td>Total Amount Paid</td>
                <td style="text-align: right;">₹${payment.amountPaid.toLocaleString()}</td>
              </tr>
            </tbody>
          </table>

          <div style="margin-top: 40px; padding: 20px; background: #f8fafc; border-radius: 12px; font-size: 12px;">
            <div class="label">Payment Method</div>
            <div class="value">${payment.paymentMethod}</div>
            <div class="label" style="margin-top: 10px;">Status</div>
            <div class="value" style="color: #10b981;">SUCCESSFUL</div>
          </div>

          <div class="footer">
            <p>This is a computer-generated document. No signature is required.</p>
            <p>&copy; 2026 LIMS NexaLink Systems. All rights reserved.</p>
          </div>

          <div class="no-print" style="margin-top: 40px; text-align: center;">
            <button onclick="window.print()" style="padding: 12px 24px; background: #2563eb; color: white; border: none; border-radius: 8px; font-weight: 700; cursor: pointer;">Print / Download PDF</button>
          </div>
        </body>
      </html>
    `;

    win.document.write(html);
    win.document.close();
  }

  trackByPaymentId(index: number, payment: PaymentResponse): number {
    return payment.paymentId;
  }
}
