import { Component, OnInit } from '@angular/core';
import { AppIcon } from '../../../shared/components/app-icon/app-icon';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AdminDashboardDto, AgentPerformance } from '../../../core/models/dashboard.model';
import { StatCard } from '../../../shared/components/stat-card/stat-card';
import { LoadingSpinner } from '../../../shared/components/loading-spinner/loading-spinner';
import { PlanList } from '../../policy/plan-list/plan-list';
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
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  title: ApexTitleSubtitle;
  tooltip: ApexTooltip;
  stroke: ApexStroke;
  dataLabels: ApexDataLabels;
  plotOptions: ApexPlotOptions;
  fill: ApexFill;
  legend: ApexLegend;
  grid: ApexGrid;
  colors: string[];
  markers: any;
};

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner, PlanList, FormsModule, NgApexchartsModule, AppIcon],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  data: AdminDashboardDto | null = null;
  loading = true;
  selectedYear: number;
  availableYears: number[] = [];

  public chartOptions: Partial<ChartOptions> = {};
  public agentPerformanceChartOptions: Partial<ChartOptions> = {};

  constructor(private api: ApiService) {
    const currentYear = new Date().getFullYear();
    for (let i = 0; i < 5; i++) {
      this.availableYears.push(currentYear - i);
    }
    this.selectedYear = currentYear;
    this.initChartOptions();
    this.initAgentPerformanceChart();
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  private initChartOptions(): void {
    this.chartOptions = {
      chart: {
        height: 250,
        type: 'bar',
        toolbar: { show: false },
        animations: {
          enabled: true,
          speed: 800
        },
        fontFamily: 'Inter, sans-serif'
      },
      colors: ['#F97316'],
      plotOptions: {
        bar: {
          columnWidth: '45%',
          borderRadius: 6,
          dataLabels: { position: 'top' }
        }
      },
      dataLabels: {
        enabled: false
      },
      grid: {
        show: false,
        padding: { left: 0, right: 0 }
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
      yaxis: {
        show: false
      },
      fill: {
        type: 'gradient',
        gradient: {
          shade: 'light',
          type: 'vertical',
          shadeIntensity: 0.25,
          gradientToColors: ['#FB923C'],
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 1,
          stops: [50, 0, 100, 100]
        }
      },
      tooltip: {
        theme: 'dark',
        y: {
          formatter: (val: number) => `₹${val.toLocaleString()}`
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
  }

  loadDashboard(): void {
    this.loading = true;
    this.api.get<AdminDashboardDto>(`dashboard/summary?year=${this.selectedYear}`).subscribe({
      next: (res) => {
        this.data = res;
        this.updateChartSeries(res.monthlyRevenue);
        this.updatePerformanceChart(res.agentPerformance);
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  private updateChartSeries(revenueData: any[]): void {
    this.chartOptions.series = [{
      name: 'Premium Collected',
      data: revenueData.map(m => m.premiumCollected)
    }];
    this.chartOptions.xaxis = {
      ...this.chartOptions.xaxis,
      categories: revenueData.map(m => m.monthName.substring(0, 3))
    };
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

  onYearChange(): void {
    this.loadDashboard();
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
