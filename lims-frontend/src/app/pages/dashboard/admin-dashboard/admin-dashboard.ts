import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { AdminDashboardDto } from '../../../core/models/dashboard.model';
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
};

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, StatCard, LoadingSpinner, PlanList, FormsModule, NgApexchartsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  data: AdminDashboardDto | null = null;
  loading = true;
  selectedYear: number;
  availableYears: number[] = [];

  public chartOptions: Partial<ChartOptions> = {};

  constructor(private api: ApiService) {
    const currentYear = new Date().getFullYear();
    for (let i = 0; i < 5; i++) {
      this.availableYears.push(currentYear - i);
    }
    this.selectedYear = currentYear;
    this.initChartOptions();
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
          formatter: (val) => `₹${val.toLocaleString()}`
        }
      }
    };
  }

  loadDashboard(): void {
    this.loading = true;
    this.api.get<AdminDashboardDto>(`dashboard/summary?year=${this.selectedYear}`).subscribe({
      next: (res) => {
        this.data = res;
        this.updateChartSeries(res.monthlyRevenue);
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
