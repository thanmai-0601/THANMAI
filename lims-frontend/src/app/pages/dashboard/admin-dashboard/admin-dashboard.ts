import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AdminDashboardDto, AgentPerformance, PlanDistribution, CustomerDistribution, CustomerPolicyFinancials } from '../../../core/models/dashboard.model';
import { StatCard } from '../../../shared/components/stat-card/stat-card';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { PlanList } from '../../policy/plan-list/plan-list';
import { Chatbot } from '../../../shared/components/chatbot/chatbot';
import { FormsModule } from '@angular/forms';
import {
  NgApexchartsModule,
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexTitleSubtitle,
  ApexYAxis,
  ApexTooltip,
  ApexStroke,
  ApexDataLabels,
  ApexPlotOptions,
  ApexFill,
  ApexLegend,
  ApexGrid
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries | number[];
  chart: ApexChart;
  xaxis?: ApexXAxis;
  yaxis?: ApexYAxis | ApexYAxis[];
  title?: ApexTitleSubtitle;
  labels?: string[];
  stroke?: ApexStroke;
  dataLabels?: ApexDataLabels;
  plotOptions?: ApexPlotOptions;
  fill?: ApexFill;
  tooltip?: ApexTooltip;
  legend?: ApexLegend;
  colors?: string[];
  grid?: ApexGrid;
  markers?: any;
};

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner, PlanList, Chatbot, FormsModule, NgApexchartsModule, AppIcon],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  data: AdminDashboardDto | null = null;
  loading = true;

  public chartOptions: Partial<ChartOptions> = {};
  public agentPerformanceChartOptions: Partial<ChartOptions> = {};
  public customerChartOptions: Partial<ChartOptions> = {};

  constructor(private api: ApiService) {
    this.initChartOptions();
    this.initAgentPerformanceChart();
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  private initChartOptions(): void {
    this.chartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 350,
        animations: {
          enabled: true,
          speed: 800
        },
        fontFamily: 'Inter, sans-serif'
      },
      labels: [], // Replaces xaxis categories for pie/donut
      colors: ['#6366F1', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#3B82F6'], // Expanded color palette
      plotOptions: {
        pie: {
          donut: {
            size: '70%',
            labels: {
              show: true,
              name: {
                show: true,
                fontSize: '14px',
                color: '#94A3B8'
              },
              value: {
                show: true,
                fontSize: '24px',
                fontWeight: 700,
                color: '#1E293B',
                formatter: function (val: any) {
                  return val + " Policies"
                }
              },
              total: {
                show: true,
                showAlways: true,
                label: 'Total Policies',
                fontSize: '14px',
                color: '#94A3B8',
                formatter: function (w: any) {
                  return w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)
                }
              }
            }
          }
        }
      },
      dataLabels: {
        enabled: false // Cleaner to rely on tooltips and the center total
      },
      grid: {
        padding: { top: 0, bottom: 0, left: 0, right: 0 }
      },
      fill: {
        opacity: 0.9,
        type: 'solid'
      },
      legend: {
        show: true,
        position: 'bottom',
        offsetY: 8,
        labels: { colors: '#94A3B8' }
      },
      tooltip: {
        theme: 'dark',
        shared: true,
        intersect: false,
        followCursor: true,
        y: {
          formatter: (val: number, { seriesIndex }: any) => {
            if (seriesIndex === 0) return `${val} Policies`;
            return `₹${val.toLocaleString('en-IN')}`;
          }
        }
      } as any
    };
  }

  private initAgentPerformanceChart(): void {
    this.agentPerformanceChartOptions = {
      chart: {
        height: 350,
        type: 'line',
        toolbar: { show: false },
        animations: {
          enabled: true,
          speed: 800
        },
        dropShadow: {
            enabled: true,
            color: '#000',
            top: 18,
            left: 7,
            blur: 10,
            opacity: 0.2
        },
        fontFamily: 'Inter, sans-serif'
      },
      colors: ['#F97316', '#10B981'],
      stroke: {
        curve: 'smooth',
        width: 3
      },
      dataLabels: {
        enabled: false
      },
      grid: {
        borderColor: 'rgba(148, 163, 184, 0.2)',
        row: {
          colors: ['rgba(148, 163, 184, 0.05)', 'transparent'],
          opacity: 1
        },
        padding: { left: 20, right: 20 }
      },
      xaxis: {
        categories: [],
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: {
          style: {
            colors: '#94A3B8',
            fontSize: '10px',
            fontWeight: 600
          }
        }
      },
      yaxis: [
        {
          min: 0,
          title: {
            text: 'Volume (Policies)',
            style: { color: '#F97316', fontWeight: 600 }
          },
          labels: {
            style: { colors: '#F97316' }
          }
        },
        {
          min: 0,
          opposite: true,
          title: {
            text: 'Yield (₹)',
            style: { color: '#10B981', fontWeight: 600 }
          },
          labels: {
            style: { colors: '#10B981' },
            formatter: (val: number) => `₹${val.toLocaleString()}`
          }
        }
      ] as any,
      markers: {
        size: 5,
        strokeColors: '#fff',
        strokeWidth: 2,
        hover: {
          size: 7
        }
      },
      legend: {
        position: 'top',
        horizontalAlign: 'right',
        floating: true,
        offsetY: -25,
        offsetX: -5,
        labels: {
          colors: '#94A3B8'
        }
      },
      tooltip: {
        theme: 'dark',
        shared: true,
        intersect: false
      }
    };

    this.customerChartOptions = {
      series: [],
      chart: {
        height: 350,
        type: 'bar',
        toolbar: { show: false },
        animations: { enabled: true, speed: 800 },
        fontFamily: 'Inter, sans-serif'
      },
      colors: ['#6366F1', '#EC4899'], // Indigo for Policies, Pink for Claims
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '55%',
          borderRadius: 6,
          dataLabels: { position: 'top' }
        }
      },
      dataLabels: { enabled: false },
      stroke: { show: true, width: 2, colors: ['transparent'] },
      xaxis: {
        categories: [],
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: {
          style: { colors: '#94A3B8', fontSize: '10px', fontWeight: 600 }
        }
      },
      yaxis: [
        {
          title: { text: 'Count' },
          min: 0,
          forceNiceScale: true,
          labels: {
            formatter: (val: number) => Math.floor(val).toString()
          }
        }
      ] as any,
      fill: { opacity: 1 },
      tooltip: {
        theme: 'dark',
        shared: true,
        intersect: false
      },
      legend: {
        position: 'top',
        horizontalAlign: 'right',
        labels: { colors: '#94A3B8' }
      },
      grid: {
        borderColor: 'rgba(148, 163, 184, 0.1)',
        strokeDashArray: 4,
        padding: { left: 0, right: 0 }
      }
    };
  }

  private updateCustomerChart(customerDistribution: CustomerDistribution[]): void {
    const getProp = (obj: any, prop: string) => {
      const pascal = prop.charAt(0).toUpperCase() + prop.slice(1);
      return obj[prop] !== undefined ? obj[prop] : obj[pascal];
    };

    this.customerChartOptions.series = [
      {
        name: 'Policies',
        data: customerDistribution.map(c => getProp(c, 'totalPolicies') || 0)
      },
      {
        name: 'Claims',
        data: customerDistribution.map(c => getProp(c, 'totalClaims') || 0)
      }
    ];

    this.customerChartOptions.xaxis = {
      ...this.customerChartOptions.xaxis,
      categories: customerDistribution.map(c => getProp(c, 'customerName') || 'Unknown')
    };
  }

  public selectedCustomerId: string = 'all';

  onCustomerSelected(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.selectedCustomerId = target.value;
    
    if (this.selectedCustomerId === 'all') {
      if (this.data?.customerDistribution) {
        this.updateCustomerChart(this.data.customerDistribution);
      }
    } else {
      this.loadCustomerFinancials(Number(this.selectedCustomerId));
    }
  }

  private loadCustomerFinancials(customerId: number): void {
    this.api.get<CustomerPolicyFinancials[]>(`dashboard/customer-financials/${customerId}`).subscribe({
      next: (financials) => {
        const getProp = (obj: any, prop: string) => {
          const pascal = prop.charAt(0).toUpperCase() + prop.slice(1);
          return obj[prop] !== undefined ? obj[prop] : obj[pascal];
        };

        this.customerChartOptions.series = [
          {
            name: 'Premium Paid',
            data: financials.map(f => getProp(f, 'totalPremiumPaid') || 0)
          },
          {
            name: 'Claim Received',
            data: financials.map(f => getProp(f, 'totalClaimReceived') || 0)
          }
        ];

        this.customerChartOptions.xaxis = {
          ...this.customerChartOptions.xaxis,
          categories: financials.map(f => `${getProp(f, 'planName')} (${getProp(f, 'policyNumber')})` || 'Unknown Policy')
        };
      },
      error: (err) => {
        console.error('Failed to load customer financials', err);
      }
    });
  }

  loadDashboard(): void {
    this.loading = true;
    this.api.get<AdminDashboardDto>(`dashboard/summary`).subscribe({
      next: (res) => {
        this.data = res;
        this.updateChartSeries(res.planDistribution);
        this.updatePerformanceChart(res.agentPerformance);
        this.updateCustomerChart(res.customerDistribution);
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  private updateChartSeries(planDistribution: PlanDistribution[]): void {
    const getProp = (obj: any, prop: string) => {
      const pascal = prop.charAt(0).toUpperCase() + prop.slice(1);
      return obj[prop] !== undefined ? obj[prop] : obj[pascal];
    };

    // For Donut chart, series must be `number[]` and labels must be `string[]`
    this.chartOptions.series = planDistribution.map(p => getProp(p, 'totalPolicies') || 0);
    this.chartOptions.labels = planDistribution.map(p => `${getProp(p, 'planName') || ''}`);
  }

  private updatePerformanceChart(performanceData: AgentPerformance[]): void {
    this.agentPerformanceChartOptions.series = [
      {
        name: 'Volume (Policies)',
        type: 'line',
        data: performanceData.map(a => a.totalPoliciesAssigned)
      },
      {
        name: 'Yield (Commission)',
        type: 'area',
        data: performanceData.map(a => a.totalCommissionEarned)
      }
    ];
    this.agentPerformanceChartOptions.xaxis = {
      ...this.agentPerformanceChartOptions.xaxis,
      categories: performanceData.map(a => a.agentName)
    };
  }

  getMaxRevenue(): number {
    if (!this.data || !this.data.monthlyRevenue.length) return 0;
    return Math.max(...this.data.monthlyRevenue.map(m => m.premiumCollected), 1);
  }

  getRevenueHeight(amount: number): number {
    const max = this.getMaxRevenue();
    return (amount / max) * 100;
  }
}
